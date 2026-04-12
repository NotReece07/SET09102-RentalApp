using System.Net.ServerSentEvents;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data;

public class AppDbContext : DbContext
{

    public AppDbContext()
    { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("StarterApp.Database.appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            connectionString = config.GetConnectionString("DevelopmentConnection");
        }

        optionsBuilder.UseNpgsql(connectionString);
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Item> Items { get; set; } //represents the Items table in the database (I want to store and access Item objects in the db)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure UserRole entity
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        // database rules for the "Item" table
        modelBuilder.Entity<Item>(entity => // entity is just a variable name, => means "goes to". Together = pass this object into the code block so we can configure it
        {
            entity.Property(e => e.Title).HasMaxLength(100); // e => e.title could also be writen as item => item.Title. "e" is just a temp name
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.LocationName).HasMaxLength(200);
            entity.Property(e => e.DailyRate).HasColumnType("decimal(10,2)"); // once again, just letting the daily rate be up to 10 numbers, with 2 after the decimal point

            entity.HasOne(e => e.Owner) //One item has one related user. That related user is stored in the Owner Propertty (one item, one owner)
                .WithMany() //User can be linked to many items (One user, many items)
                .HasForeignKey(e => e.OwnerId) //The link is made using the OwnerId field in the "Item" database
                .OnDelete(DeleteBehavior.Restrict); //Means do not auto delete items if the related user is deleted. Restrict that delete. (Safety rule)
        });
    }

}