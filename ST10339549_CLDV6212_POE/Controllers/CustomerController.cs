using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Models;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _azureFunctionUrl;
        private readonly string _functionKey;

        public CustomerController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _azureFunctionUrl = configuration["AzureFunctionSettings:BaseUrl"];
            _functionKey = configuration["AzureFunctionSettings:CustomerFunctionKey"];
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _httpClient.GetFromJsonAsync<List<Customer>>($"{_azureFunctionUrl}api/customer/Customer?code={_functionKey}");
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"{_azureFunctionUrl}api/customer/Customer?code={_functionKey}", customer);
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
            var customer = await _httpClient.GetFromJsonAsync<Customer>($"{_azureFunctionUrl}api/customer/{partitionKey}/{rowKey}?code={_functionKey}");
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"{_azureFunctionUrl}api/customer/{customer.PartitionKey}/{customer.RowKey}?code={_functionKey}", customer);
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
            var customer = await _httpClient.GetFromJsonAsync<Customer>($"{_azureFunctionUrl}api/customer/{partitionKey}/{rowKey}?code={_functionKey}");
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Customer customer)
        {
            var response = await _httpClient.DeleteAsync($"{_azureFunctionUrl}api/customer/{customer.PartitionKey}/{customer.RowKey}?code={_functionKey}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Failed to delete customer.");
            return View(customer);
        }
    }
}
