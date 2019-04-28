using Microsoft.WindowsAzure.Storage.Table;

namespace ArosGameWebApi.Models
{
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
}
