using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ST10339549_CLDV6212_POE.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE_FUNCTION_APP.Functions
{
    public static class CustomerStorageFunction
    {
        [FunctionName("CustomerStorageFunction")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", "delete", "get", Route = "customer/{partitionKey?}/{rowKey?}")] HttpRequest req,
        string partitionKey,
        string rowKey,
        ILogger log)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable customerTable = tableClient.GetTableReference("Customers");

            await customerTable.CreateIfNotExistsAsync();

            string method = req.Method.ToUpper();
            switch (method)
            {
                case "POST":
                    return await CreateCustomer(req, customerTable, log);
                case "GET":
                    return await GetCustomer(partitionKey, rowKey, customerTable, log);
                case "PUT":
                    return await UpdateCustomer(req, partitionKey, rowKey, customerTable, log);
                case "DELETE":
                    return await DeleteCustomer(partitionKey, rowKey, customerTable, log);
                default:
                    return new BadRequestObjectResult("Invalid request method.");
            }
        }

        private static async Task<IActionResult> CreateCustomer(HttpRequest req, CloudTable customerTable, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(requestBody);

            if (customer == null)
            {
                return new BadRequestObjectResult("Invalid customer data.");
            }

            if (string.IsNullOrEmpty(customer.CustomerId))
            {
                customer.CustomerId = Guid.NewGuid().ToString();
                customer.RowKey = customer.CustomerId;
            }

            TableOperation insertOperation = TableOperation.Insert(customer);
            await customerTable.ExecuteAsync(insertOperation);

            log.LogInformation($"Customer '{customer.CustomerName}' created successfully.");
            return new OkObjectResult($"Customer '{customer.CustomerName}' stored successfully.");
        }

        private static async Task<IActionResult> GetCustomer(string partitionKey, string rowKey, CloudTable customerTable, ILogger log)
        {
            if (!string.IsNullOrEmpty(rowKey))
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
                TableResult result = await customerTable.ExecuteAsync(retrieveOperation);
                Customer customer = result.Result as Customer;

                if (customer == null)
                {
                    return new NotFoundObjectResult($"Customer with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
                }

                return new OkObjectResult(customer);
            }
            else if (!string.IsNullOrEmpty(partitionKey))
            {
                TableQuery<Customer> query = new TableQuery<Customer>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                var customers = await customerTable.ExecuteQuerySegmentedAsync(query, null);
                return new OkObjectResult(customers.Results);
            }
            else
            {
                return new BadRequestObjectResult("PartitionKey must be provided.");
            }
        }

        private static async Task<IActionResult> UpdateCustomer(HttpRequest req, string partitionKey, string rowKey, CloudTable customerTable, ILogger log)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("PartitionKey and RowKey must be provided.");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedCustomer = JsonConvert.DeserializeObject<Customer>(requestBody);

            if (updatedCustomer == null)
            {
                return new BadRequestObjectResult("Invalid customer data.");
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            TableResult result = await customerTable.ExecuteAsync(retrieveOperation);
            var existingCustomer = result.Result as Customer;

            if (existingCustomer == null)
            {
                return new NotFoundObjectResult($"Customer with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
            }

            existingCustomer.CustomerName = updatedCustomer.CustomerName ?? existingCustomer.CustomerName;
            existingCustomer.CustomerSurname = updatedCustomer.CustomerSurname ?? existingCustomer.CustomerSurname;
            existingCustomer.CustomerEmail = updatedCustomer.CustomerEmail ?? existingCustomer.CustomerEmail;
            existingCustomer.CustomerAddress = updatedCustomer.CustomerAddress ?? existingCustomer.CustomerAddress;

            TableOperation updateOperation = TableOperation.Replace(existingCustomer);
            await customerTable.ExecuteAsync(updateOperation);

            log.LogInformation($"Customer '{existingCustomer.CustomerName}' updated successfully.");
            return new OkObjectResult($"Customer '{existingCustomer.CustomerName}' updated successfully.");
        }

        private static async Task<IActionResult> DeleteCustomer(string partitionKey, string rowKey, CloudTable customerTable, ILogger log)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("PartitionKey and RowKey must be provided.");
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            TableResult result = await customerTable.ExecuteAsync(retrieveOperation);
            var customerToDelete = result.Result as Customer;

            if (customerToDelete == null)
            {
                return new NotFoundObjectResult($"Customer with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
            }


            TableOperation deleteOperation = TableOperation.Delete(customerToDelete);
            await customerTable.ExecuteAsync(deleteOperation);

            log.LogInformation($"Customer '{customerToDelete.CustomerName}' deleted successfully.");
            return new OkObjectResult($"Customer '{customerToDelete.CustomerName}' deleted successfully.");
        }
    }
}
