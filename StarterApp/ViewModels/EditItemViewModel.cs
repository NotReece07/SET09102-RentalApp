using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class EditItemViewModel : ObservableObject // EditItemViewModel = class name, ObservableObject = gives it property-change notifactions for MVVM
{
    private readonly IItemRepository _itemRepository; //type and variable name
    private readonly IAuthenticationService _authService; // stores the auth service so the ViewModel can check who the logged-in local user is

    private Item? _currentItem; // Stores item currently being edited

    [ObservableProperty]
    private string title = string.Empty; //creates a bindable Title property from the field title

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

    public EditItemViewModel(IItemRepository itemRepository, IAuthenticationService authService) // Constructor. Runs when the viewmodel is created and asks for an IItemRepository. Dependency injection will provide it.
    {
        _itemRepository = itemRepository; // Stores the incoming repo in the private field so the rest of the class can use it later
        _authService = authService; // stores the auth service so the rest of the class can check the logged-in user's local ID
    }

    [RelayCommand] //Relay command makes this produce a command the UI can call
    private async Task LoadItemAsync(int id) // LoadItemASync = Method name, 
    {
        try
        {
            _currentItem = await _itemRepository.GetByIdAsync(id); // asks the repository for the item with this ID, waits for the results and stores it in _currentItem

            if (_currentItem == null) // checks if no item was found
            {
                StatusMessage = "Item not found.";
                return;
            }

            if (_currentItem.OwnerId != _authService.CurrentLocalUserId) // checks whether the logged-in local user actually owns this item
            {
                StatusMessage = "You can only edit your own items.";
                _currentItem = null; // clears the current item so update cannot continue on a non-owned item
                return;
            }

            // These load the items titlte into the ViewModels editable Title property.
            // After loading, The user sees the current values AND the user can edit them
            Title = _currentItem.Title;
            Description = _currentItem.Description;
            DailyRate = _currentItem.DailyRate;
            Category = _currentItem.Category;
            LocationName = _currentItem.LocationName;
            Latitude = _currentItem.Latitude;
            Longitude = _currentItem.Longitude;

            StatusMessage = string.Empty; // clears any old status message if everything loaded successfully
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand] // Automatically turns the method below into a command the UI can call/bind to in MVVM
    // Defines an async method to save the edited item
    private async Task UpdateItemAsync() //No id needed because the item should already be loaded into _currentItem
    {
        try
        {
            if (_currentItem == null) // makes sure there an item loaded before trying to update
            {
                StatusMessage = "No item loaded.";
                return;
            }

            if (_currentItem.OwnerId != _authService.CurrentLocalUserId) // double-checks ownership again before saving, just in case a non-owner somehow reaches this point
            {
                StatusMessage = "You can only edit your own items.";
                return;
            }

            // These lines push the edited form values back into the actual item object
            _currentItem.Title = Title;
            _currentItem.Description = Description;
            _currentItem.DailyRate = DailyRate;
            _currentItem.Category = Category;
            _currentItem.LocationName = LocationName;
            _currentItem.Latitude = Latitude;
            _currentItem.Longitude = Longitude;

            await _itemRepository.UpdateAsync(_currentItem); //sends the updated item to the repository to save the changes in the database

            StatusMessage = "Item updated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public async Task LoadItemFromIdAsync(int id) // lets another page load an item into this ViewModel by item ID
        {
            await LoadItemAsync(id); // reuses the existing owner-checking load logic already in this ViewModel
        }
}