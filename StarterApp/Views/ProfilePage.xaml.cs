using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel; // stores the profile ViewModel so the page can refresh it when appearing

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel; // stores the injected ViewModel
        BindingContext = _viewModel; // connects the page to the ViewModel for XAML bindings
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadProfileAsync(); // refreshes the profile and average rating whenever the page opens
    }
}