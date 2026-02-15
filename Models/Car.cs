using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarManagement.Models;

public class Car
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RentalPricePerDay { get; set; }

    public bool IsAvailable { get; set; } = true;
}
