using Azure;
using Azure.Data.Tables;

namespace APIAzure.Models
{
    public class ArticleUser : ITableEntity
    {
        public ArticleUser()
        {
        }
        public string ArticleUserId { get; set; }
        public string ArticleId { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
        public ETag ETag { get; set; }

        public ArticleUser(string userId, string articleUserId)
        {
            this.PartitionKey = userId;
            this.RowKey = articleUserId;
        }
    }
}
