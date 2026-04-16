using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateRentalRequestPage : ContentPage
{
    public CreateRentalRequestPage(CreateRentalRequestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}