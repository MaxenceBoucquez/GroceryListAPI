using Azure;
using Azure.Data.Tables;
using Microsoft.EntityFrameworkCore;

namespace APIAzure.Models
{
    public class Article : ITableEntity
    {

        public Article(string category, string id)
        {
            this.PartitionKey = category;
            this.RowKey = id;
        }

        public Article()
        {}

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string ArticleId { get; set; }
        public string Category { get; set; } = "";
        public string Name { get; set; } = "";
        public int UnitaryPrice { get; set; } = 0;
        public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
        public ETag ETag { get; set; }


        public override string ToString()
        {
            return "The article is '" + Name + "' and is part of the category '" + Category + "'. It costs " + UnitaryPrice + " alone.";
        }
    }

        
}
