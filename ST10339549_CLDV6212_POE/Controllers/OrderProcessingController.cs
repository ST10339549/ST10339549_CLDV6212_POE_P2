using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ST10339549_CLDV6212_POE.Models;
using ST10339549_CLDV6212_POE.Services;
using System;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class OrderProcessingController : Controller
    {
        private readonly QueueStorageService _queueService;

        public OrderProcessingController()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10339549;AccountKey=2r3eN6egjj4zNt9nF8Bw2zMs7XwNBGnPcCiTgJG1jtDfATA+SeE8xYjqgCEdyFy9XMNHTiV1NPJw+AStGagjiw==;EndpointSuffix=core.windows.net";
            _queueService = new QueueStorageService(connectionString);
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _queueService.GetOrderMessagesAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> OrderProcessing(string orderId, string productId, int quantity)
        {
            if (!string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(productId) && quantity > 0)
            {
                var orderMessage = new OrderMessage
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Action = "processOrder",
                    Timestamp = DateTime.UtcNow.ToString("o")
                };

                await _queueService.AddOrderMessageAsync(orderMessage);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
