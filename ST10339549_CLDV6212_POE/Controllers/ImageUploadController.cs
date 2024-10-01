using Microsoft.AspNetCore.Mvc;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class ImageUploadController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _azureFunctionUrl = "https://st10339549-azurefunctionapplication.azurewebsites.net/";

        public ImageUploadController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _httpClient.GetFromJsonAsync<List<string>>($"{_azureFunctionUrl}api/list-images");
            return View(images);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = image.OpenReadStream();
                form.Add(new StreamContent(fileStream), "file", image.FileName);

                var response = await _httpClient.PostAsync($"{_azureFunctionUrl}api/upload-image", form);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Image uploaded successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to upload image.";
                }
            }
            else
            {
                TempData["Error"] = "Please select an image to upload.";
            }
            return RedirectToAction("Index");
        }
    }
}
