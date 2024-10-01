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
    public static class ProductStorageFunction
    {
        [FunctionName("ProductStorageFunction")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", "put", "delete", "get", Route = "product/{partitionKey?}/{rowKey?}")] HttpRequest req,
        string partitionKey,
        string rowKey,
        ILogger log)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable productTable = tableClient.GetTableReference("Products");

            await productTable.CreateIfNotExistsAsync();

            string method = req.Method.ToUpper();
            switch (method)
            {
                case "POST":
                    return await CreateProduct(req, productTable, log);
                case "GET":
                    return await GetProduct(partitionKey, rowKey, productTable, log);
                case "PUT":
                    return await UpdateProduct(req, partitionKey, rowKey, productTable, log);
                case "DELETE":
                    return await DeleteProduct(partitionKey, rowKey, productTable, log);
                default:
                    return new BadRequestObjectResult("Invalid request method.");
            }
        }

        // CREATE product (POST)
        private static async Task<IActionResult> CreateProduct(HttpRequest req, CloudTable productTable, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonConvert.DeserializeObject<Product>(requestBody);

            if (product == null)
            {
                return new BadRequestObjectResult("Invalid product data.");
            }

            if (string.IsNullOrEmpty(product.ProductId))
            {
                product.ProductId = Guid.NewGuid().ToString();
                product.RowKey = product.ProductId;
            }
            else
            {
                product.RowKey = product.ProductId;
            }

            product.PartitionKey = "Product";

            TableOperation insertOperation = TableOperation.Insert(product);
            await productTable.ExecuteAsync(insertOperation);

            log.LogInformation($"Product '{product.ProductName}' created successfully.");
            return new OkObjectResult($"Product '{product.ProductName}' stored successfully.");
        }

        // GET product by PartitionKey and RowKey or all in Partition (GET)
        private static async Task<IActionResult> GetProduct(string partitionKey, string rowKey, CloudTable productTable, ILogger log)
        {
            if (!string.IsNullOrEmpty(rowKey))
            {
                // Retrieve single product
                TableOperation retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
                TableResult result = await productTable.ExecuteAsync(retrieveOperation);
                Product product = result.Result as Product;

                if (product == null)
                {
                    return new NotFoundObjectResult($"Product with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
                }

                return new OkObjectResult(product);
            }
            else if (!string.IsNullOrEmpty(partitionKey))
            {
                // Retrieve all products in the partition
                TableQuery<Product> query = new TableQuery<Product>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                var products = await productTable.ExecuteQuerySegmentedAsync(query, null);
                return new OkObjectResult(products.Results);
            }
            else
            {
                return new BadRequestObjectResult("PartitionKey must be provided.");
            }
        }

        // UPDATE product (PUT)
        private static async Task<IActionResult> UpdateProduct(HttpRequest req, string partitionKey, string rowKey, CloudTable productTable, ILogger log)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("PartitionKey and RowKey must be provided.");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedProduct = JsonConvert.DeserializeObject<Product>(requestBody);

            if (updatedProduct == null)
            {
                return new BadRequestObjectResult("Invalid product data.");
            }

            // Retrieve existing product
            TableOperation retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
            TableResult result = await productTable.ExecuteAsync(retrieveOperation);
            var existingProduct = result.Result as Product;

            if (existingProduct == null)
            {
                return new NotFoundObjectResult($"Product with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
            }

            // Update product data
            existingProduct.ProductName = updatedProduct.ProductName ?? existingProduct.ProductName;
            existingProduct.ProductDescription = updatedProduct.ProductDescription ?? existingProduct.ProductDescription;
            existingProduct.ProductPrice = updatedProduct.ProductPrice != 0 ? updatedProduct.ProductPrice : existingProduct.ProductPrice;

            TableOperation updateOperation = TableOperation.Replace(existingProduct);
            await productTable.ExecuteAsync(updateOperation);

            log.LogInformation($"Product '{existingProduct.ProductName}' updated successfully.");
            return new OkObjectResult($"Product '{existingProduct.ProductName}' updated successfully.");
        }

        // DELETE product (DELETE)
        private static async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, CloudTable productTable, ILogger log)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("PartitionKey and RowKey must be provided.");
            }

            // Retrieve the product
            TableOperation retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
            TableResult result = await productTable.ExecuteAsync(retrieveOperation);
            var productToDelete = result.Result as Product;

            if (productToDelete == null)
            {
                return new NotFoundObjectResult($"Product with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
            }

            // Delete the product
            TableOperation deleteOperation = TableOperation.Delete(productToDelete);
            await productTable.ExecuteAsync(deleteOperation);

            log.LogInformation($"Product '{productToDelete.ProductName}' deleted successfully.");
            return new OkObjectResult($"Product '{productToDelete.ProductName}' deleted successfully.");
        }
    }
}
