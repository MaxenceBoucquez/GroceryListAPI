using Azure.Data.Tables;
using Azure;
using APIAzure.Models;

namespace APIAzure.Services
{
    public class ArticleUserStorageService : IStorageService<ArticleUser>
    {
        private const string TableName = "ArticleUser";
        private readonly IConfiguration _configuration;
        private TableServiceClient serviceClient;
        public ArticleUserStorageService(IConfiguration configuration)
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

        public async Task<Pageable<ArticleUser>> GetAllEntities()
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<ArticleUser>();
            return query;
        }

        public async Task<ArticleUser> GetEntityAsync(string id)
        {
            var tableClient = await GetTableClient();
            var linqQuery = tableClient.Query<ArticleUser>()
                .Where(x => x.ArticleUserId == id)
                .Select(x => new ArticleUser() { PartitionKey = x.PartitionKey, RowKey = x.RowKey, ArticleId = x.ArticleId, ArticleUserId = x.ArticleUserId, Quantity = x.Quantity, UserId = x.UserId })
                .ToList();
            return await tableClient.GetEntityAsync<ArticleUser>(linqQuery.FirstOrDefault().ArticleUserId, linqQuery.FirstOrDefault().ArticleId);
        }

        public async Task<Pageable<ArticleUser>> GetUserArticles(string id)
        {
            var tableClient = await GetTableClient();
            tableClient.Query<ArticleUser>()
                .Where(x => x.UserId == id);
            return (Pageable<ArticleUser>)tableClient.Query<ArticleUser>().Where(x => x.UserId == id);
        }


        public async Task<ArticleUser> UpsertEntityAsync(ArticleUser entity)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpsertEntityAsync(entity);
            return entity;
        }
        public async Task DeleteEntityAsync(ArticleUser entity)
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<ArticleUser>()
                .Where(x => x.RowKey == entity.RowKey);
            await tableClient.DeleteEntityAsync(query.FirstOrDefault().PartitionKey, query.FirstOrDefault().RowKey);
        }

        public async Task<bool> VerifyPresenceDB(ArticleUser entity)
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<ArticleUser>()
                .Where(x => x.UserId == entity.UserId)
                .Where(x => x.ArticleId == entity.ArticleId);
            return query.Count() == 0;
        }
    }
}
