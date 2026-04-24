using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateReviewPage : ContentPage
{
    public CreateReviewPage(CreateReviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}