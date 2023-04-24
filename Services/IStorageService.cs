using APIAzure.Models;
using Azure;

namespace APIAzure.Services
{
    public interface IStorageService<T>
    {
        Task<T> GetEntityAsync(string id);
        Task<T> UpsertEntityAsync(T entity);
        Task DeleteEntityAsync(T entity);
        Task<Pageable<T>> GetAllEntities();
        Task<bool> VerifyPresenceDB(T entity);
    }
}
   