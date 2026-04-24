using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Models;
using StarterApp.Database.Data;

namespace StarterApp.Database.Data.Repositories;

public class ItemRepository : IItemRepository //ItemRepository will follow the rules in the interface (IItemRepository)
{
    private readonly AppDbContext _context; //_context is the private field storesd inside the class

    //gives the repository access to the database context, this is the constructor
    public ItemRepository(AppDbContext context) // When itemRepository is created, it is given an AppDbContext. That incoming value is temporarilly called context
    {
        _context = context; //context is the value arriving (name of the AppDbContext obnject being passed in), and _context is the saved version used by the whole class
    }

    // Gets all items from the database
    public async Task<List<Item>> GetAllAsync()
    {
        return await _context.Items
            .AsNoTracking() // Tells EF Core to fetch fresh read-only data instead of using tracked cached entities
            .Include(i => i.Owner)
            .ToListAsync();
    }
    //finds one item by its ID
    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .AsNoTracking() // tells EF Core to fetch a fresh copy of this item from the database
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    //Adds a new item to the database and saves it
    public async Task<Item> CreateAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    //This updates an existing item and saves the change
    public async Task UpdateAsync(Item item)
    {
        var existingItem = await _context.Items.FindAsync(item.Id); // finds the tracked version of the item already in the database

        if (existingItem == null)
        {
            throw new Exception("Item not found."); // stops the update if the item does not exist
        }

        existingItem.Title = item.Title; // copies the edited values into the tracked database item
        existingItem.Description = item.Description;
        existingItem.DailyRate = item.DailyRate;
        existingItem.Category = item.Category;
        existingItem.LocationName = item.LocationName;
        existingItem.Latitude = item.Latitude;
        existingItem.Longitude = item.Longitude;
        existingItem.OwnerId = item.OwnerId;

        await _context.SaveChangesAsync(); // saves the updated tracked item to the database
    }

    // Finds an item, deletes that item if it exists
    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);

        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Item>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Items
            .AsNoTracking() // tells EF Core to fetch fresh items from the db owned by this user instead of tracked cached ones
            .Include(i => i.Owner)
            .Where(i => i.OwnerId == ownerId)
            .ToListAsync();
    }
}