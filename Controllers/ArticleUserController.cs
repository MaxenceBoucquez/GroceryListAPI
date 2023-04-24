using APIAzure.Models;
using APIAzure.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;

namespace APIAzure.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArticleUserController : ControllerBase
    {
        private readonly IStorageService<ArticleUser> _storageService;
        static int counter;
        string storageUri = "https://account-cosmos-database.table.cosmos.azure.com:443/";
        string accountName = "account-cosmos-database";
        string storageAccountKey = "TiizgSU5MlmusqNnS9iqaStNeOMzWPMwKPxEfIoVQEcTKg4okLx90iYWjDfXzMoDu1QJ88PvzN5qACDbidF3Cg==";
        TableClient tableClient;
        TableClient userTable;
        TableClient articleTable;

        public ArticleUserController(IStorageService<ArticleUser> storageService) 
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            tableClient = new TableClient(
                new Uri(storageUri),
                "ArticleUser",
                new TableSharedKeyCredential(accountName, storageAccountKey));
            userTable = new TableClient(
                new Uri(storageUri),
                "User",
                new TableSharedKeyCredential(accountName, storageAccountKey));
            articleTable = new TableClient(
                new Uri(storageUri),
                "Article",
                new TableSharedKeyCredential(accountName, storageAccountKey));

            try
            {
                counter = Int32.Parse(tableClient.Query<ArticleUser>().MaxBy(x => x.RowKey).ArticleId) + 1;
            }
            catch (Exception ex)
            {
                counter = 0;
            }
        }

        private async Task<bool> VerifyUserPresenceInDB(ArticleUser entity)
        {
            bool answer = await _storageService.VerifyPresenceDB(entity) == true;
            return answer;
        }

        [HttpGet]
        [Route("link/{id}")]
        public async Task<IActionResult> GetLink(string id)
        {
            //try
            //{
                return Ok(await _storageService.GetEntityAsync(id));
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest("Error : The user number \'" + id + "\' probably doesn't exist.");
            //}
        }

        [HttpGet]
        [Route("links")]
        public async Task<IActionResult> GetAllArticleUserAsync()
        {
            var articleUsers = await _storageService.GetAllEntities();
            return Ok(articleUsers);
        }

        [HttpGet]
        [Route("{id}/articles")]
        public async Task<IActionResult> GetUserArticles(string id)
        {
            return Ok(await _storageService.GetEntityAsync(id));
        }

        [HttpPost]
        [Route("{userId}/link/{id}")]
        public async Task<IActionResult> PostAsync(string userId, string id, [FromQuery] int quantity)
        {
            ArticleUser entity = new ArticleUser((++counter).ToString(), userId)
            {
                UserId = userId,
                ArticleId = id,
                Quantity = quantity,
                ArticleUserId = counter.ToString()
            };
            var createdEntity = await _storageService.UpsertEntityAsync(entity);
            return CreatedAtAction(nameof(PostAsync),createdEntity);
        } 
    }
}
