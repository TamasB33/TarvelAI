using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public class Flight
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public required string FlightNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Airline { get; set; }

    [Required]
    [MaxLength(3)]
    public required string OriginAirport { get; set; }

    [Required]
    [MaxLength(3)]
    public required string DestinationAirport { get; set; }

    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation
    public ICollection<FlightBooking> Bookings { get; set; } = [];
}
