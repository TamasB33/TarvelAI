using TarvelAI.DTOs.Hotel;
using TarvelAI.Models;

namespace TarvelAI.Mappers;

public static class HotelMapper
{
    public static HotelDto ToDto(this Hotel h) => new()
    {
        Id       = h.Id,
        Name     = h.Name,
        Address  = h.Address,
        City     = h.City,
        Country  = h.Country,
        Rating   = h.Rating,
        ImageUrl = h.ImageUrl
    };

    public static Hotel ToEntity(this CreateHotelDto dto) => new()
    {
        Name     = dto.Name,
        Address  = dto.Address,
        City     = dto.City,
        Country  = dto.Country,
        Rating   = dto.Rating,
        ImageUrl = dto.ImageUrl
    };

    public static void UpdateEntity(this UpdateHotelDto dto, Hotel hotel)
    {
        hotel.Name     = dto.Name;
        hotel.Address  = dto.Address;
        hotel.City     = dto.City;
        hotel.Country  = dto.Country;
        hotel.Rating   = dto.Rating;
        hotel.ImageUrl = dto.ImageUrl;
    }
}
