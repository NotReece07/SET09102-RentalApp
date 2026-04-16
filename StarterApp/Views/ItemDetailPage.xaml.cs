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

    private async void OnRequestRentalClicked(object sender, EventArgs e)
    {
        if (_viewModel.Item == null)
            return;

        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<CreateRentalRequestPage>();

        if (page?.BindingContext is CreateRentalRequestViewModel rentalViewModel)
        {
            rentalViewModel.SetItemId(_viewModel.Item.Id);
            await Navigation.PushAsync(page);
        }
    }
}