using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RedisApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IConfiguration _configuration;
        private string _containerName = "data";
        private readonly IDistributedCache distributedCache;
        public ValuesController(IConfiguration configuration, IDistributedCache distributedCache)
        {
            this._configuration = configuration;
            this.distributedCache = distributedCache;
        }
        public string getConnection()
        {
            var _connectionstring = _configuration.GetConnectionString("storageConnection");
            return _connectionstring;

        }
        private string GetFromStorageAsync()
        {
            string storageAccount_connection = "";
            BlobServiceClient blobServiceClient = new BlobServiceClient(getConnection());
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient("FileA.txt");
            MemoryStream memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);
            memoryStream.Position = 0;
            string content = new StreamReader(memoryStream).ReadToEnd();
            return content;
        }
        
        public async Task<string> Getvalue()
        {
            var valueFromCache = await distributedCache.GetStringAsync("key_1");
            if (!string.IsNullOrEmpty(valueFromCache))
            {
                return valueFromCache;
            }
            else
            {
                string valueFromStorage = GetFromStorageAsync();
                await distributedCache.SetStringAsync("key_1", valueFromStorage);
                return valueFromStorage;
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            var values = Getvalue();
            BlobServiceClient blobServiceClient = new BlobServiceClient(getConnection());
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient("FileA.txt");
            MemoryStream memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);
            memoryStream.Position = 0;
            string content = new StreamReader(memoryStream).ReadToEnd();
            return Ok();
        }

    }
}
