using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.ViewModels;

namespace StarterApp.Test.ViewModels;

public class ItemsListViewModelTests
{
    [Fact]
    public async Task LoadItemsCommand_WhenRepositoryHasItems_LoadsItemsIntoViewModel()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();

        itemRepository.Items.Add(new Item
        {
            Id = 1,
            Title = "Test Drill",
            Description = "A drill for testing",
            Category = "Tools",
            DailyRate = 10,
            LocationName = "Edinburgh",
            OwnerId = 1
        });

        itemRepository.Items.Add(new Item
        {
            Id = 2,
            Title = "Test Tent",
            Description = "A tent for testing",
            Category = "Camping",
            DailyRate = 20,
            LocationName = "Glasgow",
            OwnerId = 1
        });

        var viewModel = new ItemsListViewModel(itemRepository);

        // Act
        await viewModel.LoadItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, viewModel.Items.Count);
        Assert.Equal("Test Drill", viewModel.Items[0].Title);
        Assert.Equal("Test Tent", viewModel.Items[1].Title);
    }

    [Fact]
    public void SelectItemCommand_WhenItemIsPassed_SetsSelectedItem()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var viewModel = new ItemsListViewModel(itemRepository);

        var item = new Item
        {
            Id = 1,
            Title = "Selected Item",
            Description = "Item selected during test",
            Category = "Tools",
            DailyRate = 10,
            LocationName = "Edinburgh",
            OwnerId = 1
        };

        // Act
        viewModel.SelectItemCommand.Execute(item);

        // Assert
        Assert.NotNull(viewModel.SelectedItem);
        Assert.Equal("Selected Item", viewModel.SelectedItem.Title);
    }

    [Fact]
    public void SelectItemCommand_WhenNullIsPassed_DoesNotSetSelectedItem()
    {
        // Arrange
        var itemRepository = new FakeItemRepository();
        var viewModel = new ItemsListViewModel(itemRepository);

        // Act
        viewModel.SelectItemCommand.Execute(null);

        // Assert
        Assert.Null(viewModel.SelectedItem);
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