using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ST10339549_CLDV6212_POE.Models;
using System.Text;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class OrderProcessingController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _azureFunctionBaseUrl;
        private readonly string _orderQueueFunctionKey;

        // Inject HttpClient and configuration to retrieve Azure Function details
        public OrderProcessingController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _azureFunctionBaseUrl = configuration["AzureFunctionSettings:BaseUrl"];
            _orderQueueFunctionKey = configuration["AzureFunctionSettings:OrderQueueFunctionKey"];
        }

        // Fetch and display all order messages from the Azure Queue
        public async Task<IActionResult> Index()
        {
            try
            {
                // URL for getting order messages
                var getUrl = $"{_azureFunctionBaseUrl}api/get-order-messages?code={_orderQueueFunctionKey}";

                var response = await _httpClient.GetAsync(getUrl);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to retrieve order messages.";
                    return View(new List<OrderMessage>());
                }

                // Deserialize the response into a list of order messages
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var messages = JsonConvert.DeserializeObject<List<OrderMessage>>(jsonResponse);

                return View(messages); // Pass the messages to the view
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fetching messages: {ex.Message}";
                return View(new List<OrderMessage>());
            }
        }

        // Add a new order message to the Azure Queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderProcessing(string orderId, string productId, int quantity)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(productId) || quantity <= 0)
            {
                TempData["Error"] = "Invalid order details. Please provide valid inputs.";
                return RedirectToAction(nameof(Index));
            }

            var orderMessage = new OrderMessage
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                Action = "processOrder",
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            try
            {
                // Serialize the order message into JSON
                var messageJson = JsonConvert.SerializeObject(orderMessage);
                var content = new StringContent(messageJson, Encoding.UTF8, "application/json");

                // URL for adding an order message
                var postUrl = $"{_azureFunctionBaseUrl}api/add-order-message?code={_orderQueueFunctionKey}";
                var response = await _httpClient.PostAsync(postUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Order {orderId} for product {productId} added successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to add the order message.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to process the order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete an order message from the Azure Queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(string messageId, string popReceipt)
        {
            if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(popReceipt))
            {
                TempData["Error"] = "Message ID or PopReceipt cannot be null.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // URL for deleting the order message
                var deleteUrl = $"{_azureFunctionBaseUrl}api/delete-order-message/{messageId}?popReceipt={popReceipt}&code={_orderQueueFunctionKey}";
                var response = await _httpClient.DeleteAsync(deleteUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Message with ID {messageId} was deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete the order message.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete the message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
