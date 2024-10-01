using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Models;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _azureFunctionUrl = "https://st10339549-azurefunctionapplication.azurewebsites.net/";

        public ProductController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>($"{_azureFunctionUrl}GetProducts");
            return View(products);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"{_azureFunctionUrl}ProductStorageFunction", product);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Failed to create product.");
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _httpClient.GetFromJsonAsync<Product>($"{_azureFunctionUrl}GetProduct?partitionKey={partitionKey}&rowKey={rowKey}");
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"{_azureFunctionUrl}UpdateProduct", product);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Failed to update product.");
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var product = await _httpClient.GetFromJsonAsync<Product>($"{_azureFunctionUrl}GetProduct?partitionKey={partitionKey}&rowKey={rowKey}");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var response = await _httpClient.DeleteAsync($"{_azureFunctionUrl}DeleteProduct?partitionKey={partitionKey}&rowKey={rowKey}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Failed to delete product.");
            return View();
        }
    }
}
