using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class CreateItemViewModel : ObservableObject
{
    private readonly IItemRepository _itemRepository; // IItemRepositiry = type it stores, _itemRepository = the variable name

    [ObservableProperty] // Generates a public property (e.g. Title) from the field below and notifies the UI when the value changes
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal dailyRate;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string locationName = string.Empty;

    [ObservableProperty]
    private double latitude;

    [ObservableProperty]
    private double longitude;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    private readonly IAuthenticationService _authService; // stores the auth service so the ViewModel can get the logged-in user's local database ID
    
    // Constructor
    public CreateItemViewModel(IItemRepository itemRepository, IAuthenticationService authService)
    {
        _itemRepository = itemRepository; // stores the item repository so it can be used through the whole class
        _authService = authService; // stores the auth service so the ViewModel can access the logged-in user's local database ID
    }

    [RelayCommand] // Automatically turns the method below into a comand the UI can call or bind
    private async Task CreateItemAsync()
    {
        try
        {
            // Take what the user typed in, turn it into an item, save it, then reset the form
            var item = new Item
            {
                // Left side = property on the new Item (Item.Title), Right side = value from the ViewModel (Created via [ObservableProperty])
                Title = Title, //Creates a new item and fills it with the values currently in ViewModel (Empty)
                Description = Description,
                DailyRate = DailyRate,
                Category = Category,
                LocationName = LocationName,
                Latitude = Latitude,
                Longitude = Longitude,

                OwnerId = _authService.CurrentLocalUserId
            };

            await _itemRepository.CreateAsync(item); //Sends that item to the repository to save it in the database

            StatusMessage = "Item created sucessfully."; // basic message I can show on the page

            // clears form after the item has been created
            Title = string.Empty;
            Description = string.Empty;
            DailyRate = 0;
            Category = string.Empty;
            LocationName = string.Empty;
            Latitude = 0;
            Longitude = 0;
        }
        catch (Exception ex) // If something fails, this stores an error message
        {
            StatusMessage = $"Error: {ex}";
        }
    }
}