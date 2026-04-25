using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IReviewService
{
    Task<Review> CreateReviewAsync(int rentalId, int reviewerId, int rating, string comment); // creates a review while applying review business rules
    Task<List<Review>> GetReviewsForItemAsync(int itemId); // gets all reviews for one item
    Task<double> GetAverageRatingForUserAsync(int userId); // calculates the average rating for items owned by one user
    Task<int> GetReviewCountForUserAsync(int userId); // counts reviews for items owned by one user
}