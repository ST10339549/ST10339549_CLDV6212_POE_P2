using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE_FUNCTION_APP.Functions
{
    public static class BlobStorageFunction
    {
        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureBlobStorageConnectionString");
        private static readonly string containerName = "images";

        [FunctionName("UploadImageFunction")]
        public static async Task<IActionResult> UploadImage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload-image")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function to upload an image.");

            var file = req.Form.Files["file"];
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Please upload a file.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return new OkObjectResult($"File uploaded successfully: {blobClient.Uri}");
        }

        [FunctionName("ListImagesFunction")]
        public static async Task<IActionResult> ListImages(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "list-images")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function to list images.");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            var images = new List<string>();
            await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
            {
                images.Add(blobContainerClient.GetBlobClient(blobItem.Name).Uri.ToString());
            }

            return new OkObjectResult(images);
        }
    }
}
