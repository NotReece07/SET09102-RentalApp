using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context; // stores the database context so the repository can access the reviews table

    // Constructor
    public ReviewRepository(AppDbContext context)
    {
        _context = context; //stores the incoming database context so it can be used through the whole class
    }

    public async Task<List<Review>> GetAllAsync() // Returns all reviews asynchronously
    {
        return await _context.Reviews //Start queerying the reviews table
            .AsNoTracking() //Fetches fresh read only review data from the database
            .Include(r => r.Rental) //Loads the related rental object for reach review
            .Include(r => r.Item)
            .Include(r => r.Reviewer)
            .ToListAsync(); // Turns the query into a list and returns it
    }

    public async Task<Review?> GetByIdAsync(int id) // Get one review by ID
    {
        return await _context.Reviews
            .AsNoTracking() // fetches a fresh read-only review by ID
            .Include(r => r.Rental)
            .Include(r => r.Item)
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id); //Find the first review whose Id matches the given id
    }

    public async Task<List<Review>> GetByItemIdAsync(int itemId)
    {
        return await _context.Reviews
            .AsNoTracking() // fetches fresh reviews for a specific item
            .Include(r => r.Rental)
            .Include(r => r.Item)
            .Include(r => r.Reviewer)
            .Where(r => r.ItemId == itemId) //Only keep reviews where ItemId matches the given item ID
            .ToListAsync(); // Returns those matching reviews as a list
    }

    public async Task<List<Review>> GetByReviewerIdAsync(int reviewerId) // Get all reviews written by one reviewer
    {
        return await _context.Reviews
            .AsNoTracking() // fetches fresh reviews written by a specific user
            .Include(r => r.Rental)
            .Include(r => r.Item)
            .Include(r => r.Reviewer)
            .Where(r => r.ReviewerId == reviewerId)
            .ToListAsync(); // Returns them as a list
    }

    public async Task<Review> CreateAsync(Review review) // Defne a method to create a new review
    {
        _context.Reviews.Add(review); // adds the new review to the database context
        await _context.SaveChangesAsync(); // saves the new review to the database
        return review; // Returns the saved review object
    }

    public async Task UpdateAsync(Review review) //Update an existing review
    {
        var existingReview = await _context.Reviews.FindAsync(review.Id); // finds the tracked version of the review in the database

        if (existingReview == null)
        {
            throw new Exception("Review not found."); // stops the update if the review does not exist
        }


        existingReview.RentalId = review.RentalId; // copies the edited values into the tracked database review
        existingReview.ItemId = review.ItemId;
        existingReview.ReviewerId = review.ReviewerId;
        existingReview.Rating = review.Rating;
        existingReview.Comment = review.Comment;
        existingReview.UpdatedAt = DateTime.UtcNow; //Update the timestamp to show when the review was last edited

        await _context.SaveChangesAsync(); // saves the updated review to the database
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id); // finds the review by its primary key

        if (review != null)
        {
            _context.Reviews.Remove(review); // Removes the review from the database context
            await _context.SaveChangesAsync();
        }
    }
}