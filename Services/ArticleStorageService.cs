using APIAzure.Models;
using Azure;
using Azure.Data.Tables;

namespace APIAzure.Services
{
    public class ArticleStorageService : IStorageService<Article>
    {
        private const string TableName = "Article";
        private readonly IConfiguration _configuration;
        private TableServiceClient serviceClient;
        public ArticleStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceClient = new TableServiceClient(_configuration["StorageCollectionString"]);
        }

        private async Task<TableClient> GetTableClient()
        {
            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task<Pageable<Article>> GetAllEntities()
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<Article>();
            return query;
        }

        public async Task<Article> GetEntityAsync(string id)
        {
            var tableClient = await GetTableClient();
            var linqQuery = tableClient.Query<Article>()
                .Where(x =>x.ArticleId == id)
                .Select(x => new Article() { PartitionKey = x.PartitionKey, RowKey = x.RowKey, UnitaryPrice = x.UnitaryPrice, ArticleId = x.ArticleId, Category = x.Category, Name = x.Name});
            var queryList = linqQuery.ToList();
            return await tableClient.GetEntityAsync<Article>(linqQuery.FirstOrDefault().Category, linqQuery.FirstOrDefault().ArticleId.ToString());
        }

        public async Task<Article> UpsertEntityAsync(Article entity)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpsertEntityAsync(entity);
            return entity;
        }
        public async Task DeleteEntityAsync(Article entity)
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<Article>()
                .Where(x => x.RowKey == entity.RowKey);
            await tableClient.DeleteEntityAsync(query.FirstOrDefault().PartitionKey, query.FirstOrDefault().RowKey);
        }

        public async Task<bool> VerifyPresenceDB(Article entity)
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<Article>()
                .Where(x => x.Name == entity.Name);
            return query.Count() == 0;
        }
    }
}
