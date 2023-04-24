using APIAzure.Models;
using APIAzure.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
namespace APIAzure.Controllers
{
    [ApiController]
    [Route("article/")]
    public class ArticleController : ControllerBase
    {
        private readonly IStorageService<Article> _storageService;
        static int counter;
        string storageUri = "https://account-cosmos-database.table.cosmos.azure.com:443/";
        string accountName = "account-cosmos-database";
        string storageAccountKey = "TiizgSU5MlmusqNnS9iqaStNeOMzWPMwKPxEfIoVQEcTKg4okLx90iYWjDfXzMoDu1QJ88PvzN5qACDbidF3Cg==";
        TableClient tableClient;
        public ArticleController(IStorageService<Article> storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            tableClient = new TableClient(
                new Uri(storageUri),
                "Article",
                new TableSharedKeyCredential(accountName, storageAccountKey));
            try
            {
                counter = Int32.Parse(tableClient.Query<Article>().MaxBy(x => Int32.Parse(x.ArticleId)).ArticleId) + 1;
            }
            catch (Exception ex)
            {
                counter = 0;
            }
        }

        private async Task<bool> VerifyUserPresenceInDB(Article entity)
        {
            bool answer = await _storageService.VerifyPresenceDB(entity) == true;
            return answer;
        }

        [HttpPost]
        [Route("post-test")]
        public async Task<IActionResult> PostTestAsync()
        {
            Article newArticle = new Article("fruit", (++counter).ToString())
            {
                ArticleId = counter.ToString(),
                Category = "fruit",
                Name = "bananes",
                UnitaryPrice = 2
            };
            if(await VerifyUserPresenceInDB(newArticle))
            {
            var createdArticle = await _storageService.UpsertEntityAsync(newArticle);
            return CreatedAtAction(nameof(GetAsync), createdArticle);
            }
            return BadRequest("An article with the same name already exists in the Database.");
        }

        [HttpGet]
        [Route("/articles")]
        public async Task<IActionResult> GetAllArticlesAsync()
        {
            var articles = await _storageService.GetAllEntities();
            return Ok(articles);
        }

        [HttpGet]
        [Route("{id}")]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync(string id)
        {
            try
            {
                return Ok(await _storageService.GetEntityAsync(id));
            }
            catch (Exception ex)
            {
                return BadRequest("Erreur : L\'article d\'id " + id + " n\'existe pas");
            }
        }

        [HttpPost]
        [Route("add/")]
        public async Task<IActionResult> PostAsync([FromBody] ArticleDto articleDto)
        {
            Article entity = new Article()
            {
                ArticleId = counter.ToString(),
                Name = articleDto.Name,
                UnitaryPrice = articleDto.UnitaryPrice,
                Category = articleDto.Category,
                RowKey = counter.ToString(),
                PartitionKey = articleDto.Category,
            };
            if(await VerifyUserPresenceInDB(entity))
            {
            var createdEntity = await _storageService.UpsertEntityAsync(entity);
            return CreatedAtAction(nameof(PostAsync), createdEntity);
            }
            return BadRequest("The article already exists in the Database.");
        }

        [HttpPut]
        [Route("modify/{id}")]
        public async Task<IActionResult> PutAsync( string id ,[FromBody] ArticleDto entity)
        {
            try
            {
                Article article = await _storageService.GetEntityAsync(id);
                article.UnitaryPrice = entity.UnitaryPrice;
                article.Category = entity.Category;
                await _storageService.UpsertEntityAsync(article);
                return NoContent();
            }
            catch
            { 
                return BadRequest("The article doesn't exist in the database");
            }
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            await _storageService.DeleteEntityAsync(id);
            return NoContent();
        }
    }
}
