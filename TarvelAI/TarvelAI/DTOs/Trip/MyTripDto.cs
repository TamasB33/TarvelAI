using TarvelAI.Models;

namespace TarvelAI.DTOs.Trip;

public sealed class MyTripDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationDays { get; set; }
    public TripStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime BookedAtUtc { get; set; }
    public int HotelBookingsCount { get; set; }
    public int FlightBookingsCount { get; set; }

    /// <summary>Earliest hotel check-in or flight departure (UTC calendar day) for this user's booking; null if no itinerary rows.</summary>
    public DateOnly? ItineraryStartDate { get; set; }
}
