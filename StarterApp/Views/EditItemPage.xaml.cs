using StarterApp.ViewModels;

namespace StarterApp.Views; //belongs to the StarterApp.Views namespace

public partial class EditItemPage : ContentPage // EditItemPage = page class, : contentpage = means this page is a MAUI ContentPage
{
    public EditItemPage(EditItemViewModel viewModel)
    {
        InitializeComponent(); //Loads anc connects everything from EditItemPage.xaml. What makes the XAML UI actually get built
        BindingContext = viewModel;
    }
}