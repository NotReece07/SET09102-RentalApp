using Microsoft.Extensions.DependencyInjection;

namespace StarterApp.Views;

public partial class TempPage : ContentPage
{
    public TempPage()
    {
        InitializeComponent();
    }

    private async void OnOpenItemsListClicked(object sender, EventArgs e)
    {
        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<ItemsListPage>();
        if (page != null)
        {
            await Navigation.PushAsync(page);
        }
    }

    private async void OnOpenCreateItemClicked(object sender, EventArgs e)
    {
        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<CreateItemPage>();
        if (page != null)
        {
            await Navigation.PushAsync(page);
        }
    }

    private async void OnOpenItemDetailClicked(object sender, EventArgs e)
    {
        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<ItemDetailPage>();
        if (page != null)
        {
            await Navigation.PushAsync(page);
        }
    }

    private async void OnOpenEditItemClicked(object sender, EventArgs e)
    {
        var page = Application.Current!.Handler!.MauiContext!.Services.GetService<EditItemPage>();
        if (page != null)
        {
            await Navigation.PushAsync(page);
        }
    }
}