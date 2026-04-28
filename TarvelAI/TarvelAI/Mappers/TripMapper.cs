using TarvelAI.DTOs.Trip;
using TarvelAI.Models;

namespace TarvelAI.Mappers;

public static class TripMapper
{
    public static TripDto ToDto(this Trip t) => new()
    {
        Id           = t.Id,
        Name         = t.Name,
        Destination  = t.Destination,
        Description  = t.Description,
        ImageUrl     = t.ImageUrl,
        BasePrice    = t.BasePrice,
        DurationDays = t.DurationDays,
        Status       = t.Status,
        CreatedAt    = t.CreatedAt,
        CreatedBy    = t.CreatedBy
    };

    public static Trip ToEntity(this CreateTripDto dto) => new()
    {
        Name         = dto.Name,
        Destination  = dto.Destination,
        Description  = dto.Description,
        ImageUrl     = dto.ImageUrl ?? "",
        BasePrice    = dto.BasePrice,
        DurationDays = dto.DurationDays,
        Status       = dto.Status,
        CreatedBy    = dto.CreatedBy,
        CreatedAt    = DateTime.UtcNow,
        UpdatedAt    = DateTime.UtcNow
    };

    public static void UpdateEntity(this UpdateTripDto dto, Trip trip)
    {
        trip.Name         = dto.Name;
        trip.Destination  = dto.Destination;
        trip.Description  = dto.Description;
        trip.ImageUrl     = dto.ImageUrl ?? "";
        trip.BasePrice    = dto.BasePrice;
        trip.DurationDays = dto.DurationDays;
        trip.Status       = dto.Status;
        trip.UpdatedAt    = DateTime.UtcNow;
    }
}
