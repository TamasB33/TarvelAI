using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Hotel;
using TarvelAI.Models;

namespace TarvelAI.Repositories;

public class HotelRepository(AppDbContext db) : IHotelRepository
{
    public async Task<IEnumerable<HotelDto>> GetAllAsync()
    {
        return await db.Hotels
            .AsNoTracking()
            .Select(h => ToDto(h))
            .ToListAsync();
    }

    public async Task<HotelDto?> GetByIdAsync(int id)
    {
        var hotel = await db.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        return hotel is null ? null : ToDto(hotel);
    }

    public async Task<HotelDto> CreateAsync(CreateHotelDto dto)
    {
        var hotel = new Hotel
        {
            Name     = dto.Name,
            Address  = dto.Address,
            City     = dto.City,
            Country  = dto.Country,
            Rating   = dto.Rating,
            ImageUrl = dto.ImageUrl
        };

        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
        return ToDto(hotel);
    }

    public async Task<HotelDto?> UpdateAsync(int id, UpdateHotelDto dto)
    {
        var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == id);
        if (hotel is null) return null;

        hotel.Name     = dto.Name;
        hotel.Address  = dto.Address;
        hotel.City     = dto.City;
        hotel.Country  = dto.Country;
        hotel.Rating   = dto.Rating;
        hotel.ImageUrl = dto.ImageUrl;

        await db.SaveChangesAsync();
        return ToDto(hotel);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == id);
        if (hotel is null) return false;

        db.Hotels.Remove(hotel);
        await db.SaveChangesAsync();
        return true;
    }

    private static HotelDto ToDto(Hotel h) => new()
    {
        Id       = h.Id,
        Name     = h.Name,
        Address  = h.Address,
        City     = h.City,
        Country  = h.Country,
        Rating   = h.Rating,
        ImageUrl = h.ImageUrl
    };
}
