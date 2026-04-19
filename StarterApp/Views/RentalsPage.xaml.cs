using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RentalsPage : ContentPage
{
    public RentalsPage(RentalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}