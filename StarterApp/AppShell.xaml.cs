using StarterApp.ViewModels;
using StarterApp.Views;

namespace StarterApp;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent(); // loads the XAML for AppShell
        BindingContext = viewModel; // connects the shell to its ViewModel

        // registers routes so Shell navigation can find these pages by name
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(UserListPage), typeof(UserListPage));
        Routing.RegisterRoute(nameof(UserDetailPage), typeof(UserDetailPage));
        Routing.RegisterRoute(nameof(TempPage), typeof(TempPage));

        Routing.RegisterRoute(nameof(ItemsListPage), typeof(ItemsListPage));
        Routing.RegisterRoute(nameof(CreateItemPage), typeof(CreateItemPage));
        Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
        Routing.RegisterRoute(nameof(EditItemPage), typeof(EditItemPage));
        Routing.RegisterRoute(nameof(CreateRentalRequestPage), typeof(CreateRentalRequestPage));
        Routing.RegisterRoute(nameof(RentalsPage), typeof(RentalsPage));
    }
}