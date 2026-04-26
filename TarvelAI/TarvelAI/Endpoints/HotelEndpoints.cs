using TarvelAI.DTOs.Hotel;
using TarvelAI.Repositories;

namespace TarvelAI.Endpoints;

public static class HotelEndpoints
{
    public static void MapHotelEndpoints(this WebApplication app)
    {
        // GET endpoints — any authenticated user can view
        var readGroup = app.MapGroup("/api/hotels").RequireAuthorization();

        // GET /api/hotels
        readGroup.MapGet("/", async (IHotelRepository repo) =>
        {
            var hotels = await repo.GetAllAsync();
            return Results.Ok(hotels);
        });

        // GET /api/hotels/{id}
        readGroup.MapGet("/{id:int}", async (int id, IHotelRepository repo) =>
        {
            var hotel = await repo.GetByIdAsync(id);
            return hotel is null ? Results.NotFound() : Results.Ok(hotel);
        });

        // Write endpoints — Admin only
        var adminGroup = app.MapGroup("/api/hotels").RequireAuthorization("Admin");

        // POST /api/hotels
        adminGroup.MapPost("/", async (CreateHotelDto dto, IHotelRepository repo) =>
        {
            var created = await repo.CreateAsync(dto);
            return Results.Created($"/api/hotels/{created.Id}", created);
        });

        // PUT /api/hotels/{id}
        adminGroup.MapPut("/{id:int}", async (int id, UpdateHotelDto dto, IHotelRepository repo) =>
        {
            var updated = await repo.UpdateAsync(id, dto);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        // DELETE /api/hotels/{id}
        adminGroup.MapDelete("/{id:int}", async (int id, IHotelRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
