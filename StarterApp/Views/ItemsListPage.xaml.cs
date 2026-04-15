using StarterApp.Database.Models;
using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemsListPage : ContentPage
{
    private readonly ItemsListViewModel _viewModel;
    private readonly ItemDetailViewModel _itemDetailViewModel;

    public ItemsListPage(ItemsListViewModel viewModel, ItemDetailViewModel itemDetailViewModel)
    {
        InitializeComponent(); //loads everything from the xaml file
        _viewModel = viewModel; //takes the viewModel passed into the constructor and stores it in the private _viewModel field
        _itemDetailViewModel = itemDetailViewModel; //same idea as above really
        BindingContext = _viewModel; //tells the page "Use _viewModel as the source for all data bindings in the XAML"
    }

    private async void OnItemTapped(object sender, TappedEventArgs e)
    {   
        //checks whether the thing that triggered th even is a Frame. "Sender" is the object that is tapped
        if (sender is not Frame frame) // If sender is a Frame, creates a variable called "frame"
            return; //only runs if the check fails, prevents errors

        if (frame.BindingContext is not Item selectedItem) //If the frames BindingContext an Item then store it in variable SelectedItem
            return; //If the binding context isnt an item, method stops. Prevents errors

        await _itemDetailViewModel.LoadItemFromIdAsync(selectedItem.Id); //Loads the full details for the tapped item
        await Navigation.PushAsync(new ItemDetailPage(_itemDetailViewModel)); //Moves the app to the detail page, pushes the page onto the navigation stack
    }
}