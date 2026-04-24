using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class CreateReviewViewModel : ObservableObject
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAuthenticationService _authService;

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
    public CreateReviewViewModel(IReviewRepository reviewRepository, IAuthenticationService authService)
    {
        _reviewRepository = reviewRepository; // stores reviewRepository so it can be used through the whole class
        _authService = authService; // stores authService so the ViewModel can get the logged-in local user ID

        reviewerId = _authService.CurrentLocalUserId; // automatically sets the reviewer to the currently logged-in local user
    }

    [RelayCommand] // Turns the method below into a command the UI can bind to
    private async Task CreateReviewAsync() // creates and saves a new review
    {
        try
        {
            var review = new Review // creates a new review object
            {
                RentalId = RentalId, // links the review to a rental
                ItemId = ItemId, // links the review to an item
                ReviewerId = ReviewerId, // links the review to the logged-in reviewer
                Rating = Rating, // stores the selected rating
                Comment = Comment // stores the written comment
            }; 

            await _reviewRepository.CreateAsync(review);

            statusMessage = "Review created successfully.";

            RentalId = 0; // resets the rental ID field
            ItemId = 0; // resets the item ID field
            Rating = 5; // resets the rating back to the default
            Comment = string.Empty; // clears the comment box
        }
        catch (Exception ex)
        {
            StatusMessage = ex.InnerException?.Message ?? ex.Message; // stores the real error message so it can be shown on the page instead of crashing
        }
    }
}