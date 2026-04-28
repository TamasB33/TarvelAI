using System.ComponentModel.DataAnnotations;
using TarvelAI.Models;

namespace TarvelAI.DTOs.Trip;

public class UpdateTripDto
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

    public TripStatus Status   { get; set; }
}
