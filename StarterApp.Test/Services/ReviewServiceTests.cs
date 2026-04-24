using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using Xunit;

namespace StarterApp.Test.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_ValidReview_CreatesReview()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Approved"
        });

        var service = new ReviewService(reviewRepository, rentalRepository);

        // Act
        var review = await service.CreateReviewAsync(
            rentalId: 1,
            reviewerId: 200,
            rating: 5,
            comment: "Great item, easy rental.");

        // Assert
        Assert.Equal(1, review.RentalId);
        Assert.Equal(10, review.ItemId);
        Assert.Equal(200, review.ReviewerId);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Great item, easy rental.", review.Comment);
        Assert.Single(reviewRepository.Reviews);
    }

    [Fact]
    public async Task CreateReviewAsync_InvalidRating_ThrowsException()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Approved"
        });

        var service = new ReviewService(reviewRepository, rentalRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReviewAsync(
                rentalId: 1,
                reviewerId: 200,
                rating: 6,
                comment: "Invalid rating."));

        // Assert
        Assert.Equal("Rating must be between 1 and 5.", exception.Message);
    }

    [Fact]
    public async Task CreateReviewAsync_NonBorrower_ThrowsException()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Approved"
        });

        var service = new ReviewService(reviewRepository, rentalRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReviewAsync(
                rentalId: 1,
                reviewerId: 999,
                rating: 5,
                comment: "Trying to review someone else's rental."));

        // Assert
        Assert.Equal("You can only review rentals that you requested.", exception.Message);
    }

    [Fact]
    public async Task CreateReviewAsync_DuplicateReview_ThrowsException()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Approved"
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 1,
            RentalId = 1,
            ItemId = 10,
            ReviewerId = 200,
            Rating = 5,
            Comment = "Existing review."
        });

        var service = new ReviewService(reviewRepository, rentalRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReviewAsync(
                rentalId: 1,
                reviewerId: 200,
                rating: 4,
                comment: "Trying to review again."));

        // Assert
        Assert.Equal("You have already reviewed this rental.", exception.Message);
    }

    [Fact]
    public async Task CreateReviewAsync_RentalNotApprovedOrCompleted_ThrowsException()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Requested"
        });

        var service = new ReviewService(reviewRepository, rentalRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReviewAsync(
                rentalId: 1,
                reviewerId: 200,
                rating: 5,
                comment: "Trying to review before approval."));

        // Assert
        Assert.Equal("You can only review approved or completed rentals.", exception.Message);
    }

    [Fact]
    public async Task GetReviewsForItemAsync_ExistingReviews_ReturnsReviewsForItem()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();

        reviewRepository.Reviews.Add(new Review
        {
            Id = 1,
            RentalId = 1,
            ItemId = 10,
            ReviewerId = 200,
            Rating = 5,
            Comment = "Review for item 10."
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 2,
            RentalId = 2,
            ItemId = 20,
            ReviewerId = 300,
            Rating = 4,
            Comment = "Review for item 20."
        });

        var service = new ReviewService(reviewRepository, rentalRepository);

        // Act
        var reviews = await service.GetReviewsForItemAsync(itemId: 10);

        // Assert
        Assert.Single(reviews);
        Assert.Equal(10, reviews[0].ItemId);
        Assert.Equal("Review for item 10.", reviews[0].Comment);
    }

    private class FakeReviewRepository : IReviewRepository
    {
        public List<Review> Reviews { get; } = new();

        public Task<List<Review>> GetAllAsync()
        {
            return Task.FromResult(Reviews);
        }

        public Task<Review?> GetByIdAsync(int id)
        {
            var review = Reviews.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(review);
        }

        public Task<List<Review>> GetByItemIdAsync(int itemId)
        {
            var reviews = Reviews
                .Where(r => r.ItemId == itemId)
                .ToList();

            return Task.FromResult(reviews);
        }

        public Task<List<Review>> GetByReviewerIdAsync(int reviewerId)
        {
            var reviews = Reviews
                .Where(r => r.ReviewerId == reviewerId)
                .ToList();

            return Task.FromResult(reviews);
        }

        public Task<Review> CreateAsync(Review review)
        {
            review.Id = Reviews.Count + 1;
            Reviews.Add(review);
            return Task.FromResult(review);
        }

        public Task UpdateAsync(Review review)
        {
            var existingReview = Reviews.FirstOrDefault(r => r.Id == review.Id);

            if (existingReview == null)
            {
                throw new Exception("Review not found.");
            }

            existingReview.RentalId = review.RentalId;
            existingReview.ItemId = review.ItemId;
            existingReview.ReviewerId = review.ReviewerId;
            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = review.UpdatedAt;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var review = Reviews.FirstOrDefault(r => r.Id == id);

            if (review != null)
            {
                Reviews.Remove(review);
            }

            return Task.CompletedTask;
        }
    }

    private class FakeRentalRepository : IRentalRepository
    {
        public List<Rental> Rentals { get; } = new();

        public Task<List<Rental>> GetAllAsync()
        {
            return Task.FromResult(Rentals);
        }

        public Task<Rental?> GetByIdAsync(int id)
        {
            var rental = Rentals.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(rental);
        }

        public Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId)
        {
            var rentals = Rentals
                .Where(r => r.BorrowerId == borrowerId)
                .ToList();

            return Task.FromResult(rentals);
        }

        public Task<List<Rental>> GetByItemIdAsync(int itemId)
        {
            var rentals = Rentals
                .Where(r => r.ItemId == itemId)
                .ToList();

            return Task.FromResult(rentals);
        }

        public Task<Rental> CreateAsync(Rental rental)
        {
            rental.Id = Rentals.Count + 1;
            Rentals.Add(rental);
            return Task.FromResult(rental);
        }

        public Task UpdateAsync(Rental rental)
        {
            var existingRental = Rentals.FirstOrDefault(r => r.Id == rental.Id);

            if (existingRental == null)
            {
                throw new Exception("Rental not found.");
            }

            existingRental.ItemId = rental.ItemId;
            existingRental.Item = rental.Item;
            existingRental.BorrowerId = rental.BorrowerId;
            existingRental.Borrower = rental.Borrower;
            existingRental.StartDate = rental.StartDate;
            existingRental.EndDate = rental.EndDate;
            existingRental.Status = rental.Status;
            existingRental.TotalPrice = rental.TotalPrice;
            existingRental.UpdatedAt = rental.UpdatedAt;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var rental = Rentals.FirstOrDefault(r => r.Id == id);

            if (rental != null)
            {
                Rentals.Remove(rental);
            }

            return Task.CompletedTask;
        }
    }
}