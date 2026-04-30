using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent(); // loads everything from the XAML file
        BindingContext = viewModel; // tells the page to use the ViewModel for bindings
    }
}