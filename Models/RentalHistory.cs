using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarManagement.Models;

public class RentalHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int CarId { get; set; }

    [ForeignKey(nameof(CarId))]
    public Car? Car { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    [Required]
    public DateTime RentalDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned { get; set; }

    // Calculated cost based on how many days the car was (or has been) rented
    [NotMapped]
    public decimal RentalCost
    {
        get
        {
            if (Car == null || Car.RentalPricePerDay == 0) return 0;
            var endDate = ReturnDate ?? DateTime.Now;
            var days = (endDate - RentalDate).Days;
            if (days < 1) days = 1;
            return days * Car.RentalPricePerDay;
        }
    }

    [NotMapped]
    public int RentalDays
    {
        get
        {
            var endDate = ReturnDate ?? DateTime.Now;
            var days = (endDate - RentalDate).Days;
            return days < 1 ? 1 : days;
        }
    }
}
