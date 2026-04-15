using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.ViewModels;

public partial class ItemsListViewModel : ObservableObject
{
    // private = only this class can use it, readonly = it can be set once, buy not changed later
    private readonly IItemRepository _itemRepository; // IItemRepository = the type of thing it stores, _itemRepository = the variable name

    [ObservableProperty] //creates a public property, notifies the UI when the value changes
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private Item? selectedItem;

    public ItemsListViewModel(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    [RelayCommand] // automatically turns the method below into a command the UI can call/bind to in MVVM
    private async Task LoadItemsAsync()
    {
        var allItems = await _itemRepository.GetAllAsync(); //ask repository for all items, store the results in allItems
        Items = new ObservableCollection<Item>(allItems); //take the list that came back, turn it into an Observable collection, store it in the viewmodels items collection so the UI can use it
    }

    [RelayCommand]
    private void SelectItem(Item? item)
    {
        if (item==null)
        return;

        SelectedItem = item;
    }
}