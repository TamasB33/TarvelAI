using System.ComponentModel.DataAnnotations;
using TarvelAI.Models;

namespace TarvelAI.DTOs.Trip;

public class CreateTripDto
{
    [Required, MaxLength(150)]
    public required string Name        { get; set; }

    [Required, MaxLength(150)]
    public required string Destination { get; set; }

    [MaxLength(1000)]
    public string Description  { get; set; } = "";

    [MaxLength(500)]
    public string? ImageUrl    { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BasePrice   { get; set; }

    [Range(1, 365)]
    public int DurationDays    { get; set; }

    public TripStatus Status   { get; set; } = TripStatus.Planning;

    /// <summary>Catalog hotel used for the package template (<see cref="Models.HotelBooking.TripBookingId"/> null).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select a hotel from the catalog.")]
    public int TemplateHotelId { get; set; }

    /// <summary>Catalog flight used for the package template (<see cref="Models.FlightBooking.TripBookingId"/> null).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Select a flight from the catalog.")]
    public int TemplateFlightId { get; set; }

    [Required]
    public required string CreatedBy  { get; set; }
}
