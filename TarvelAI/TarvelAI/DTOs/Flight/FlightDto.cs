namespace TarvelAI.DTOs.Flight;

public class FlightDto
{
    public int      Id                  { get; set; }
    public string   FlightNumber        { get; set; } = "";
    public string   Airline             { get; set; } = "";
    public string   OriginAirport       { get; set; } = "";
    public string   DestinationAirport  { get; set; } = "";
    public DateTime DepartureTime       { get; set; }
    public DateTime ArrivalTime         { get; set; }
    public string?  ImageUrl            { get; set; }
}
