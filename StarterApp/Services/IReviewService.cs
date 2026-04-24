using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IReviewService
{
    Task<Review> CreateReviewAsync(int rentalId, int reviewerId, int rating, string comment); // creates a review while applying review business rules
    Task<List<Review>> GetReviewsForItemAsync(int itemId); // gets all reviews for one item
}