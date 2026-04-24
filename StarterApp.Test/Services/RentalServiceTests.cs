using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using Xunit;

namespace StarterApp.Test.Services;

public class RentalServiceTests
{
    [Fact]
    public async Task RequestRentalAsync_ValidRequest_CreatesRentalWithRequestedStatusAndTotalPrice()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var rentalRepository = new FakeRentalRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 1,
            Title = "Power Drill",
            DailyRate = 10.00m,
            OwnerId = 100
        });

        var service = new RentalService(rentalRepository, itemRepository);

        // Act
        var rental = await service.RequestRentalAsync(
            itemId: 1,
            borrowerId: 200,
            startDate: new DateOnly(2026, 4, 23),
            endDate: new DateOnly(2026, 4, 25));

        // Assert
        Assert.Equal(1, rental.ItemId);
        Assert.Equal(200, rental.BorrowerId);
        Assert.Equal("Requested", rental.Status);
        Assert.Equal(30.00m, rental.TotalPrice);
        Assert.Single(rentalRepository.Rentals);
    }

    [Fact]
    public async Task RequestRentalAsync_UserRentingOwnItem_ThrowsException()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var rentalRepository = new FakeRentalRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 1,
            Title = "Power Drill",
            DailyRate = 10.00m,
            OwnerId = 100
        });

        var service = new RentalService(rentalRepository, itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.RequestRentalAsync(
                itemId: 1,
                borrowerId: 100,
                startDate: new DateOnly(2026, 4, 23),
                endDate: new DateOnly(2026, 4, 25)));

        // Assert
        Assert.Equal("You cannot rent your own item.", exception.Message);
    }

    [Fact]
    public async Task RequestRentalAsync_StartDateAfterEndDate_ThrowsException()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var rentalRepository = new FakeRentalRepository();
        var service = new RentalService(rentalRepository, itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.RequestRentalAsync(
                itemId: 1,
                borrowerId: 200,
                startDate: new DateOnly(2026, 4, 25),
                endDate: new DateOnly(2026, 4, 23)));

        // Assert
        Assert.Equal("Start date cannot be after end date.", exception.Message);
    }

    [Fact]
    public async Task ApproveRentalAsync_RequestedRental_ChangesStatusToApproved()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var rentalRepository = new FakeRentalRepository();

        var item = new Item
        {
            Id = 1,
            Title = "Power Drill",
            DailyRate = 10.00m,
            OwnerId = 100
        };

        itemRepository.Items.Add(item);

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 1,
            Item = item,
            BorrowerId = 200,
            StartDate = new DateOnly(2026, 4, 23),
            EndDate = new DateOnly(2026, 4, 25),
            Status = "Requested",
            TotalPrice = 30.00m
        });

        var service = new RentalService(rentalRepository, itemRepository);

        // Act
        await service.ApproveRentalAsync(rentalId: 1, currentOwnerId: 100);

        // Assert
        Assert.Equal("Approved", rentalRepository.Rentals[0].Status);
    }

    [Fact]
    public async Task ApproveRentalAsync_NonOwner_ThrowsException()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var rentalRepository = new FakeRentalRepository();

        var item = new Item
        {
            Id = 1,
            Title = "Power Drill",
            DailyRate = 10.00m,
            OwnerId = 100
        };

        itemRepository.Items.Add(item);

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 1,
            Item = item,
            BorrowerId = 200,
            StartDate = new DateOnly(2026, 4, 23),
            EndDate = new DateOnly(2026, 4, 25),
            Status = "Requested",
            TotalPrice = 30.00m
        });

        var service = new RentalService(rentalRepository, itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.ApproveRentalAsync(rentalId: 1, currentOwnerId: 999));

        // Assert
        Assert.Equal("You can only approve rentals for your own items.", exception.Message);
    }

    [Fact]
    public async Task ApproveRentalAsync_OverlappingApprovedRental_ThrowsException()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var rentalRepository = new FakeRentalRepository();

        var item = new Item
        {
            Id = 1,
            Title = "Power Drill",
            DailyRate = 10.00m,
            OwnerId = 100
        };

        itemRepository.Items.Add(item);

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 1,
            Item = item,
            BorrowerId = 200,
            StartDate = new DateOnly(2026, 4, 23),
            EndDate = new DateOnly(2026, 4, 25),
            Status = "Requested",
            TotalPrice = 30.00m
        });

        rentalRepository.Rentals.Add(new Rental
        {
            Id = 2,
            ItemId = 1,
            Item = item,
            BorrowerId = 300,
            StartDate = new DateOnly(2026, 4, 24),
            EndDate = new DateOnly(2026, 4, 26),
            Status = "Approved",
            TotalPrice = 30.00m
        });

        var service = new RentalService(rentalRepository, itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.ApproveRentalAsync(rentalId: 1, currentOwnerId: 100));

        // Assert
        Assert.Equal("This item is already booked for the selected dates.", exception.Message);
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
            existingItem.IsAvailable = item.IsAvailable;

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