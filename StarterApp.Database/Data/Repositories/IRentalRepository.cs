using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

// IRentalRepository inherits the shared IRepository<Rental> methods and adds rental-specific queries
public interface IRentalRepository : IRepository<Rental>
{
    Task<List<Rental>> GetByBorrowerIdAsync(int borrowerId); // gets rentals requested by one borrower
    Task<List<Rental>> GetByItemIdAsync(int itemId); // gets rentals linked to one item
}