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

    [ObservableProperty] // Generates a public property from the field below and notifies the UI when the value changes
    private ObservableCollection<RentalDisplayItem> outgoingRentals = new(); // stores the rental requests made by the current user, wrapped with button visibility rules

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> incomingRentals = new(); // stores rental requests for items owned by the current user, wrapped with button visibility rules

    [ObservableProperty]
    private string statusMessage = string.Empty; // used to show messages such as errors or "no rentals found"

    // constructor
    public RentalsViewModel(IRentalService rentalService, IAuthenticationService authService)
    {
        _rentalService = rentalService; // stores rentalService so it can be used through the whole class
        _authService = authService; // stores authService so the ViewModel can get the logged-in user's local database ID
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task LoadOutgoingRentalsAsync() // loads rental requests made by the currently logged-in user
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

            var rentals = await _rentalService.GetOutgoingRentalsAsync(localUserId); // gets all rentals where this user is the borrower through the service layer

            OutgoingRentals = new ObservableCollection<RentalDisplayItem>(
                rentals.Select(rental => new RentalDisplayItem(rental))
            ); // wraps each rental so the UI knows which buttons should be visible

            if (OutgoingRentals.Count == 0)
            {
                StatusMessage = "No outgoing rental requests found.";
            }
            else
            {
                StatusMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task LoadIncomingRentalsAsync() // loads rental requests for items owned by the currently logged-in user
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

            var rentals = await _rentalService.GetIncomingRentalsAsync(localUserId); // gets all rentals for items owned by the current user through the service layer

            IncomingRentals = new ObservableCollection<RentalDisplayItem>(
                rentals.Select(rental => new RentalDisplayItem(rental))
            ); // wraps each rental so the UI knows which buttons should be visible

            if (IncomingRentals.Count == 0)
            {
                StatusMessage = "No incoming rental requests found.";
            }
            else
            {
                StatusMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task ApproveRentalAsync(RentalDisplayItem? rentalDisplayItem) // approves a selected rental request if the current user owns the related item
    {
        try
        {
            if (rentalDisplayItem == null)
            {
                return; // stops the command if no rental was passed in
            }

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.ApproveRentalAsync(rentalDisplayItem.Rental.Id, localUserId); // asks the service layer to approve the rental using business rules

            StatusMessage = "Rental approved successfully.";

            await LoadIncomingRentalsAsync(); // refreshes the incoming rental list after the approval
            await LoadOutgoingRentalsAsync(); // refreshes the outgoing rental list too so the borrower-side status is updated
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task CompleteRentalAsync(RentalDisplayItem? rentalDisplayItem) // completes a selected approved rental if the current user is the borrower
    {
        try
        {
            if (rentalDisplayItem == null)
            {
                return; // stops the command if no rental was passed in
            }

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in borrower's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.CompleteRentalAsync(rentalDisplayItem.Rental.Id, localUserId); // asks the service layer to complete the rental using business rules

            StatusMessage = "Rental completed successfully.";

            await LoadOutgoingRentalsAsync(); // refreshes the outgoing rental list after completion
            await LoadIncomingRentalsAsync(); // refreshes the incoming rental list too so the owner-side status is updated
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task RejectRentalAsync(RentalDisplayItem? rentalDisplayItem) // rejects a selected rental request if the current user owns the related item
    {
        try
        {
            if (rentalDisplayItem == null)
            {
                return; // stops the command if no rental was passed in
            }

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.RejectRentalAsync(rentalDisplayItem.Rental.Id, localUserId); // asks the service layer to reject the rental using business rules

            StatusMessage = "Rental rejected successfully.";

            await LoadIncomingRentalsAsync(); // refreshes the incoming rental list after the rejection
            await LoadOutgoingRentalsAsync(); // refreshes the outgoing rental list too so the borrower-side status is updated
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
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

    public bool CanComplete => Rental.Status == "Approved"; // borrower can only complete approved rentals

    public bool CanReview => Rental.Status == "Completed"; // borrower can only review completed rentals
}