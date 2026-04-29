using TarvelAI.DTOs.Trip;
using TarvelAI.Repositories;

namespace TarvelAI.Endpoints;

public static class TripEndpoints
{
    public static void MapTripEndpoints(this WebApplication app)
    {
        // GET endpoints — any authenticated user
        var readGroup = app.MapGroup("/api/trips").RequireAuthorization();

        // GET /api/trips
        readGroup.MapGet("/", async (ITripRepository repo) =>
        {
            var trips = await repo.GetAllAsync();
            return Results.Ok(trips);
        });

        // GET /api/trips/{id}
        readGroup.MapGet("/{id:int}", async (int id, ITripRepository repo) =>
        {
            var trip = await repo.GetByIdAsync(id);
            return trip is null ? Results.NotFound() : Results.Ok(trip);
        });

        // Write endpoints — Admin only
        var adminGroup = app.MapGroup("/api/trips").RequireAuthorization("Admin");

        // POST /api/trips
        adminGroup.MapPost("/", async (CreateTripDto dto, ITripRepository repo) =>
        {
            var created = await repo.CreateAsync(dto);
            return Results.Created($"/api/trips/{created.Id}", created);
        });

        // PUT /api/trips/{id}
        adminGroup.MapPut("/{id:int}", async (int id, UpdateTripDto dto, ITripRepository repo) =>
        {
            var updated = await repo.UpdateAsync(id, dto);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        // DELETE /api/trips/{id}
        adminGroup.MapDelete("/{id:int}", async (int id, ITripRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
