using Azure.Storage.Files.Shares;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ST10339549_CLDV6212_POE.Services
{
    public class FileStorageService
    {
        private readonly ShareClient _share;

        public FileStorageService(string connectionString)
        {
            _share = new ShareClient(connectionString, "contracts");
            _share.CreateIfNotExists();
        }

        public async Task UploadAsync(IFormFile formFile)
        {
            var directory = _share.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(formFile.FileName);

            using var readStream = formFile.OpenReadStream();

            await fileClient.CreateAsync(readStream.Length);
            await fileClient.UploadAsync(readStream);
        }

        public async Task<List<string>> FilesAsync()
        {
            var files = new List<string>();
            var directory = _share.GetRootDirectoryClient();
            await foreach (var item in directory.GetFilesAndDirectoriesAsync())
            {
                var fileUrl = $"{_share.Uri}/{item.Name}";
                files.Add(fileUrl);
            }
            return files;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var directory = _share.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
    }
}
