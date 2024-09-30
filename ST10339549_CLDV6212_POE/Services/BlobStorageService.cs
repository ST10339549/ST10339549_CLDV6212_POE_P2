using Azure.Storage.Blobs;

namespace ST10339549_CLDV6212_POE.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorageService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient("images");
            _blobContainerClient. CreateIfNotExists();
        }

        public async Task UploadFileAsync(IFormFile formFile)
        {
            var blobClient = _blobContainerClient.GetBlobClient(formFile.FileName);
            using var stream = formFile.OpenReadStream();
            await blobClient.UploadAsync(stream, true);
        }

        public async Task<List<string>> ListFilesAsync()
        {
            var files = new List<string>();
            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync())
            {
                files.Add(blobItem.Name);
            }
            return files; 
        }
    }
}
