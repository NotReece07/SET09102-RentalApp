using Microsoft.Extensions.DependencyInjection;
using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemDetailPage : ContentPage
{
    private readonly ItemDetailViewModel _viewModel;

    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.RefreshItemAsync(); // reloads the item each time the detail page appears so edited values are shown
    }

    private async void OnRequestRentalClicked(object sender, EventArgs e)
    {
        if (_viewModel.Item == null)
            return;

        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<CreateRentalRequestPage>();

        if (page?.BindingContext is CreateRentalRequestViewModel rentalViewModel)
        {
            rentalViewModel.SetItemId(_viewModel.Item.Id); // passes the current item's ID into the rental request page
            await Navigation.PushAsync(page);
        }
    }

    private async void OnEditItemClicked(object sender, EventArgs e)
    {
        if (_viewModel.Item == null)
            return;

        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<EditItemPage>();

        if (page?.BindingContext is EditItemViewModel editViewModel)
        {
            await editViewModel.LoadItemFromIdAsync(_viewModel.Item.Id); // loads the selected item into the edit page before opening it
            await Navigation.PushAsync(page);
        }
    }
}