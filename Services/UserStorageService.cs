using APIAzure.Models;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Serialization;

namespace APIAzure.Services
{
    public class UserStorageService : IStorageService<User>
    {
        private const string TableName = "User";
        private readonly IConfiguration _configuration;
        private TableServiceClient serviceClient;
        public UserStorageService(IConfiguration configuration)
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

        public async Task<Pageable<User>> GetAllEntities()
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<User>();
            return query;
        }


        public async Task<User?> GetEntityAsync(string username)
        {
           
            var tableClient = await GetTableClient();
            var query = tableClient.Query<User>()
                .Where(x => x.UserName == username)
                .Select(x => new User() { PartitionKey = x.PartitionKey, RowKey = x.RowKey, UserId = x.UserId, Password = x.Password, Email = x.Email, FirstName = x.FirstName, LastName = x.LastName, ETag = x.ETag, Timestamp = x.Timestamp });
                query.ToList();
            return await tableClient.GetEntityAsync<User>(query.FirstOrDefault().PartitionKey, query.FirstOrDefault().RowKey);
        }


        public async Task<User?> UpsertEntityAsync(User entity)
        {
            var tableClient = await GetTableClient();
            await tableClient.UpsertEntityAsync(entity);
            return entity;
        }
        public async Task DeleteEntityAsync(User entity)
        {
            if (await VerifyPresenceDB(entity))
            {
                var tableClient = await GetTableClient();
                var query = tableClient.Query<User>()
                    .Where(x => x.UserName == entity.UserName);
                await tableClient.DeleteEntityAsync(query.FirstOrDefault().PartitionKey, query.FirstOrDefault().RowKey);
            }
        }

        public async Task<bool> VerifyPresenceDB(User entity)
        {
            var tableClient = await GetTableClient();
            var query = tableClient.Query<User>()
                .Where(x=> x.UserName == entity.UserName);

            return query.Count() == 0;
        }
    }
}
