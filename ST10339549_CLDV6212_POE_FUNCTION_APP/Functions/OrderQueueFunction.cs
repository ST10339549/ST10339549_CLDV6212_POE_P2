using Azure.Storage.Queues; // Required for working with Azure Storage Queues
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ST10339549_CLDV6212_POE.Models; // Assuming this is where your OrderMessage model is
using System;
using System.IO;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE_FUNCTION_APP.Functions
{
    public static class AddOrderMessageFunction
    {
        // Define the function name as AddOrderMessage
        [FunctionName("OrderQueueFunction")]
        public static async Task<IActionResult> AddOrderMessage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "add-order-message")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AddOrderMessage function triggered.");

            // Parse the incoming order message from the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderMessage = JsonConvert.DeserializeObject<OrderMessage>(requestBody);

            if (orderMessage == null || string.IsNullOrEmpty(orderMessage.OrderId) || string.IsNullOrEmpty(orderMessage.ProductId) || orderMessage.Quantity <= 0)
            {
                log.LogWarning("Invalid order message received.");
                return new BadRequestObjectResult("Invalid order message. Please provide valid orderId, productId, and quantity.");
            }

            try
            {
                // Connect to Azure Queue Storage
                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                QueueClient queueClient = new QueueClient(connectionString, "orders");

                // Ensure the queue exists
                await queueClient.CreateIfNotExistsAsync();

                // Serialize the order message to JSON and add it to the queue
                string orderMessageJson = JsonConvert.SerializeObject(orderMessage);
                await queueClient.SendMessageAsync(orderMessageJson);

                log.LogInformation($"Order message for OrderId {orderMessage.OrderId} has been added to the queue.");
                return new OkObjectResult($"Order {orderMessage.OrderId} for product {orderMessage.ProductId} added to the queue successfully.");
            }
            catch (System.Exception ex)
            {
                log.LogError($"Error adding order message to the queue: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
