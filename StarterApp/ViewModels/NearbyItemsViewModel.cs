using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class NearbyItemsViewModel : ObservableObject
{
    private readonly IItemRepository _itemRepository; // stores the item repository so nearby items can be loaded from the database
    private readonly ILocationService _locationService; // stores the location service so the ViewModel can ask for the user's current GPS location

    [ObservableProperty]
    private ObservableCollection<Item> nearbyItems = new(); // stores items found within the selected radius

    [ObservableProperty]
    private double radiusKm = 5; // default search radius is 5km, matching the coursework example

    [ObservableProperty]
    private string statusMessage = string.Empty; // used to show success or error messages on the page

    public NearbyItemsViewModel(IItemRepository itemRepository, ILocationService locationService)
    {
        _itemRepository = itemRepository; // stores itemRepository so it can be used through the whole class
        _locationService = locationService; // stores locationService so it can be used through the whole class
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task FindNearbyItemsAsync()
    {
        try
        {
            StatusMessage = "Finding your location...";

            var location = await _locationService.GetCurrentLocationAsync(); // asks the device for its current location

            if (location == null)
            {
                StatusMessage = "Could not get your location. Check location permissions or use the emulator location settings.";
                NearbyItems = new ObservableCollection<Item>();
                return;
            }

            var items = await _itemRepository.GetNearbyAsync(
                location.Latitude,
                location.Longitude,
                RadiusKm); // gets items within the chosen radius

            NearbyItems = new ObservableCollection<Item>(items); // updates the UI with nearby items

            if (NearbyItems.Count == 0)
            {
                StatusMessage = $"No items found within {RadiusKm}km.";
            }
            else
            {
                StatusMessage = $"Found {NearbyItems.Count} item(s) within {RadiusKm}km.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // shows the real error message instead of crashing
        }
    }
}