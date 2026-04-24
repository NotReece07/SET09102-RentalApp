using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class CreateReviewViewModel : ObservableObject
{
    private readonly IReviewService _reviewService; // stores the review service so the ViewModel can create reviews through business rules
    private readonly IAuthenticationService _authService; // stores the auth service so the ViewModel can get the logged-in local user ID

    [ObservableProperty] // Generates a public property from the field below and notifies the UI when the value changes
    private int rentalId; // stores the rental ID the review is linked to

    [ObservableProperty]
    private int itemId; // Stores the item Id the review is about

    [ObservableProperty]
    private int reviewerId; // stores the logged in local user ID for the reviewer

    [ObservableProperty]
    private int rating = 5; //stores the rating value, defaulting to 5

    [ObservableProperty]
    private string comment = string.Empty; // stores the written review comment

    [ObservableProperty]
    private string statusMessage = string.Empty; // used to show success or error messages on the page

    // Constructor
    public CreateReviewViewModel(IReviewService reviewService, IAuthenticationService authService)
    {
        _reviewService = reviewService; // stores reviewService so it can be used through the whole class
        _authService = authService; // stores authService so the ViewModel can get the logged-in local user ID

        ReviewerId = _authService.CurrentLocalUserId; // automatically sets the reviewer to the currently logged-in local user
    }

    public void SetRentalDetails(int rentalId, int itemId) // lets the rentals page pass in the selected rental and item
    {
        RentalId = rentalId; // stores the rental ID so the user does not need to type it manually
        ItemId = itemId; // stores the item ID so the user does not need to type it manually
        ReviewerId = _authService.CurrentLocalUserId; // refreshes the reviewer ID in case the logged-in user changed
        StatusMessage = string.Empty; // clears any old message from the page
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task CreateReviewAsync() // creates and saves a new review
    {
        try
        {
            ReviewerId = _authService.CurrentLocalUserId; // refreshes the logged-in local user ID before saving

            await _reviewService.CreateReviewAsync(
                RentalId,
                ReviewerId,
                Rating,
                Comment); // sends the review details to the service so business rules are checked before saving

            StatusMessage = "Review created successfully.";

            Rating = 5; // resets the rating back to the default
            Comment = string.Empty; // clears the comment box
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }
}