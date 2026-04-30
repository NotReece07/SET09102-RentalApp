using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemsListPage : ContentPage
{
    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent(); // loads everything from the XAML file
        BindingContext = viewModel; // tells the page to use the ViewModel for bindings
    }
}