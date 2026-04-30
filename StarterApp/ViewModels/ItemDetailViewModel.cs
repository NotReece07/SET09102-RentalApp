using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.Views;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : ObservableObject // inherits MVVM support for property change notification
{
    private readonly IItemRepository _itemRepository; // Creates a private stored variable for the item repo
    private readonly IReviewService _reviewService; // stores the review service so this ViewModel can load reviews for the current item
    private readonly IServiceProvider _serviceProvider; // stores the service provider so the ViewModel can open other pages through dependency injection

    private int itemId;

    public int ItemId
    {
        get => itemId;
        set
        {
            itemId = value; // stores the item ID passed from Shell navigation
            _ = LoadItemFromIdAsync(itemId); // loads the item when the page receives the ID
        }
    }

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new(); // stores the reviews for the current item so the UI can display them

    [ObservableProperty]
    private string statusMessage = string.Empty; // Set to empty "" rather than null to be safe

    [ObservableProperty]
    private string reviewStatusMessage = string.Empty; // stores messages specifically for the reviews section

    public ItemDetailViewModel(IItemRepository itemRepository, IReviewService reviewService, IServiceProvider serviceProvider)
    {
        _itemRepository = itemRepository; // stores the incoming repo in the private field so that later methods in the class can use "_itemRepository"
        _reviewService = reviewService; // stores the review service so that this ViewModel can load reviews for this item
        _serviceProvider = serviceProvider; // stores the service provider so this ViewModel can open pages without using button click handlers in code-behind
    }

    [RelayCommand] // Turns the method below into a command the UI can call/bind to in MVVM
    public async Task LoadItemFromIdAsync(int id)
    {
        try // if something goes wrong, execution moves to catch rather than crashing
        {
            Item = await _itemRepository.GetByIdAsync(id); // calls the repo, asks for the item with this id, waits for the database call to finish and stores the result in the item property

            Reviews = new ObservableCollection<Review>(); // clears old reviews when a different item is loaded
            ReviewStatusMessage = string.Empty; // clears old review messages

            if (Item == null)
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

    [RelayCommand]
    private async Task LoadItemAsync(int id)
    {
        await LoadItemFromIdAsync(id);
    }

    [RelayCommand] // Turns the method below into a command that the Request Rental button can call
    private async Task RequestRentalAsync()
    {
        try
        {
            if (Item == null)
            {
                StatusMessage = "No item selected.";
                return;
            }

            var page = _serviceProvider.GetService<CreateRentalRequestPage>(); // gets the rental request page from dependency injection

            if (page?.BindingContext is CreateRentalRequestViewModel rentalViewModel)
            {
                rentalViewModel.SetItemId(Item.Id); // passes the current item's ID into the rental request page
                await Application.Current!.MainPage!.Navigation.PushAsync(page); // opens the rental request page
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // shows the real error message instead of crashing
        }
    }

    [RelayCommand] // Turns the method below into a command that the Edit Item button can call
    private async Task EditItemAsync()
    {
        try
        {
            if (Item == null)
            {
                StatusMessage = "No item selected.";
                return;
            }

            var page = _serviceProvider.GetService<EditItemPage>(); // gets the edit item page from dependency injection

            if (page?.BindingContext is EditItemViewModel editViewModel)
            {
                await editViewModel.LoadItemFromIdAsync(Item.Id); // loads the selected item into the edit page before opening it
                await Application.Current!.MainPage!.Navigation.PushAsync(page); // opens the edit item page
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // shows the real error message instead of crashing
        }
    }

    [RelayCommand] // Turns the method below into a command that the Load Reviews button can call
    private async Task LoadReviewsAsync()
    {
        try
        {
            if (Item == null)
            {
                ReviewStatusMessage = "No item selected.";
                Reviews = new ObservableCollection<Review>();
                return;
            }

            var itemReviews = await _reviewService.GetReviewsForItemAsync(Item.Id); // loads all reviews linked to the current item through the service layer

            Reviews = new ObservableCollection<Review>(itemReviews); // puts the reviews into an ObservableCollection so the UI updates

            if (Reviews.Count == 0)
            {
                ReviewStatusMessage = "No reviews found for this item.";
            }
            else
            {
                ReviewStatusMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ReviewStatusMessage = ex.InnerException?.Message ?? ex.Message; // shows the real error message instead of crashing
        }
    }

    public async Task RefreshItemAsync() // reloads the current item from the database so the detail page shows the newest saved values
    {
        if (Item == null)
            return;

        await LoadItemFromIdAsync(Item.Id); // reuses the existing load logic to refresh the current item by its ID
    }
}