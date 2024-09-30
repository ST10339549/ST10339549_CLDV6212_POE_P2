using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Models;
using ST10339549_CLDV6212_POE.Services;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CustomerController()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10339549;AccountKey=2r3eN6egjj4zNt9nF8Bw2zMs7XwNBGnPcCiTgJG1jtDfATA+SeE8xYjqgCEdyFy9XMNHTiV1NPJw+AStGagjiw==;EndpointSuffix=core.windows.net";

            _tableStorageService = new TableStorageService(connectionString);
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetCustomersAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _tableStorageService.AddCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = await _tableStorageService.GetCustomerAsync(customer.PartitionKey, customer.RowKey);
                if (existingCustomer != null)
                {
                    customer.ETag = existingCustomer.ETag;
                    await _tableStorageService.UpdateCustomerAsync(customer);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Customer customer)
        {
            var existingCustomer = await _tableStorageService.GetCustomerAsync(customer.PartitionKey, customer.RowKey);
            if (existingCustomer != null)
            {
                customer.ETag = existingCustomer.ETag;
                await _tableStorageService.DeleteCustomerAsync(customer);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
