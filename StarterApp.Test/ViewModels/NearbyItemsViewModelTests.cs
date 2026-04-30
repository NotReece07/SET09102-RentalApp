using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.Test.Services;
using StarterApp.ViewModels;

namespace StarterApp.Test.ViewModels;

public class NearbyItemsViewModelTests
{
    [Fact]
    public async Task FindNearbyItemsCommand_WhenLocationIsFound_LoadsNearbyItems()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 1,
            Title = "Nearby Drill",
            Description = "A nearby test item",
            Category = "Tools",
            DailyRate = 10,
            LocationName = "Edinburgh",
            Latitude = 55.9336,
            Longitude = -3.2133,
            OwnerId = 1
        });

        var mockLocationService = new MockLocationService(
            new AppLocation(55.9336, -3.2133)); // fake GPS location for the test

        var viewModel = new NearbyItemsViewModel(itemRepository, mockLocationService);

        // Act
        await viewModel.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(viewModel.NearbyItems);
        Assert.Equal("Nearby Drill", viewModel.NearbyItems[0].Title);
        Assert.Equal("Found 1 item(s) within 5km.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_WhenLocationIsNull_ShowsLocationError()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var mockLocationService = new MockLocationService(null); // simulates location permission denied or unavailable

        var viewModel = new NearbyItemsViewModel(itemRepository, mockLocationService);

        // Act
        await viewModel.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(viewModel.NearbyItems);
        Assert.Equal("Could not get your location. Check location permissions or use the emulator location settings.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_WhenNoItemsFound_ShowsNoItemsMessage()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();

        var mockLocationService = new MockLocationService(
            new AppLocation(55.9336, -3.2133)); // fake GPS location for the test

        var viewModel = new NearbyItemsViewModel(itemRepository, mockLocationService);

        // Act
        await viewModel.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(viewModel.NearbyItems);
        Assert.Equal("No items found within 5km.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_UsesSelectedRadius()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 1,
            Title = "Nearby Charger",
            Description = "A nearby charger",
            Category = "Electronics",
            DailyRate = 5,
            LocationName = "Edinburgh",
            Latitude = 55.9336,
            Longitude = -3.2133,
            OwnerId = 1
        });

        var mockLocationService = new MockLocationService(
            new AppLocation(55.9336, -3.2133)); // fake GPS location for the test

        var viewModel = new NearbyItemsViewModel(itemRepository, mockLocationService)
        {
            RadiusKm = 10
        };

        // Act
        await viewModel.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(10, itemRepository.LastRadiusKm);
        Assert.Equal("Found 1 item(s) within 10km.", viewModel.StatusMessage);
    }

    private class FakeItemRepository : IItemRepository
    {
        public List<Item> Items { get; } = new();

        public double LastLatitude { get; private set; } // stores the latitude passed into GetNearbyAsync
        public double LastLongitude { get; private set; } // stores the longitude passed into GetNearbyAsync
        public double LastRadiusKm { get; private set; } // stores the radius passed into GetNearbyAsync

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
            LastLatitude = latitude; // stores the latitude so the test can confirm the ViewModel passed it correctly
            LastLongitude = longitude; // stores the longitude so the test can confirm the ViewModel passed it correctly
            LastRadiusKm = radiusKm; // stores the radius so the test can confirm the selected radius was used

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