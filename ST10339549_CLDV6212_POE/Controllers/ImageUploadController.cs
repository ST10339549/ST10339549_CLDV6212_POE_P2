using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace ST10339549_CLDV6212_POE.Controllers
{
    public class ImageUploadController : Controller
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=st10339549;AccountKey=2r3eN6egjj4zNt9nF8Bw2zMs7XwNBGnPcCiTgJG1jtDfATA+SeE8xYjqgCEdyFy9XMNHTiV1NPJw+AStGagjiw==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "images";

        public IActionResult Index()
        {
            var images = GetImageUrls();
            return View(images);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var imageName = Path.GetFileName(image.FileName);
                var blobUrl = await UploadImageToBlobAsync(image, imageName);
                TempData["Message"] = "Image uploaded successfully!";
            }
            else
            {
                TempData["Error"] = "Please select an image to upload.";
            }
            return RedirectToAction("Index");
        }

        public async Task<string> UploadImageToBlobAsync(IFormFile image, string fileName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        private List<string> GetImageUrls()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var images = new List<string>();

            foreach (var blobItem in containerClient.GetBlobs())
            {
                images.Add(containerClient.GetBlobClient(blobItem.Name).Uri.ToString());
            }

            return images;
        }
    }
}
