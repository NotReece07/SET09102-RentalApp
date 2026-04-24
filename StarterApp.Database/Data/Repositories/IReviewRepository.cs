using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetAllAsync();
    Task<Review?> GetByIdAsync(int id);
    Task<List<Review>> GetByItemIdAsync(int itemId);
    Task<List<Review>> GetByReviewerIdAsync(int reviewerId);
    Task<Review> CreateAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(int id);
}