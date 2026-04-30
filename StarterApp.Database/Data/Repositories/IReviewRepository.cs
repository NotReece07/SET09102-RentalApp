using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

// IReviewRepository inherits the shared IRepository<Review> methods and adds review-specific queries
public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByItemIdAsync(int itemId); // gets reviews linked to one item
    Task<List<Review>> GetByReviewerIdAsync(int reviewerId); // gets reviews written by one user
}