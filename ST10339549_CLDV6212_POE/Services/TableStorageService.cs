using Microsoft.Azure.Cosmos.Table;
using ST10339549_CLDV6212_POE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE.Services
{
    public class TableStorageService
    {
        private readonly CloudTableClient _tableClient;
        private readonly CloudTable _customerTable;
        private readonly CloudTable _productTable;

        public TableStorageService(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            _customerTable = _tableClient.GetTableReference("Customers");
            _productTable = _tableClient.GetTableReference("Products");

            _customerTable.CreateIfNotExists();
            _productTable.CreateIfNotExists();
        }

        // ---------------------- Customer Methods ----------------------

        public async Task AddCustomerAsync(Customer customer)
        {
            var insertOperation = TableOperation.Insert(customer);
            await _customerTable.ExecuteAsync(insertOperation);
        }

        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            var result = await _customerTable.ExecuteAsync(retrieveOperation);
            return result.Result as Customer;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var updateOperation = TableOperation.Replace(customer);
            await _customerTable.ExecuteAsync(updateOperation);
        }

        public async Task DeleteCustomerAsync(Customer customer)
        {
            var deleteOperation = TableOperation.Delete(customer);
            await _customerTable.ExecuteAsync(deleteOperation);
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var query = new TableQuery<Customer>();
            var customerTable = await _customerTable.ExecuteQuerySegmentedAsync(query, null);
            return customerTable.Results;
        }

        // ---------------------- Product Methods ----------------------

        public async Task<List<Product>> GetProductsAsync()
        {
            var query = new TableQuery<Product>();
            var productTable = await _productTable.ExecuteQuerySegmentedAsync(query, null);
            return productTable.Results;
        }

        public async Task AddProductAsync(Product product)
        {
            var insertOperation = TableOperation.Insert(product);
            await _productTable.ExecuteAsync(insertOperation);
        }

        public async Task<Product> GetProductAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                throw new ArgumentNullException("PartitionKey and RowKey must be provided.");

            var retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
            var result = await _productTable.ExecuteAsync(retrieveOperation);
            return result.Result as Product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            var updateOperation = TableOperation.Replace(product);
            await _productTable.ExecuteAsync(updateOperation);
        }

        public async Task DeleteProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            var deleteOperation = TableOperation.Delete(product);
            await _productTable.ExecuteAsync(deleteOperation);
        }

    }
}
