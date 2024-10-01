using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Models;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _azureFunctionUrl = "https://st10339549-azurefunctionapplication.azurewebsites.net/";

        public CustomerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _httpClient.GetFromJsonAsync<List<Customer>>($"{_azureFunctionUrl}GetCustomers");
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"{_azureFunctionUrl}CustomerStorageFunction", customer);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create customer.");
                }
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _httpClient.GetFromJsonAsync<Customer>($"{_azureFunctionUrl}GetCustomer?partitionKey={partitionKey}&rowKey={rowKey}");
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"{_azureFunctionUrl}UpdateCustomer", customer);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Failed to update customer.");
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _httpClient.GetFromJsonAsync<Customer>($"{_azureFunctionUrl}GetCustomer?partitionKey={partitionKey}&rowKey={rowKey}");
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Customer customer)
        {
            var response = await _httpClient.DeleteAsync($"{_azureFunctionUrl}DeleteCustomer?partitionKey={customer.PartitionKey}&rowKey={customer.RowKey}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Failed to delete customer.");
            return View(customer);
        }
    }
}
