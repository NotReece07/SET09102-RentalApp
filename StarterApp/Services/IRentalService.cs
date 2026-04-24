using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<Rental> RequestRentalAsync(int itemId, int borrowerId, DateOnly startDate, DateOnly endDate); // creates a rental request while applying business rules
    Task ApproveRentalAsync(int rentalId, int currentOwnerId); // lets the item owner approve a requested rental
    Task RejectRentalAsync(int rentalId, int currentOwnerId); // lets the item owner reject a requested rental
    Task CompleteRentalAsync(int rentalId, int currentBorrowerId); // lets the borrower mark an approved rental as completed
    Task<List<Rental>> GetOutgoingRentalsAsync(int borrowerId); // gets rentals where the current user is the borrower
    Task<List<Rental>> GetIncomingRentalsAsync(int ownerId); // gets rentals for items owned by the current user
}