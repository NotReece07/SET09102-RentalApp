using Microsoft.EntityFrameworkCore; // lets the file use features like Include, ToListAsync etc...
using StarterApp.Database.Data; // lets the file use AppDbContext
using StarterApp.Database.Models; // Lets the file use the Rental, Item, and User model types

namespace StarterApp.Database.Data.Repositories;

public class RentalRepository : IRentalRepository //The RentalRepositoriy class must provice all methods listed in the interface
{
    private readonly AppDbContext _context; //Creates a private stores database context for the class. _Context is the saved database object used by all methods

    // Constructor
    public RentalRepository(AppDbContext context) //runs when RentalRepository is created and recives an AppDbContext
    {
        _context = context; // stores the incoming context in the private field _context. So the rest of the class can use the database
    }

    public async Task<List<Rental>> GetAllAsync() // method that returns a list of all rentals
    {
        return await _context.Rentals
            .AsNoTracking() // tells EF Core to fetch fresh read-only rental data instead of tracked cached entities
            .Include(r => r.Item) // load the related item for each rental
            .Include(r => r.Borrower) // also load the related borrower for each rental
            .ToListAsync(); // Get all rentals, including their related item and borrower
    }

    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals
            .AsNoTracking() // tells EF Core to fetch a fresh read-only rental from the database
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .FirstOrDefaultAsync(r => r.Id == id); // Find one rental by ID, including its item and borrower
    }

    public async Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId) // get all rentals made by one borrower
    {
        return await _context.Rentals
            .AsNoTracking() // tells EF Core to fetch fresh read-only rentals for this borrower
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .Where(r => r.BorrowerId == borrowerId) // only keep rentals where the borrower id matches the given user ID
            .ToListAsync(); // get all rentals requested by a particular user
    }

    public async Task<List<Rental>> GetByItemIdAsync(int itemId) // Get all rentals for a particular item
    {
        return await _context.Rentals
            .AsNoTracking() // tells EF Core to fetch fresh read-only rentals for this item
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .Where(r => r.ItemId == itemId) // only keep rentals where ItemId matches given item ID
            .ToListAsync(); // Get all rental requests for a given item
    }

    public async Task<Rental> CreateAsync(Rental rental)
    {
        _context.Rentals.Add(rental); // add the rental to the Rental table
        await _context.SaveChangesAsync(); // save the change to the actual database
        return rental; // return the saved rental object
    }

    public async Task UpdateAsync(Rental rental)
    {
        var existingRental = await _context.Rentals.FindAsync(rental.Id); // finds the tracked version of the rental already in the database

        if (existingRental == null)
        {
            throw new Exception("Rental not found."); // stops the update if the rental does not exist
        }

        existingRental.ItemId = rental.ItemId; // copies the edited values into the tracked database rental
        existingRental.BorrowerId = rental.BorrowerId;
        existingRental.StartDate = rental.StartDate;
        existingRental.EndDate = rental.EndDate;
        existingRental.Status = rental.Status;
        existingRental.TotalPrice = rental.TotalPrice;
        existingRental.UpdatedAt = DateTime.UtcNow; // updates the timestamp to show when the rental was last edited

        await _context.SaveChangesAsync(); //save changes to database
    }

    public async Task DeleteAsync(int id)
    {
        var rental = await _context.Rentals.FindAsync(id); // look for the rental with that ID

        if (rental != null) //only continue if rental found
        {
            _context.Rentals.Remove(rental); //Mark found rental for deletion
            await _context.SaveChangesAsync(); //save the deletion to database
        }
    }
}