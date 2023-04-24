using Azure;
using Azure.Data.Tables;

namespace APIAzure.Models
{
    public class User : ITableEntity
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string Email { get; set; } = "email";
        public string Password { get; set; } = "password";
        public string PartitionKey { get; set; } 
        public string RowKey { get; set; } 
        public DateTimeOffset? Timestamp { get; set; } = DateTime.Now;
        public ETag ETag { get; set; } 
        public User()
        {
             
        }

        public User(string firstName, string userName)
        {
            this.PartitionKey = firstName;
            this.RowKey = userName;
        }
    }
}
