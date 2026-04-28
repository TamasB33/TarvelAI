using TarvelAI.Models;

namespace TarvelAI.DTOs.Trip;

public class TripDto
{
    public int        Id           { get; set; }
    public string     Name         { get; set; } = "";
    public string     Destination  { get; set; } = "";
    public string     Description  { get; set; } = "";
    public string?    ImageUrl     { get; set; }
    public decimal    BasePrice    { get; set; }
    public int        DurationDays { get; set; }
    public TripStatus Status       { get; set; }
    public DateTime   CreatedAt    { get; set; }
    public string     CreatedBy    { get; set; } = "";
}
