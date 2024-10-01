using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE_FUNCTION_APP.Functions
{
    public class FileStorageFunction
    {
        private readonly ShareClient _shareClient;

        public FileStorageFunction()
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            _shareClient = new ShareClient(connectionString, "contracts");
            _shareClient.CreateIfNotExists();
        }

        [FunctionName("UploadFileToFileStorage")]
        public async Task<IActionResult> UploadFile(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Upload file to Azure File Share function triggered.");

            var formData = await req.ReadFormAsync();
            var file = formData.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Please upload a valid file.");
            }

            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(file.FileName);

            using var stream = file.OpenReadStream();
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadAsync(stream);

            return new OkObjectResult($"File '{file.FileName}' uploaded successfully.");
        }

        [FunctionName("DeleteFileFromFileStorage")]
        public async Task<IActionResult> DeleteFile(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "delete-file/{fileName}")] HttpRequest req,
        string fileName, ILogger log)
        {
            log.LogInformation($"Delete file '{fileName}' from Azure File Share function triggered.");

            if (string.IsNullOrEmpty(fileName))
            {
                return new BadRequestObjectResult("File name is required.");
            }

            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();

            return new OkObjectResult($"File '{fileName}' deleted successfully.");
        }
    }
}
