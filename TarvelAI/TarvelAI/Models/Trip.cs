using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public class Trip
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Destination { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [Range(1, 365)]
    public int DurationDays { get; set; }

    public TripStatus Status { get; set; } = TripStatus.Planning;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // FK to IdentityUser
    [Required]
    public required string CreatedBy { get; set; }
    public IdentityUser User { get; set; } = null!;

    // Navigation
    public ICollection<HotelBooking> HotelBookings { get; set; } = [];
    public ICollection<FlightBooking> FlightBookings { get; set; } = [];
}
