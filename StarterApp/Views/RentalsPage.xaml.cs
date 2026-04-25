using Microsoft.Extensions.DependencyInjection;
using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RentalsPage : ContentPage
{
    public RentalsPage(RentalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnIncomingRentalTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not RentalDisplayItem selectedRentalDisplayItem)
            return;

        var selectedRental = selectedRentalDisplayItem.Rental; // gets the actual rental from the display wrapper

        if (selectedRental.Item == null)
            return;

        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<ItemDetailPage>();

        if (page?.BindingContext is ItemDetailViewModel itemDetailViewModel)
        {
            await itemDetailViewModel.LoadItemFromIdAsync(selectedRental.Item.Id);
            await Navigation.PushAsync(page);
        }
    }

    private async void OnLeaveReviewClicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not RentalDisplayItem selectedRentalDisplayItem)
            return;

        var selectedRental = selectedRentalDisplayItem.Rental; // gets the actual rental from the display wrapper

        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<CreateReviewPage>();

        if (page?.BindingContext is CreateReviewViewModel reviewViewModel)
        {
            reviewViewModel.SetRentalDetails(selectedRental.Id, selectedRental.ItemId); // passes rental/item IDs into the review page
            await Navigation.PushAsync(page);
        }
    }
}