namespace TarvelAI.DTOs.Trip;

public sealed class PlanningTripAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Destination { get; set; } = "";
    public bool AdminHotelConfirmed { get; set; }
    public bool AdminAirlineConfirmed { get; set; }
    public int ActiveBookingCount { get; set; }
}
