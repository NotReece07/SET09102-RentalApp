using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Services;

// This service works as a simple rental state machine.
// It controls which rental status transitions are allowed and blocks invalid transitions.
// Valid workflow:
// Requested -> Approved or Rejected
// Approved -> Out For Rent
// Out For Rent -> Returned
// Returned -> Completed
// This is not a full formal State Pattern with separate RequestedState/ApprovedState classes,
// but it applies the same idea by centralising state transition rules in one service layer.

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository; // stores the rental repository so this service can load and save rentals
    private readonly IItemRepository _itemRepository; // stores the item repository so this service can load items and check ownership / price

    public RentalService(IRentalRepository rentalRepository, IItemRepository itemRepository)
    {
        _rentalRepository = rentalRepository; // stores rentalRepository so it can be used through the whole class
        _itemRepository = itemRepository; // stores itemRepository so it can be used through the whole class
    }

    public async Task<Rental> RequestRentalAsync(int itemId, int borrowerId, DateOnly startDate, DateOnly endDate)
    {
        if (borrowerId <= 0)
        {
            throw new Exception("You must be logged in to request a rental."); // stops the request if no valid logged-in local user exists
        }

        if (startDate > endDate)
        {
            throw new Exception("Start date cannot be after end date."); // stops invalid date ranges
        }

        var item = await _itemRepository.GetByIdAsync(itemId); // loads the item being requested

        if (item == null)
        {
            throw new Exception("Item not found."); // stops the request if the item does not exist
        }

        if (item.OwnerId == borrowerId)
        {
            throw new Exception("You cannot rent your own item."); // stops the user from requesting their own item
        }

        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId); // loads existing rentals for this item

        var hasOverlap = existingRentals.Any(r =>
            IsBookingStatus(r.Status) && // approved/out-for-rent/returned/completed rentals block the dates
            startDate <= r.EndDate &&
            endDate >= r.StartDate);

        if (hasOverlap)
        {
            throw new Exception("This item is already booked for the selected dates."); // stops double booking
        }

        var numberOfDays = endDate.DayNumber - startDate.DayNumber + 1; // works out how many rental days are being requested, inclusive of both start and end date

        if (numberOfDays <= 0)
        {
            throw new Exception("Rental duration must be at least 1 day."); // safety check
        }

        var rental = new Rental // creates a new rental object after all business rules have passed
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            StartDate = startDate,
            EndDate = endDate,
            Status = "Requested",
            TotalPrice = item.DailyRate * numberOfDays, // calculates the total price using daily rate x number of days
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _rentalRepository.CreateAsync(rental); // saves the new rental request to the database
    }

    public async Task ApproveRentalAsync(int rentalId, int currentOwnerId)
    {
        var rental = await LoadRentalWithItemAsync(rentalId); // loads the rental and checks it exists

        if (rental.Item!.OwnerId != currentOwnerId)
        {
            throw new Exception("You can only approve rentals for your own items."); // blocks non-owners from approving rentals
        }

        if (rental.Status != "Requested")
        {
            throw new Exception("Only requested rentals can be approved."); // only requested rentals can move to approved
        }

        var existingRentals = await _rentalRepository.GetByItemIdAsync(rental.ItemId); // loads other rentals for the same item

        var hasOverlap = existingRentals.Any(r =>
            r.Id != rental.Id && // ignore the rental being approved
            IsBookingStatus(r.Status) && // active/confirmed rental states block the dates
            rental.StartDate <= r.EndDate &&
            rental.EndDate >= r.StartDate);

        if (hasOverlap)
        {
            throw new Exception("This item is already booked for the selected dates."); // prevents approving a booking that overlaps an existing booking
        }

        rental.Status = "Approved"; // updates the rental to approved
        rental.UpdatedAt = DateTime.UtcNow;

        await _rentalRepository.UpdateAsync(rental); // saves the approval to the database
    }

    public async Task RejectRentalAsync(int rentalId, int currentOwnerId)
    {
        var rental = await LoadRentalWithItemAsync(rentalId); // loads the rental and checks it exists

        if (rental.Item!.OwnerId != currentOwnerId)
        {
            throw new Exception("You can only reject rentals for your own items."); // blocks non-owners from rejecting rentals
        }

        if (rental.Status != "Requested")
        {
            throw new Exception("Only requested rentals can be rejected."); // only requested rentals can move to rejected
        }

        rental.Status = "Rejected"; // updates the rental to rejected
        rental.UpdatedAt = DateTime.UtcNow;

        await _rentalRepository.UpdateAsync(rental); // saves the rejection to the database
    }

    public async Task MarkOutForRentAsync(int rentalId, int currentOwnerId)
    {
        var rental = await LoadRentalWithItemAsync(rentalId); // loads the rental and checks it exists

        if (rental.Item!.OwnerId != currentOwnerId)
        {
            throw new Exception("You can only mark your own items as out for rent."); // blocks non-owners from moving the rental forward
        }

        if (rental.Status != "Approved")
        {
            throw new Exception("Only approved rentals can be marked as out for rent."); // approved moves to out for rent
        }

        rental.Status = "Out For Rent"; // updates the rental to out for rent
        rental.UpdatedAt = DateTime.UtcNow;

        await _rentalRepository.UpdateAsync(rental); // saves the status change to the database
    }

    public async Task MarkReturnedAsync(int rentalId, int currentBorrowerId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId); // loads the rental being returned

        if (rental == null)
        {
            throw new Exception("Rental not found."); // stops if rental does not exist
        }

        if (rental.BorrowerId != currentBorrowerId)
        {
            throw new Exception("You can only return rentals that you requested."); // blocks other users from returning the rental
        }

        if (rental.Status != "Out For Rent")
        {
            throw new Exception("Only out for rent rentals can be marked as returned."); // out for rent moves to returned
        }

        rental.Status = "Returned"; // updates the rental to returned
        rental.UpdatedAt = DateTime.UtcNow;

        await _rentalRepository.UpdateAsync(rental); // saves the returned status to the database
    }

    public async Task CompleteRentalAsync(int rentalId, int currentOwnerId)
    {
        var rental = await LoadRentalWithItemAsync(rentalId); // loads the rental and checks it exists

        if (rental.Item!.OwnerId != currentOwnerId)
        {
            throw new Exception("You can only complete rentals for your own items."); // blocks non-owners from completing the rental
        }

        if (rental.Status != "Returned")
        {
            throw new Exception("Only returned rentals can be completed."); // returned moves to completed
        }

        rental.Status = "Completed"; // updates the rental to completed
        rental.UpdatedAt = DateTime.UtcNow;

        await _rentalRepository.UpdateAsync(rental); // saves the completed status to the database
    }

    public async Task<List<Rental>> GetOutgoingRentalsAsync(int borrowerId)
    {
        if (borrowerId <= 0)
        {
            return new List<Rental>(); // returns an empty list if there is no valid logged-in borrower
        }

        return await _rentalRepository.GetByBorrowerIdAsync(borrowerId); // gets all rentals where the current user is the borrower
    }

    public async Task<List<Rental>> GetIncomingRentalsAsync(int ownerId)
    {
        if (ownerId <= 0)
        {
            return new List<Rental>(); // returns an empty list if there is no valid logged-in owner
        }

        var ownedItems = await _itemRepository.GetByOwnerIdAsync(ownerId); // gets all items owned by the current user
        var incomingRentals = new List<Rental>(); // creates a temporary list to hold all rentals for those owned items

        foreach (var item in ownedItems)
        {
            var itemRentals = await _rentalRepository.GetByItemIdAsync(item.Id); // gets rentals for each owned item
            incomingRentals.AddRange(itemRentals); // adds them into one combined list
        }

        return incomingRentals; // returns all incoming rentals for items owned by the current user
    }

    private static bool IsBookingStatus(string status)
    {
        return status == "Approved"
            || status == "Out For Rent"
            || status == "Returned"
            || status == "Completed"; // these states block overlapping bookings
    }

    private async Task<Rental> LoadRentalWithItemAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId); // loads the rental

        if (rental == null)
        {
            throw new Exception("Rental not found."); // stops if rental does not exist
        }

        if (rental.Item == null)
        {
            throw new Exception("Rental item details could not be loaded."); // stops if related item data is missing
        }

        return rental; // returns the valid rental with item details
    }
}