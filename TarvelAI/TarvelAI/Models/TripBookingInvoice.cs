using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public sealed class TripBookingInvoice
{
    [Key]
    public int TripBookingId { get; set; }

    public TripBooking TripBooking { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string LegalName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? AddressLine2 { get; set; }

    [Required]
    [MaxLength(120)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Phone { get; set; }
}
