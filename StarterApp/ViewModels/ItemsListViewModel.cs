using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class ItemsListViewModel : ObservableObject
{
    // private = only this class can use it, readonly = it can be set once, but not changed later
    private readonly IItemRepository _itemRepository; // IItemRepository = the type of thing it stores, _itemRepository = the variable name
    private readonly StarterApp.Services.INavigationService _navigationService; // stores the navigation service so item taps can navigate through a command instead of code-behind

    [ObservableProperty] // creates a public property, notifies the UI when the value changes
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private Item? selectedItem;

    public ItemsListViewModel(IItemRepository itemRepository, StarterApp.Services.INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
    }

    [RelayCommand] // automatically turns the method below into a command the UI can call/bind to in MVVM
    private async Task LoadItemsAsync()
    {
        var allItems = await _itemRepository.GetAllAsync(); // ask repository for all items, store the results in allItems
        Items = new ObservableCollection<Item>(allItems); // take the list that came back, turn it into an ObservableCollection, store it so the UI can use it
    }

    [RelayCommand]
    private void SelectItem(Item? item)
    {
        if (item == null)
            return;

        SelectedItem = item;
    }

    [RelayCommand] // turns the method below into a command that opens the selected item
    private async Task OpenItemAsync(Item? selectedItem)
    {
        if (selectedItem == null)
        {
            return; // stops if no item was selected
        }

        SelectedItem = selectedItem; // stores the selected item in the ViewModel

        var parameters = new Dictionary<string, object>
        {
            { "itemId", selectedItem.Id }
        }; // passes the selected item ID to the item detail page

        await _navigationService.NavigateToAsync("ItemDetailPage", parameters); // opens the item detail page using Shell navigation
    }
}