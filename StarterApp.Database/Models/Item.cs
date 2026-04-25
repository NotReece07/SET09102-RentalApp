using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace StarterApp.Database.Models;

[Table("items")] //titles the table
[PrimaryKey(nameof(Id))] //sets the primary key as "id"
public class Item
{
    public int Id{ get; set; } // allows the property to be gotten from other code, and set by other code

    [Required] //means it cant be null
    [MaxLength(100)] // its obvious what this does cmon now
    public string Title{ get; set; } = string.Empty; // string.Empty sets the title property to "" rather than null, which prevents issues in the future.

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")] // stores Daily rate as a decimal with up to 10 total digits, and 2 digits after decimal point (eg, 10.50, 99999,99)
    public decimal DailyRate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(200)]
    public string LocationName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public Point? Location { get; set; } // stores the real PostGIS spatial point used for nearby search

    public int OwnerId { get; set; }

    public User? Owner { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // Created at this exact moment, allowed to be null for flexibility
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public bool IsAvailable { get; set; } = true;

}