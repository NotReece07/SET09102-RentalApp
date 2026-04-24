using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository; // stores the review repository so this service can save and load reviews
    private readonly IRentalRepository _rentalRepository; // stores the rental repository so this service can check the rental being reviewed

    public ReviewService(IReviewRepository reviewRepository, IRentalRepository rentalRepository)
    {
        _reviewRepository = reviewRepository; // stores reviewRepository so it can be used through the whole class
        _rentalRepository = rentalRepository; // stores rentalRepository so it can be used through the whole class
    }

    public async Task<Review> CreateReviewAsync(int rentalId, int reviewerId, int rating, string comment)
    {
        if (reviewerId <= 0)
        {
            throw new Exception("You must be logged in to create a review.");
        }

        if (rentalId <= 0)
        {
            throw new Exception("No rental selected for this review.");
        }

        if (rating < 1 || rating > 5)
        {
            throw new Exception("Rating must be between 1 and 5.");
        }

        var rental = await _rentalRepository.GetByIdAsync(rentalId); // loads the rental being reviewed

        if (rental == null)
        {
            throw new Exception("Rental not found.");
        }

        if (rental.BorrowerId != reviewerId)
        {
            throw new Exception("You can only review rentals that you requested.");
        }

        if (rental.Status != "Approved" && rental.Status != "Completed")
        {
            throw new Exception("You can only review approved or completed rentals.");
        }

        var existingReviews = await _reviewRepository.GetByReviewerIdAsync(reviewerId); // loads reviews already written by this user

        var alreadyReviewed = existingReviews.Any(r => r.RentalId == rentalId);

        if (alreadyReviewed)
        {
            throw new Exception("You have already reviewed this rental.");
        }

        var review = new Review
        {
            RentalId = rental.Id,
            ItemId = rental.ItemId,
            ReviewerId = reviewerId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _reviewRepository.CreateAsync(review); // saves the review to the database
    }

    public async Task<List<Review>> GetReviewsForItemAsync(int itemId)
    {
        if (itemId <= 0)
        {
            return new List<Review>();
        }

        return await _reviewRepository.GetByItemIdAsync(itemId); // gets all reviews linked to this item
    }
}