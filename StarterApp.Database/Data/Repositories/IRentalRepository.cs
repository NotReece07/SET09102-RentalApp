using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IRentalRepository
{
    Task<List<Rental>> GetAllAsync(); // get all rental records
    Task<Rental?> GetByIdAsync(int id); // get one rental by its ID
    Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId); // get all rentals made by one user
    Task<List<Rental>> GetByItemIdAsync(int itemId); // get all rentals for one item id
    Task<Rental> CreateAsync(Rental rental); // create a new rental request
    Task UpdateAsync(Rental rental); // update a rental record
    Task DeleteAsync(int id); // delete a rental record
}