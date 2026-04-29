using TarvelAI.DTOs.Flight;
using TarvelAI.Repositories;

namespace TarvelAI.Endpoints;

public static class FlightEndpoints
{
    public static void MapFlightEndpoints(this WebApplication app)
    {
        // GET endpoints — any authenticated user
        var readGroup = app.MapGroup("/api/flights").RequireAuthorization();

        // GET /api/flights
        readGroup.MapGet("/", async (IFlightRepository repo) =>
        {
            var flights = await repo.GetAllAsync();
            return Results.Ok(flights);
        });

        // GET /api/flights/{id}
        readGroup.MapGet("/{id:int}", async (int id, IFlightRepository repo) =>
        {
            var flight = await repo.GetByIdAsync(id);
            return flight is null ? Results.NotFound() : Results.Ok(flight);
        });

        // Write endpoints — Admin only
        var adminGroup = app.MapGroup("/api/flights").RequireAuthorization("Admin");

        // POST /api/flights
        adminGroup.MapPost("/", async (CreateFlightDto dto, IFlightRepository repo) =>
        {
            var created = await repo.CreateAsync(dto);
            return Results.Created($"/api/flights/{created.Id}", created);
        });

        // PUT /api/flights/{id}
        adminGroup.MapPut("/{id:int}", async (int id, UpdateFlightDto dto, IFlightRepository repo) =>
        {
            var updated = await repo.UpdateAsync(id, dto);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        // DELETE /api/flights/{id}
        adminGroup.MapDelete("/{id:int}", async (int id, IFlightRepository repo) =>
        {
            var deleted = await repo.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
