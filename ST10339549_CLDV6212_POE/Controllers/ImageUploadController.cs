using Microsoft.AspNetCore.Mvc;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class ImageUploadController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ImageUploadController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["AzureFunctionSettings:BaseUrl"];
        }

        public async Task<IActionResult> Index()
        {
            string functionKey = _configuration["AzureFunctionSettings:ListImagesFunctionKey"];
            var images = await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}api/list-images?code={functionKey}");
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

                string functionKey = _configuration["AzureFunctionSettings:UploadImageFunctionKey"];
                var response = await _httpClient.PostAsync($"{_baseUrl}api/upload-image?code={functionKey}", form);
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
