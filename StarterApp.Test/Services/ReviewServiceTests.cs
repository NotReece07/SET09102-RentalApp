using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.Test.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_ValidReview_CreatesReview()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Completed"
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

        // Act
        var review = await service.CreateReviewAsync(
            rentalId: 1,
            reviewerId: 200,
            rating: 5,
            comment: "Great item.");

        // Assert
        Assert.Equal(1, review.RentalId);
        Assert.Equal(10, review.ItemId);
        Assert.Equal(200, review.ReviewerId);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Great item.", review.Comment);
        Assert.Single(reviewRepository.Reviews);
    }

    [Fact]
    public async Task CreateReviewAsync_InvalidRating_ThrowsException()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Completed"
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

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
        var itemRepository = new FakeItemRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Completed"
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

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
        var itemRepository = new FakeItemRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Completed"
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 1,
            RentalId = 1,
            ItemId = 10,
            ReviewerId = 200,
            Rating = 5,
            Comment = "Already reviewed."
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

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
    public async Task CreateReviewAsync_RentalNotCompleted_ThrowsException()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 10,
            BorrowerId = 200,
            Status = "Approved"
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateReviewAsync(
                rentalId: 1,
                reviewerId: 200,
                rating: 5,
                comment: "Trying to review before completion."));

        // Assert
        Assert.Equal("You can only review completed rentals.", exception.Message);
    }

    [Fact]
    public async Task GetReviewsForItemAsync_ValidItem_ReturnsReviewsForThatItem()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        reviewRepository.Reviews.Add(new Review
        {
            Id = 1,
            ItemId = 10,
            RentalId = 1,
            ReviewerId = 200,
            Rating = 5,
            Comment = "Review for item 10."
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 2,
            ItemId = 99,
            RentalId = 2,
            ReviewerId = 201,
            Rating = 3,
            Comment = "Review for another item."
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

        // Act
        var reviews = await service.GetReviewsForItemAsync(10);

        // Assert
        Assert.Single(reviews);
        Assert.Equal(10, reviews[0].ItemId);
        Assert.Equal("Review for item 10.", reviews[0].Comment);
    }

    [Fact]
    public async Task GetAverageRatingForUserAsync_UserOwnsReviewedItems_ReturnsAverageRating()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 10,
            Title = "Drill",
            OwnerId = 100
        });

        itemRepository.Items.Add(new Item
        {
            Id = 11,
            Title = "Tent",
            OwnerId = 100
        });

        itemRepository.Items.Add(new Item
        {
            Id = 99,
            Title = "Different Owner Item",
            OwnerId = 999
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 1,
            ItemId = 10,
            RentalId = 1,
            ReviewerId = 200,
            Rating = 5,
            Comment = "Great."
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 2,
            ItemId = 11,
            RentalId = 2,
            ReviewerId = 201,
            Rating = 3,
            Comment = "Okay."
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 3,
            ItemId = 99,
            RentalId = 3,
            ReviewerId = 202,
            Rating = 1,
            Comment = "Should not count for user 100."
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

        // Act
        var averageRating = await service.GetAverageRatingForUserAsync(100);

        // Assert
        Assert.Equal(4.0, averageRating);
    }

    [Fact]
    public async Task GetReviewCountForUserAsync_UserOwnsReviewedItems_ReturnsReviewCount()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 10,
            Title = "Drill",
            OwnerId = 100
        });

        itemRepository.Items.Add(new Item
        {
            Id = 11,
            Title = "Tent",
            OwnerId = 100
        });

        itemRepository.Items.Add(new Item
        {
            Id = 99,
            Title = "Different Owner Item",
            OwnerId = 999
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 1,
            ItemId = 10,
            RentalId = 1,
            ReviewerId = 200,
            Rating = 5,
            Comment = "Great."
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 2,
            ItemId = 11,
            RentalId = 2,
            ReviewerId = 201,
            Rating = 3,
            Comment = "Okay."
        });

        reviewRepository.Reviews.Add(new Review
        {
            Id = 3,
            ItemId = 99,
            RentalId = 3,
            ReviewerId = 202,
            Rating = 1,
            Comment = "Should not count for user 100."
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

        // Act
        var reviewCount = await service.GetReviewCountForUserAsync(100);

        // Assert
        Assert.Equal(2, reviewCount);
    }

    [Fact]
    public async Task GetAverageRatingForUserAsync_UserHasNoReviews_ReturnsZero()
    {
        // Arrange
        var reviewRepository = new FakeReviewRepository();
        var rentalRepository = new FakeRentalRepository();
        var itemRepository = new FakeItemRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 10,
            Title = "Drill",
            OwnerId = 100
        });

        var service = new ReviewService(reviewRepository, rentalRepository, itemRepository);

        // Act
        var averageRating = await service.GetAverageRatingForUserAsync(100);

        // Assert
        Assert.Equal(0, averageRating);
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
            existingRental.BorrowerId = rental.BorrowerId;
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

    private class FakeItemRepository : IItemRepository
    {
        public List<Item> Items { get; } = new();

        public Task<List<Item>> GetAllAsync()
        {
            return Task.FromResult(Items);
        }

        public Task<Item?> GetByIdAsync(int id)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(item);
        }

        public Task<List<Item>> GetByOwnerIdAsync(int ownerId)
        {
            var items = Items
                .Where(i => i.OwnerId == ownerId)
                .ToList();

            return Task.FromResult(items);
        }

        public Task<List<Item>> GetNearbyAsync(double latitude, double longitude, double radiusKm)
        {
            return Task.FromResult(Items);
        }

        public Task<Item> CreateAsync(Item item)
        {
            item.Id = Items.Count + 1;
            Items.Add(item);
            return Task.FromResult(item);
        }

        public Task UpdateAsync(Item item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem == null)
            {
                throw new Exception("Item not found.");
            }

            existingItem.Title = item.Title;
            existingItem.Description = item.Description;
            existingItem.DailyRate = item.DailyRate;
            existingItem.Category = item.Category;
            existingItem.LocationName = item.LocationName;
            existingItem.Latitude = item.Latitude;
            existingItem.Longitude = item.Longitude;
            existingItem.OwnerId = item.OwnerId;
            existingItem.UpdatedAt = item.UpdatedAt;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var item = Items.FirstOrDefault(i => i.Id == id);

            if (item != null)
            {
                Items.Remove(item);
            }

            return Task.CompletedTask;
        }
    }
}