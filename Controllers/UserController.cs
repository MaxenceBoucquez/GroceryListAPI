using APIAzure.Models;
using APIAzure.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
namespace APIAzure.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IStorageService<User> _storageService;
        static int counter;
        string storageUri = "https://account-cosmos-database.table.cosmos.azure.com:443/";
        string accountName = "account-cosmos-database";
        string storageAccountKey = "TiizgSU5MlmusqNnS9iqaStNeOMzWPMwKPxEfIoVQEcTKg4okLx90iYWjDfXzMoDu1QJ88PvzN5qACDbidF3Cg==";
        private TableClient tableClient;
        private TableClient articleUserClient;


        public UserController(IStorageService<User> storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            tableClient = new TableClient(
                new Uri(storageUri),
                "User",
                new TableSharedKeyCredential(accountName, storageAccountKey));

            articleUserClient = new TableClient(
                new Uri(storageUri),
                "ArticleUser",
                new TableSharedKeyCredential(accountName, storageAccountKey));
            try
            {
                counter = Int32.Parse(tableClient.Query<User>().MaxBy(x => Int32.Parse(x.UserId)).UserId);
            }
            catch (Exception ex)
            {
                counter = 0;
            }
        }

        [HttpPost]
        [Route("post-test")]
        public async Task<IActionResult> PostTestAsync()
        {
            User newUser = new User("Alfred", counter.ToString())
            {
                UserId = counter.ToString(),
                UserName = "Alfred",
                FirstName = "Alfred",
                LastName = "Alfred",
                Email = "alfred.alfred@test.com",
                Password = "Alfred"
            };
            counter++;
            if(await VerifyUserPresenceInDB(newUser))
            {
                var createdUser = await _storageService.UpsertEntityAsync(newUser);
                return CreatedAtAction(nameof(GetAsync), createdUser);
            }
            return BadRequest("The user already exists in the Database");
        }

        private async Task<bool> VerifyUserPresenceInDB(User entity)
        {
            bool answer = await _storageService.VerifyPresenceDB(entity) == true;
            return answer;
        }

        [HttpGet]
        [Route("{username}")]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync(string username)
        {
            try
            {
                return Ok(await _storageService.GetEntityAsync(username));
            }
            catch (Exception ex)
            {
                return BadRequest("No user with this username");
            }
        }

        [HttpGet]
        [Route("/users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _storageService.GetAllEntities();
            return Ok(users);
        }


        [HttpPost]
        [Route("/user/new")]
        public async Task<IActionResult> PostAsync([FromBody] UserDto newUser)
        {
            User entity = new User(newUser.FirstName, (++counter).ToString())
            {
                UserId = counter.ToString(),
                UserName = newUser.Username,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Password = newUser.Password,
                PartitionKey = newUser.FirstName,
                RowKey = counter.ToString()
            };  
            if(await VerifyUserPresenceInDB(entity))
            {
                var createdUser = await _storageService.UpsertEntityAsync(entity);
                return CreatedAtAction(nameof(GetAsync), createdUser);
            }
            return BadRequest("The User already exists in the Database.");
        }

        [HttpPost]
        [Route("/user/article/add/{name}")]
        public async Task<IActionResult> AddArticleAsync(string id, Article newArticle)
        {
            var user = await _storageService.GetEntityAsync(id);
            return Ok();
        }

        [HttpPut]
        [Route("/User/modify")]
        public async Task<IActionResult> PutAsync([FromBody] User entity)
        {
            entity.PartitionKey = entity.FirstName;
            entity.RowKey = entity.UserId.ToString();
            await _storageService.UpsertEntityAsync(entity);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(User entity)
        {
            await _storageService.DeleteEntityAsync(entity);
            return NoContent();
        }

        [HttpGet]
        [Route("{id}/articles")]
        public async Task<IActionResult> GetArticles(string id)
        {
            RedirectToRoute("/ArticleUser/{id}");
            return Ok();
        }
    }
}
