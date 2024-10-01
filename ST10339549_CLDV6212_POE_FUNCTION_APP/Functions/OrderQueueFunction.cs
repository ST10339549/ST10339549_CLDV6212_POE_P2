using Azure.Storage.Queues; // Required for working with Azure Storage Queues
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ST10339549_CLDV6212_POE.Models; // Assuming this is where your OrderMessage model is
using System;
using System.Collections.Generic;
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

        [FunctionName("GetOrderMessagesFunction")]
        public static async Task<IActionResult> GetOrderMessages(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "get-order-messages")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetOrderMessages function triggered.");

            try
            {
                // Connect to Azure Queue Storage
                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                QueueClient queueClient = new QueueClient(connectionString, "orders");

                // Ensure the queue exists
                if (!await queueClient.ExistsAsync())
                {
                    return new NotFoundObjectResult("Queue not found.");
                }

                // Retrieve messages from the queue
                QueueMessage[] retrievedMessages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);

                var orderMessages = new List<OrderMessage>();

                // Loop through the messages, deserialize, and add to list
                foreach (var message in retrievedMessages)
                {
                    var orderMessage = JsonConvert.DeserializeObject<OrderMessage>(message.MessageText);
                    orderMessages.Add(orderMessage);

                    // Delete the message after processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }

                return new OkObjectResult(orderMessages);
            }
            catch (Exception ex)
            {
                log.LogError($"Error retrieving order messages: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
