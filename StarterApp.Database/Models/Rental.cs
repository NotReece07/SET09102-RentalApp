using Microsoft.EntityFrameworkCore; //Lets me use core atributes like PrimaryKey
using System.ComponentModel.DataAnnotations; // Lets me use validation attributes
using System.ComponentModel.DataAnnotations.Schema; // Lets me use database mapping attributes like Table("rentals")
using System.Dynamic;
using System.Net.ServerSentEvents;
using System.Reflection.Metadata;

namespace StarterApp.Database.Models;

[Table("rentals")] // Table name should be rentals
[PrimaryKey(nameof(Id))] //id is the primary key
public class Rental //creates the rental class
{
    public int Id { get; set; } // Unique Id for each rental record

    public int ItemId{ get; set; } //stores the id of the item being rented, also the foreign key linking rental to an item
    public Item? Item{ get; set; } //Navigation property to the full Item object. Code can also access the related item itself

    public int BorrowerId { get; set; } //Stores the id of whoever is borrowing the item
    public User? Borrower { get; set; } //Navigation property to the full User object for the borrower

    public DateOnly StartDate { get; set; } // stores only the rental start date, without time
    public DateOnly EndDate { get; set; } // stores only the rental end date, without time

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Requested;"; //When a new rental is created, its status starts as Requested

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // Stores when the rental record was created, given the current time by default
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow; // Stores when the rental record was last updated, also given the current time
}