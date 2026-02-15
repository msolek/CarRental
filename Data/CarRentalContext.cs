using CarManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace CarManagement.Data;

public class CarRentalContext : DbContext
{
    // Shared connection keeps the in-memory SQLite database alive across multiple context instances
    private static readonly SqliteConnection _connection;

    static CarRentalContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<RentalHistory> RentalHistories { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RentalHistory>()
            .HasOne(r => r.Car)
            .WithMany()
            .HasForeignKey(r => r.CarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
