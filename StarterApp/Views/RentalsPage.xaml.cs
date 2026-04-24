using Microsoft.Extensions.DependencyInjection;
using StarterApp.Database.Models;
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
        if (e.Parameter is not Rental selectedRental)
            return;

        if (selectedRental.Item == null)
            return;

        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<ItemDetailPage>();

        if (page?.BindingContext is ItemDetailViewModel itemDetailViewModel)
        {
            await itemDetailViewModel.LoadItemFromIdAsync(selectedRental.Item.Id);
            await Navigation.PushAsync(page);
        }
    }
}