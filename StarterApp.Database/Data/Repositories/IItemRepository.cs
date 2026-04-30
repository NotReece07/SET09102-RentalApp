using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

// IItemRepository inherits the shared IRepository<Item> methods and adds item-specific queries
public interface IItemRepository : IRepository<Item>
{
    Task<List<Item>> GetByOwnerIdAsync(int ownerId); // gets items owned by one user
    Task<List<Item>> GetNearbyAsync(double latitude, double longitude, double radiusKm); // gets items near a location within a radius
}