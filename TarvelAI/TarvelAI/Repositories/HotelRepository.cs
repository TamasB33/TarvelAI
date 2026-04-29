using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Hotel;
using TarvelAI.Mappers;
using TarvelAI.Models;

namespace TarvelAI.Repositories;

public class HotelRepository(AppDbContext db) : IHotelRepository
{
    public async Task<IEnumerable<HotelDto>> GetAllAsync()
    {
        return await db.Hotels
            .AsNoTracking()
            .Select(h => h.ToDto())
            .ToListAsync();
    }

    public async Task<HotelDto?> GetByIdAsync(int id)
    {
        var hotel = await db.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        return hotel?.ToDto();
    }

    public async Task<HotelDto> CreateAsync(CreateHotelDto dto)
    {
        var hotel = dto.ToEntity();
        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
        return hotel.ToDto();
    }

    public async Task<HotelDto?> UpdateAsync(int id, UpdateHotelDto dto)
    {
        var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == id);
        if (hotel is null) return null;

        dto.UpdateEntity(hotel);
        await db.SaveChangesAsync();
        return hotel.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == id);
        if (hotel is null) return false;

        db.Hotels.Remove(hotel);
        await db.SaveChangesAsync();
        return true;
    }
}
