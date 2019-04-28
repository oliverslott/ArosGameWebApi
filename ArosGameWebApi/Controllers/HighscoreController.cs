using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using ArosGameWebApi.Models;

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

        [HttpGet]
        public ActionResult<IEnumerable<Person>> Get()
        {
            var list = new List<Person>();
            var query = new TableQuery<HighScoreEntity>();
            TableContinuationToken token = null;
            do
            {
                var resultSegment = GetHighscoreTable().ExecuteQuerySegmentedAsync(query, token).Result;
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

        [HttpPost]
        public void Post([FromBody] Person person)
        {
            var insertoperation = TableOperation.Insert(new HighScoreEntity(person.Highscore.ToString(), person.Name));
            GetHighscoreTable().ExecuteAsync(insertoperation);
        }

        private CloudTable GetHighscoreTable()
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("Highscores");
            _ = table.CreateIfNotExistsAsync().Result;
            return table;
        }
    }
}
