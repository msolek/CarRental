using CarManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CarManagement.Data;

public static class DatabaseSeeder
{
    // Populates the database with sample cars, customers, and rental history on first run
    public static async Task SeedAsync(CarRentalContext db)
    {
        if (await db.Cars.AnyAsync() || await db.Customers.AnyAsync() || await db.RentalHistories.AnyAsync())
            return;

        var toyota = new Car { Brand = "Toyota", Model = "Yaris", Year = 2023, RentalPricePerDay = 30.00m, IsAvailable = true };
        var honda = new Car { Brand = "Honda", Model = "Civic", Year = 1998, RentalPricePerDay = 100.00m, IsAvailable = true };
        var ford = new Car { Brand = "Ford", Model = "Mustang", Year = 2024, RentalPricePerDay = 85.00m, IsAvailable = true };
        var tesla = new Car { Brand = "Tesla", Model = "Model 3", Year = 2023, RentalPricePerDay = 95.00m, IsAvailable = true };
        var bmw = new Car { Brand = "BMW", Model = "X5", Year = 2023, RentalPricePerDay = 120.00m, IsAvailable = true };

        await db.Cars.AddRangeAsync(toyota, honda, ford, tesla, bmw);
        await db.SaveChangesAsync();

        var seedCustomers = new[]
        {
            new Customer { Name = "Jacek Kowalski", Phone = "536761912", Email = "j.kowalski@gmail.com", Address = "Powstańców 21, Rzeszów", Notes = "Spóznia się z terminem oddania auta." },
            new Customer { Name = "Jan Miodek", Phone = "876876876", Email = "miodek@gmail.com", Address = "Podkarpacka 21, Kraków" },
            new Customer { Name = "Maciej Sołek", Phone = "123761123", Email = "msołek@gmail.com", Address = "Jezuitów 93/12d, Kraków" },
            new Customer { Name = "Joanna Podgórska", Phone = "654876987", Email = "podgórska@gmail.com", Address = "Cicha 21, Warszawa", Notes = "Oddała brudne auto."}
        };
        await db.Customers.AddRangeAsync(seedCustomers);
        await db.SaveChangesAsync();

        // Create some rental history for MS to demonstrate the rental tracking feature
        var maciej = seedCustomers[2];

        var seedRentals = new[]
        {
            new RentalHistory
            {
                CarId = toyota.Id,
                CustomerName = maciej.Name,
                CustomerPhone = maciej.Phone,
                CustomerEmail = maciej.Email,
                RentalDate = new DateTime(2025, 1, 10),
                ReturnDate = new DateTime(2025, 1, 15),
                IsReturned = true
            },
            new RentalHistory
            {
                CarId = honda.Id,
                CustomerName = maciej.Name,
                CustomerPhone = maciej.Phone,
                CustomerEmail = maciej.Email,
                RentalDate = new DateTime(2025, 2, 1),
                ReturnDate = new DateTime(2025, 2, 4),
                IsReturned = true
            },
            // Active rental - Toyota rented
            new RentalHistory
            {
                CarId = toyota.Id,
                CustomerName = maciej.Name,
                CustomerPhone = maciej.Phone,
                CustomerEmail = maciej.Email,
                RentalDate = DateTime.Now.AddDays(-2),
                ReturnDate = null,
                IsReturned = false
            }
        };

        toyota.IsAvailable = false;

        await db.RentalHistories.AddRangeAsync(seedRentals);
        await db.SaveChangesAsync();
    }
}
