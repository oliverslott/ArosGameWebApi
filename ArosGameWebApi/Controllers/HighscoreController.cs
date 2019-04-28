using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;

namespace ArosGameWebApi.Controllers
{
    //https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-develop-table-dotnet
    //https://docs.microsoft.com/en-us/azure/visual-studio/vs-storage-aspnet5-getting-started-tables
    //https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication
    //https://docs.microsoft.com/en-us/azure/key-vault/tutorial-net-create-vault-azure-web-app
    [Route("api/[controller]")]
    [ApiController]
    public class HighscoreController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public HighscoreController(IConfiguration config)
        {
            configuration = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetAsync()
        {
            var list = new List<Person>();
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("Highscores");
            _ = table.CreateIfNotExistsAsync().Result;
            var query = new TableQuery<HighScoreEntity>();
            TableContinuationToken token = null;
            do
            {
                var resultSegment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;
                foreach(var entity in resultSegment.Results)
                {
                    list.Add(new Person()
                    {
                        Name = entity.PartitionKey,
                        Highscore = Int32.Parse(entity.RowKey)
                    });
                }
            } while (token != null);

            return list;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] PostData postData)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("Highscores");
            _ = table.CreateIfNotExistsAsync().Result;
            var insertoperation = TableOperation.Insert(new HighScoreEntity(postData.Highscore.ToString(), postData.Name));
            table.ExecuteAsync(insertoperation);
        }

        //private string GetStorageKey()
        //{
        //    var azureServiceTokenProvider = new AzureServiceTokenProvider();
        //    var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        //    var key = kv.GetSecretAsync("https://arosgamekeyvault.vault.azure.net/secrets/StorageKey/dca0e168d32d4ab18e955da0c5f34a3d").Result;
        //    return key.Value;
        //}

        public class PostData
        {
            public string Name { get; set; }
            public int Highscore { get; set; }
        }

        public class HighScoreEntity : TableEntity
        {
            public HighScoreEntity()
            {

            }
            public HighScoreEntity(string highscore, string name)
            {
                PartitionKey = name;
                RowKey = highscore;
            }
        }

        public class Person
        {
            public string Name { get; set; }
            public int Highscore { get; set; }
        }
    }
}
