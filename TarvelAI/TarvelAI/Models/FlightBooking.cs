using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public class FlightBooking
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public required string CabinClass { get; set; }

    [Range(1, 20)]
    public int Passengers { get; set; } = 1;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string? ConfirmationNumber { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Planned;

    // FK to Trip
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    // FK to Flight
    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;
}
