using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.Repositories;

public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture; // stores the shared database fixture for integration tests

    public ItemRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture; // stores the fixture so each test can create a database context
    }

    [Fact]
    public async Task CreateAsync_ValidItem_SavesItemToDatabase()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new ItemRepository(context);

        var owner = await CreateTestUserAsync(context);

        var item = new Item
        {
            Title = "Integration Test Drill",
            Description = "Created by ItemRepositoryTests",
            DailyRate = 12.50m,
            Category = "Tools",
            LocationName = "Edinburgh",
            Latitude = 55.9336,
            Longitude = -3.2133,
            OwnerId = owner.Id
        };

        try
        {
            // Act
            var createdItem = await repository.CreateAsync(item);

            // Assert
            Assert.True(createdItem.Id > 0);
            Assert.Equal("Integration Test Drill", createdItem.Title);
            Assert.NotNull(createdItem.Location); // confirms the spatial location was created before saving
        }
        finally
        {
            await CleanupTestDataAsync(context, owner.Id);
        }
    }

    [Fact]
    public async Task GetByOwnerIdAsync_WhenOwnerHasItems_ReturnsOnlyThatOwnersItems()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new ItemRepository(context);

        var ownerOne = await CreateTestUserAsync(context);
        var ownerTwo = await CreateTestUserAsync(context);

        try
        {
            await repository.CreateAsync(new Item
            {
                Title = "Owner One Drill",
                Description = "Belongs to owner one",
                DailyRate = 10,
                Category = "Tools",
                LocationName = "Edinburgh",
                Latitude = 55.9336,
                Longitude = -3.2133,
                OwnerId = ownerOne.Id
            });

            await repository.CreateAsync(new Item
            {
                Title = "Owner Two Tent",
                Description = "Belongs to owner two",
                DailyRate = 20,
                Category = "Camping",
                LocationName = "Glasgow",
                Latitude = 55.8642,
                Longitude = -4.2518,
                OwnerId = ownerTwo.Id
            });

            // Act
            var ownerOneItems = await repository.GetByOwnerIdAsync(ownerOne.Id);

            // Assert
            Assert.Single(ownerOneItems);
            Assert.Equal("Owner One Drill", ownerOneItems[0].Title);
            Assert.Equal(ownerOne.Id, ownerOneItems[0].OwnerId);
        }
        finally
        {
            await CleanupTestDataAsync(context, ownerOne.Id);
            await CleanupTestDataAsync(context, ownerTwo.Id);
        }
    }

    [Fact]
    public async Task GetNearbyAsync_ItemInsideRadius_ReturnsNearbyItem()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new ItemRepository(context);

        var owner = await CreateTestUserAsync(context);

        try
        {
            await repository.CreateAsync(new Item
            {
                Title = "Nearby Integration Test Charger",
                Description = "Near Edinburgh Napier Merchiston",
                DailyRate = 5,
                Category = "Electronics",
                LocationName = "Edinburgh Napier Merchiston",
                Latitude = 55.9336,
                Longitude = -3.2133,
                OwnerId = owner.Id
            });

            // Act
            var nearbyItems = await repository.GetNearbyAsync(
                latitude: 55.9336,
                longitude: -3.2133,
                radiusKm: 5);

            // Assert
            Assert.Contains(nearbyItems, item => item.Title == "Nearby Integration Test Charger");
        }
        finally
        {
            await CleanupTestDataAsync(context, owner.Id);
        }
    }

    private static async Task<User> CreateTestUserAsync(StarterApp.Database.Data.AppDbContext context)
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var user = new User
        {
            FirstName = "Integration",
            LastName = "Tester",
            Email = $"integration-test-{uniqueId}@example.com",
            PasswordHash = "TEST_HASH",
            PasswordSalt = "TEST_SALT",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    private static async Task CleanupTestDataAsync(StarterApp.Database.Data.AppDbContext context, int ownerId)
    {
        var items = context.Items
            .Where(i => i.OwnerId == ownerId)
            .ToList();

        context.Items.RemoveRange(items);

        var users = context.Users
            .Where(u => u.Id == ownerId)
            .ToList();

        context.Users.RemoveRange(users);

        await context.SaveChangesAsync();
    }
}