using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace StarterApp.Database.Models;

[Table("reviews")] //Table name is reviews
[PrimaryKey(nameof(Id))] //id is the primary key
public class Review
{
    public int Id { get; set; } //unique id for each review

    public int RentalId { get; set; } // Stores the id of the rental this review belongs to
    public Rental? Rental { get; set; } //Navigation property to the full Rental object, can be null

    public int ItemId { get; set; }
    public Item? Item { get; set; }

    public int ReviewerId { get; set; } //Stores the Id of the user who wrote the review
    public User? Reviewer { get; set; } //Navigation property to the full User object for the reviewer

    [Range(1, 5)] //Next property must be between 1 and 5
    public int Rating { get; set; } // Stores the review rating

    [MaxLength(1000)] //Limits next text field to 1000 characters
    public string Comment { get; set; } = String.Empty; //Stores writen review, starts as empty string rather than null

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // Stores when review was created
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow; // Stores when review was updated
}