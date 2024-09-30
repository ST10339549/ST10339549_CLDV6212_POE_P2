using Microsoft.AspNetCore.Mvc;
using ST10339549_CLDV6212_POE.Services;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class FileStorageController : Controller
    {
        private readonly FileStorageService _fileStorageService;

        public FileStorageController()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10339549;AccountKey=2r3eN6egjj4zNt9nF8Bw2zMs7XwNBGnPcCiTgJG1jtDfATA+SeE8xYjqgCEdyFy9XMNHTiV1NPJw+AStGagjiw==;EndpointSuffix=core.windows.net";
            _fileStorageService = new FileStorageService(connectionString);
        }

        public async Task<IActionResult> Index()
        {
            var files = await _fileStorageService.FilesAsync();
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile formFile)
        {
            if (formFile != null)
            {
                await _fileStorageService.UploadAsync(formFile);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            ViewBag.FileName = fileName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                await _fileStorageService.DeleteFileAsync(fileName);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
