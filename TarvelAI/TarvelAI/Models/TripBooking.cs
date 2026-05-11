using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public sealed class TripBooking
{
    public int Id { get; set; }

    [Required]
    public int TripId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime BookedAtUtc { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;

    public ICollection<HotelBooking> UserHotelBookings { get; set; } = [];
    public ICollection<FlightBooking> UserFlightBookings { get; set; } = [];

    public TripBookingInvoice? Invoice { get; set; }
}
