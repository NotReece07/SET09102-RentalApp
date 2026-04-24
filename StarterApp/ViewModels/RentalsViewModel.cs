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
    private ObservableCollection<Rental> outgoingRentals = new(); // stores the rental requests made by the current user

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new(); // stores rental requests for items owned by the current user

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
                OutgoingRentals = new ObservableCollection<Rental>();
                return;
            }

            var rentals = await _rentalService.GetOutgoingRentalsAsync(localUserId); // gets all rentals where this user is the borrower through the service layer

            OutgoingRentals = new ObservableCollection<Rental>(rentals); // puts the rental list into an ObservableCollection so the UI can display it

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
                IncomingRentals = new ObservableCollection<Rental>();
                return;
            }

            var rentals = await _rentalService.GetIncomingRentalsAsync(localUserId); // gets all rentals for items owned by the current user through the service layer

            IncomingRentals = new ObservableCollection<Rental>(rentals); // puts the combined incoming list into an ObservableCollection so the UI can display it

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
    private async Task ApproveRentalAsync(Rental? rental) // approves a selected rental request if the current user owns the related item
    {
        try
        {
            if (rental == null)
            {
                return; // stops the command if no rental was passed in
            }

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.ApproveRentalAsync(rental.Id, localUserId); // asks the service layer to approve the rental using business rules

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
    private async Task CompleteRentalAsync(Rental? rental) // completes a selected approved rental if the current user is the borrower
    {
        try
        {
            if (rental == null)
            {
                return; // stops the command if no rental was passed in
            }

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in borrower's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.CompleteRentalAsync(rental.Id, localUserId); // asks the service layer to complete the rental using business rules

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
    private async Task RejectRentalAsync(Rental? rental) // rejects a selected rental request if the current user owns the related item
    {
        try
        {
            if (rental == null)
            {
                return; // stops the command if no rental was passed in
            }

            var localUserId = _authService.CurrentLocalUserId; // gets the logged-in owner's local database ID

            if (localUserId == 0)
            {
                StatusMessage = "No local user found.";
                return;
            }

            await _rentalService.RejectRentalAsync(rental.Id, localUserId); // asks the service layer to reject the rental using business rules

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