using StarterApp.Database.Models;
using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    private readonly NearbyItemsViewModel _viewModel;
    private readonly ItemDetailViewModel _itemDetailViewModel;

    public NearbyItemsPage(NearbyItemsViewModel viewModel, ItemDetailViewModel itemDetailViewModel)
    {
        InitializeComponent(); // loads everything from the XAML file
        _viewModel = viewModel; // stores the nearby items ViewModel
        _itemDetailViewModel = itemDetailViewModel; // stores the item detail ViewModel so selected items can be opened
        BindingContext = _viewModel; // tells the page to use the ViewModel for bindings
    }

    private async void OnNearbyItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Frame frame)
            return;

        if (frame.BindingContext is not Item selectedItem)
            return;

        await _itemDetailViewModel.LoadItemFromIdAsync(selectedItem.Id); // loads the full selected item details
        await Navigation.PushAsync(new ItemDetailPage(_itemDetailViewModel)); // opens the item detail page
    }
}