namespace StarterApp.Database.Data.Repositories;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(); // gets all records of this type
    Task<T?> GetByIdAsync(int id); // gets one record by ID, or null if it does not exist
    Task<T> CreateAsync(T entity); // creates a new record
    Task UpdateAsync(T entity); // updates an existing record
    Task DeleteAsync(int id); // deletes a record by ID
}