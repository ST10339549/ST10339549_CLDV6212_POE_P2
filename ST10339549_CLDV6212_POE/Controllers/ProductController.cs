using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Models;
using ST10339549_CLDV6212_POE.Services;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public ProductController()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10339549;AccountKey=2r3eN6egjj4zNt9nF8Bw2zMs7XwNBGnPcCiTgJG1jtDfATA+SeE8xYjqgCEdyFy9XMNHTiV1NPJw+AStGagjiw==;EndpointSuffix=core.windows.net";
            _tableStorageService = new TableStorageService(connectionString);
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetProductsAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _tableStorageService.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _tableStorageService.GetProductAsync(product.PartitionKey, product.RowKey);
                if (existingProduct != null)
                {
                    product.ETag = existingProduct.ETag;
                    await _tableStorageService.UpdateProductAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product != null)
            {
                await _tableStorageService.DeleteProductAsync(product);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
