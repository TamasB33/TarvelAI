using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Flight;
using TarvelAI.Mappers;
using TarvelAI.Models;

namespace TarvelAI.Repositories;

public class FlightRepository(AppDbContext db) : IFlightRepository
{
    public async Task<IEnumerable<FlightDto>> GetAllAsync()
    {
        return await db.Flights
            .AsNoTracking()
            .Select(f => f.ToDto())
            .ToListAsync();
    }

    public async Task<FlightDto?> GetByIdAsync(int id)
    {
        var flight = await db.Flights.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        return flight?.ToDto();
    }

    public async Task<FlightDto> CreateAsync(CreateFlightDto dto)
    {
        var flight = dto.ToEntity();
        db.Flights.Add(flight);
        await db.SaveChangesAsync();
        return flight.ToDto();
    }

    public async Task<FlightDto?> UpdateAsync(int id, UpdateFlightDto dto)
    {
        var flight = await db.Flights.FirstOrDefaultAsync(f => f.Id == id);
        if (flight is null) return null;

        dto.UpdateEntity(flight);
        await db.SaveChangesAsync();
        return flight.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var flight = await db.Flights.FirstOrDefaultAsync(f => f.Id == id);
        if (flight is null) return false;

        db.Flights.Remove(flight);
        await db.SaveChangesAsync();
        return true;
    }
}
