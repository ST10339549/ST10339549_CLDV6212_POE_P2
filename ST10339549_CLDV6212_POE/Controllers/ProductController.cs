using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Models;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _azureFunctionUrl;
        private readonly string _functionKey;

        public ProductController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _azureFunctionUrl = configuration["AzureFunctionSettings:BaseUrl"];
            _functionKey = configuration["AzureFunctionSettings:ProductFunctionKey"];
        }

        public async Task<IActionResult> Index()
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>($"{_azureFunctionUrl}api/product/Product?code={_functionKey}");
            return View(products);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync($"{_azureFunctionUrl}api/product?code={_functionKey}", product);
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
            var product = await _httpClient.GetFromJsonAsync<Product>($"{_azureFunctionUrl}api/product/{partitionKey}/{rowKey}?code={_functionKey}");
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"{_azureFunctionUrl}api/product/{product.PartitionKey}/{product.RowKey}?code={_functionKey}", product);
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
            var product = await _httpClient.GetFromJsonAsync<Product>($"{_azureFunctionUrl}api/product/{partitionKey}/{rowKey}?code={_functionKey}");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var response = await _httpClient.DeleteAsync($"{_azureFunctionUrl}api/product/{partitionKey}/{rowKey}?code={_functionKey}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Failed to delete product.");
            return View();
        }
    }
}
