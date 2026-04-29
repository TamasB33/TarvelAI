using System.ComponentModel.DataAnnotations;

namespace TarvelAI.DTOs.Flight;

public class UpdateFlightDto
{
    [Required(ErrorMessage = "Flight number is required.")]
    [MaxLength(10, ErrorMessage = "Flight number cannot exceed 10 characters.")]
    public string FlightNumber { get; set; } = "";

    [Required(ErrorMessage = "Airline is required.")]
    [MaxLength(100, ErrorMessage = "Airline cannot exceed 100 characters.")]
    public string Airline { get; set; } = "";

    [Required(ErrorMessage = "Origin airport is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Origin airport must be a 3-letter IATA code e.g. LHR.")]
    public string OriginAirport { get; set; } = "";

    [Required(ErrorMessage = "Destination airport is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Destination airport must be a 3-letter IATA code e.g. JFK.")]
    public string DestinationAirport { get; set; } = "";

    [Required(ErrorMessage = "Departure time is required.")]
    public DateTime DepartureTime { get; set; }

    [Required(ErrorMessage = "Arrival time is required.")]
    public DateTime ArrivalTime { get; set; }

    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    public string? ImageUrl { get; set; }
}
