using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class CreateRentalRequestViewModel : ObservableObject
{
    private readonly IRentalRepository _rentalRepository; // creates a privately stored repository field. IRentalRepo = type, _rentalRepository = variable name
    private readonly IAuthenticationService _authService; // stores the authentication service so the ViewModel can get the currently logged-in user

    [ObservableProperty] //Generates a public property from the field below and notifies the UI when the value changes
    private int itemId; //stores the ID of the item being requested

    [ObservableProperty]
    private int borrowerId; //stores the ID of the logged-in user making the rental request

    [ObservableProperty]
    private DateTime startDate = DateTime.Today; //requested rental start date

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(1); // this is the requested rental end date

    [ObservableProperty]
    private string statusMessage = string.Empty; //used to show error messages or success messages

    //constructor
    public CreateRentalRequestViewModel(IRentalRepository rentalRepository, IAuthenticationService authService)
    {
        _rentalRepository = rentalRepository; // stores rentalRepository object in _rentalRepository so it can be used through the whole class
        _authService = authService; // stores authService object in _authService so the ViewModel can access the current user

        BorrowerId = _authService.CurrentLocalUserId; // uses the synced local database user ID instead of the API user ID
    }

    public void SetItemId(int id) // lets another page/viewmodel pass in the selected item's ID
    {
        ItemId = id; // stores the selected item's ID in the ViewModel so the user does not need to type it manually
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task CreateRentalRequestAsync() //defines the method that creates a rental request
    {
        try
        {
            var rental = new Rental // creates a new rental object
            {
                ItemId = ItemId, // Left side = property on the new Rental, right side = value currently stored in the ViewModel
                BorrowerId = BorrowerId, // BorrowerId comes from the logged-in user
                StartDate = DateOnly.FromDateTime(StartDate), // converts the DatePicker DateTime into a DateOnly value
                EndDate = DateOnly.FromDateTime(EndDate), // converts the DatePicker DateTime into a DateOnly value
                Status = "Requested"
            };

            await _rentalRepository.CreateAsync(rental); // Sends the new rental request to the repository to save it in the database

            StatusMessage = "Rental request created successfully.";

            StartDate = DateTime.Today; //Resets the start date
            EndDate = DateTime.Today.AddDays(1); //Resets the end date
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the error message so it can be shown on the page instead of crashing
        }
    }
}