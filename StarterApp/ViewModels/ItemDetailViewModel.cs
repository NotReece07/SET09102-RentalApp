using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.ViewModels;

public partial class ItemDetailViewModel : ObservableObject // inherits MVVM support for property change notification
{
    private readonly IItemRepository _itemRepository; //Creates a private stores variable for the item repo

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private string statusMessage = string.Empty; //Set to empty "" rather than null to be safe

    public ItemDetailViewModel(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository; //Stores the incoming repo in the private field so that later methods in the class can use "_itemRepository"
    }

    [RelayCommand] // Turns the method below into a command the UI can call.bind to in MVVM
    private async Task LoadItemAsync(int id)
    {
        try // if something goes wrong, execution moves to catch rather than crashing
        {
            item = await _itemRepository.GetByIdAsync(id); //calls the repo, asks for the item with this id, wait for the database call to finish and store the results in the item property

            if (item == null)
            {
                StatusMessage = "Item not found.";
            }
            else
            {
                StatusMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}