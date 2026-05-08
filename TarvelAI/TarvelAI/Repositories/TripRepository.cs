using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Trip;
using TarvelAI.Mappers;

namespace TarvelAI.Repositories;

public class TripRepository(AppDbContext db) : ITripRepository
{
    public async Task<IEnumerable<TripDto>> GetAllAsync()
    {
        var trips = await db.Trips
            .AsNoTracking()
            .Include(t => t.User)
            .ToListAsync();

        var displayNames = await LoadCreatorDisplayNamesAsync(trips.Select(t => t.CreatedBy));
        return trips.Select(t => MapToDto(t, displayNames)).ToList();
    }

    public async Task<TripDto?> GetByIdAsync(int id)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync();
        if (trip is null) return null;

        var displayNames = await LoadCreatorDisplayNamesAsync([trip.CreatedBy]);
        return MapToDto(trip, displayNames);
    }

    public async Task<TripDto> CreateAsync(CreateTripDto dto)
    {
        var trip = new Models.Trip
        {
            Name = dto.Name,
            Destination = dto.Destination,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl ?? "",
            BasePrice = dto.BasePrice,
            DurationDays = dto.DurationDays,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();
        return await GetByIdAsync(trip.Id) ?? throw new InvalidOperationException("Created trip could not be loaded.");
    }

    public async Task<TripDto?> UpdateAsync(int id, UpdateTripDto dto)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return null;

        dto.UpdateEntity(trip);
        await db.SaveChangesAsync();
        return await GetByIdAsync(trip.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return false;

        db.Trips.Remove(trip);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<Dictionary<string, string>> LoadCreatorDisplayNamesAsync(IEnumerable<string> creatorIds)
    {
        var ids = creatorIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        var claims = await db.UserClaims
            .AsNoTracking()
            .Where(c => ids.Contains(c.UserId) && (c.ClaimType == ClaimTypes.Name || c.ClaimType == "name"))
            .Select(c => new { c.UserId, c.ClaimValue })
            .ToListAsync();

        return claims
            .Where(c => !string.IsNullOrWhiteSpace(c.ClaimValue))
            .GroupBy(c => c.UserId)
            .ToDictionary(g => g.Key, g => g.First().ClaimValue!);
    }

    private static TripDto MapToDto(Models.Trip t, IReadOnlyDictionary<string, string> displayNames) =>
        new()
        {
            Id = t.Id,
            Name = t.Name,
            Destination = t.Destination,
            Description = t.Description,
            ImageUrl = t.ImageUrl,
            BasePrice = t.BasePrice,
            DurationDays = t.DurationDays,
            Status = t.Status,
            CreatedAt = t.CreatedAt,
            CreatedBy = displayNames.TryGetValue(t.CreatedBy, out var displayName)
                ? displayName
                : !string.IsNullOrWhiteSpace(t.User.UserName)
                    ? t.User.UserName
                    : !string.IsNullOrWhiteSpace(t.User.Email)
                        ? t.User.Email
                        : t.CreatedBy
        };
}
