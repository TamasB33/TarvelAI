using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Trip;
using TarvelAI.Mappers;

namespace TarvelAI.Repositories;

public class TripRepository(AppDbContext db) : ITripRepository
{
    public async Task<IEnumerable<TripDto>> GetAllAsync()
    {
        return await db.Trips
            .AsNoTracking()
            .Select(t => t.ToDto())
            .ToListAsync();
    }

    public async Task<TripDto?> GetByIdAsync(int id)
    {
        var trip = await db.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return trip?.ToDto();
    }

    public async Task<TripDto> CreateAsync(CreateTripDto dto)
    {
        var trip = dto.ToEntity();
        db.Trips.Add(trip);
        await db.SaveChangesAsync();
        return trip.ToDto();
    }

    public async Task<TripDto?> UpdateAsync(int id, UpdateTripDto dto)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return null;

        dto.UpdateEntity(trip);
        await db.SaveChangesAsync();
        return trip.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return false;

        db.Trips.Remove(trip);
        await db.SaveChangesAsync();
        return true;
    }
}
