using TarvelAI.DTOs.Flight;
using TarvelAI.Models;

namespace TarvelAI.Mappers;

public static class FlightMapper
{
    public static FlightDto ToDto(this Flight f) => new()
    {
        Id                 = f.Id,
        FlightNumber       = f.FlightNumber,
        Airline            = f.Airline,
        OriginAirport      = f.OriginAirport,
        DestinationAirport = f.DestinationAirport,
        DepartureTime      = f.DepartureTime,
        ArrivalTime        = f.ArrivalTime,
        ImageUrl           = f.ImageUrl
    };

    public static Flight ToEntity(this CreateFlightDto dto) => new()
    {
        FlightNumber       = dto.FlightNumber,
        Airline            = dto.Airline,
        OriginAirport      = dto.OriginAirport,
        DestinationAirport = dto.DestinationAirport,
        DepartureTime      = dto.DepartureTime,
        ArrivalTime        = dto.ArrivalTime,
        ImageUrl           = dto.ImageUrl
    };

    public static void UpdateEntity(this UpdateFlightDto dto, Flight flight)
    {
        flight.FlightNumber       = dto.FlightNumber;
        flight.Airline            = dto.Airline;
        flight.OriginAirport      = dto.OriginAirport;
        flight.DestinationAirport = dto.DestinationAirport;
        flight.DepartureTime      = dto.DepartureTime;
        flight.ArrivalTime        = dto.ArrivalTime;
        flight.ImageUrl           = dto.ImageUrl;
    }
}
