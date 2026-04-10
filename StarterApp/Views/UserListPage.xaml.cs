using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class UserListPage : ContentPage
{
    public UserListPage(UserListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}