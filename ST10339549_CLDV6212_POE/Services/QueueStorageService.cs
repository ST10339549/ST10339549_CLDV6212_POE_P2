using Azure.Storage.Queues;
using Newtonsoft.Json;
using ST10339549_CLDV6212_POE.Models;
using System.Text.Json;  

namespace ST10339549_CLDV6212_POE.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(string connectionString)
        {
            _queueClient = new QueueClient(connectionString, "orders");
            _queueClient.CreateIfNotExists();
        }

        public async Task AddOrderMessageAsync(OrderMessage orderMessage)
        {
            string messageJson = JsonConvert.SerializeObject(orderMessage);
            await _queueClient.SendMessageAsync(messageJson);
        }

        public async Task<List<OrderMessage>> GetOrderMessagesAsync()
        {
            var messages = new List<OrderMessage>();
            var response = await _queueClient.ReceiveMessagesAsync(maxMessages: 5);

            foreach (var msg in response.Value)
            {
                var orderDetails = JsonConvert.DeserializeObject<OrderMessage>(msg.MessageText);
                messages.Add(orderDetails);
                await _queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
            }
            return messages;
        }
    }
}
