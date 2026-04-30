using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : ObservableObject
{
    private readonly IRentalService _rentalService; // stores the rental service so the ViewModel can load rentals and apply rental workflow business rules
    private readonly IAuthenticationService _authService; // stores the auth service so the ViewModel can get the logged-in user's local database ID

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> outgoingRentals = new(); // stores the rental requests made by the current user, wrapped with button visibility rules

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> incomingRentals = new(); // stores rental requests for items owned by the current user, wrapped with button visibility rules

    [ObservableProperty]
    private string statusMessage = string.Empty; // used to show messages such as errors or "no rentals found"

    public RentalsViewModel(IRentalService rentalService, IAuthenticationService authService)
    {
        _rentalService = rentalService; // stores rentalService so it can be used through the whole class
        _authService = authService; // stores authService so the ViewModel can get the logged-in user's local database ID
    }

    [RelayCommand]
    private async Task LoadOutgoingRentalsAsync()
    {
        try
        {
            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in user's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                OutgoingRentals = new ObservableCollection<RentalDisplayItem>();
                return;
            }

            var rentals = await _rentalService.GetOutgoingRentalsAsync(localUserId); // gets all rentals where this user is the borrower

            OutgoingRentals = new ObservableCollection<RentalDisplayItem>(
                rentals.Select(rental => new RentalDisplayItem(rental))
            ); // wraps each rental so the UI knows which buttons should be visible

            StatusMessage = OutgoingRentals.Count == 0
                ? "No outgoing rental requests found."
                : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }

    [RelayCommand]
    private async Task LoadIncomingRentalsAsync()
    {
        try
        {
            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in user's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                IncomingRentals = new ObservableCollection<RentalDisplayItem>();
                return;
            }

            var rentals = await _rentalService.GetIncomingRentalsAsync(localUserId); // gets all rentals for items owned by the current user

            IncomingRentals = new ObservableCollection<RentalDisplayItem>(
                rentals.Select(rental => new RentalDisplayItem(rental))
            ); // wraps each rental so the UI knows which buttons should be visible

            StatusMessage = IncomingRentals.Count == 0
                ? "No incoming rental requests found."
                : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(RentalDisplayItem? rentalDisplayItem)
    {
        try
        {
            if (rentalDisplayItem == null)
                return;

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.ApproveRentalAsync(rentalDisplayItem.Rental.Id, localUserId); // moves requested to approved

            StatusMessage = "Rental approved successfully.";

            await RefreshRentalListsAsync(); // refreshes both lists after the status change
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message;
        }
    }

    [RelayCommand]
    private async Task RejectRentalAsync(RentalDisplayItem? rentalDisplayItem)
    {
        try
        {
            if (rentalDisplayItem == null)
                return;

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.RejectRentalAsync(rentalDisplayItem.Rental.Id, localUserId); // moves requested to rejected

            StatusMessage = "Rental rejected successfully.";

            await RefreshRentalListsAsync(); // refreshes both lists after the status change
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message;
        }
    }

    [RelayCommand]
    private async Task MarkOutForRentAsync(RentalDisplayItem? rentalDisplayItem)
    {
        try
        {
            if (rentalDisplayItem == null)
                return;

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.MarkOutForRentAsync(rentalDisplayItem.Rental.Id, localUserId); // moves approved to out for rent

            StatusMessage = "Rental marked as out for rent.";

            await RefreshRentalListsAsync(); // refreshes both lists after the status change
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message;
        }
    }

    [RelayCommand]
    private async Task MarkReturnedAsync(RentalDisplayItem? rentalDisplayItem)
    {
        try
        {
            if (rentalDisplayItem == null)
                return;

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in borrower's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.MarkReturnedAsync(rentalDisplayItem.Rental.Id, localUserId); // moves out for rent to returned

            StatusMessage = "Rental marked as returned.";

            await RefreshRentalListsAsync(); // refreshes both lists after the status change
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message;
        }
    }

    [RelayCommand]
    private async Task CompleteRentalAsync(RentalDisplayItem? rentalDisplayItem)
    {
        try
        {
            if (rentalDisplayItem == null)
                return;

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.CompleteRentalAsync(rentalDisplayItem.Rental.Id, localUserId); // moves returned to completed

            StatusMessage = "Rental completed successfully.";

            await RefreshRentalListsAsync(); // refreshes both lists after the status change
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private async Task RefreshRentalListsAsync()
    {
        await LoadIncomingRentalsAsync(); // refreshes the owner-side rental list
        await LoadOutgoingRentalsAsync(); // refreshes the borrower-side rental list
    }
}

public class RentalDisplayItem
{
    public Rental Rental { get; } // stores the actual rental record

    public RentalDisplayItem(Rental rental)
    {
        Rental = rental; // stores the rental passed in from the ViewModel
    }

    public bool CanApproveOrReject => Rental.Status == "Requested"; // owner can only approve/reject requested rentals

    public bool CanMarkOutForRent => Rental.Status == "Approved"; // owner can only mark approved rentals as out for rent

    public bool CanMarkReturned => Rental.Status == "Out For Rent"; // borrower can only return out for rent rentals

    public bool CanComplete => Rental.Status == "Returned"; // owner can only complete returned rentals

    public bool CanReview => Rental.Status == "Completed"; // borrower can only review completed rentals
}