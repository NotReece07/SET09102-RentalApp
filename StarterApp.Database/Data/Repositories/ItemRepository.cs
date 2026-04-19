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
            .Include(i => i.Owner) // loads the related owner user
            .ToListAsync();
    }

    //finds one item by its ID
    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id); // find the first item whose id matches the id given, if none exists then return null
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
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
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
            .Include(i => i.Owner)
            .Where(i => i.OwnerId == ownerId)
            .ToListAsync();
    }
}