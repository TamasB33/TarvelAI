using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public class Hotel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(250)]
    public required string Address { get; set; }

    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Country { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation
    public ICollection<HotelBooking> Bookings { get; set; } = [];
}
