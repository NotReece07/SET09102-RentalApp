using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : ObservableObject
{
    private readonly IRentalRepository _rentalRepository; // stores the rental repository so the ViewModel can load rental records from the database
    private readonly IItemRepository _itemRepository; // stores the item repository so the ViewModel can find items owned by the current user
    private readonly IAuthenticationService _authService; // stores the auth service so the ViewModel can get the logged-in user's local database ID

    [ObservableProperty] // Generates a public property from the field below and notifies the UI when the value changes
    private ObservableCollection<Rental> outgoingRentals = new(); // stores the rental requests made by the current user

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new(); // stores rental requests for items owned by the current user

    [ObservableProperty]
    private string statusMessage = string.Empty; // used to show messages such as errors or "no rentals found"

    // constructor
    public RentalsViewModel(IRentalRepository rentalRepository, IItemRepository itemRepository, IAuthenticationService authService)
    {
        _rentalRepository = rentalRepository; // stores rentalRepository so it can be used through the whole class
        _itemRepository = itemRepository; // stores itemRepository so it can be used through the whole class
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

            var rentals = await _rentalRepository.GetByBorrowerIdAsync(localUserId); // gets all rentals where this user is the borrower

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

            var ownedItems = await _itemRepository.GetByOwnerIdAsync(localUserId); // gets all items owned by the current user

            var incomingList = new List<Rental>(); // temporary list used to collect rentals for all owned items

            foreach (var item in ownedItems)
            {
                var itemRentals = await _rentalRepository.GetByItemIdAsync(item.Id); // gets all rentals linked to this owned item
                incomingList.AddRange(itemRentals); // adds those rentals into the combined incoming list
            }

            IncomingRentals = new ObservableCollection<Rental>(incomingList); // puts the combined incoming list into an ObservableCollection so the UI can display it

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
}