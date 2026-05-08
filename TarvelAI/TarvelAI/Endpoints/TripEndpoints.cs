using TarvelAI.DTOs.Trip;
using TarvelAI.Repositories;
using System.Security.Claims;

namespace TarvelAI.Endpoints;

public static class TripEndpoints
{
    public static void MapTripEndpoints(this WebApplication app)
    {
        // GET endpoints — public for explore browsing
        var readGroup = app.MapGroup("/api/trips");

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

        // GET /api/trips/my
        readGroup.MapGet("/my", async (ClaimsPrincipal user, ITripRepository repo) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var trips = await repo.GetMyBookedTripsAsync(userId);
            return Results.Ok(trips);
        });

        // POST /api/trips/{id}/book
        readGroup.MapPost("/{id:int}/book", async (int id, ClaimsPrincipal user, ITripRepository repo) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var result = await repo.BookTripAsync(id, userId);
            return result switch
            {
                TripBookingOperationResult.Success => Results.Ok(new { message = "Trip booked successfully." }),
                TripBookingOperationResult.TripNotFound => Results.NotFound(new { message = "Trip not found." }),
                TripBookingOperationResult.AlreadyBooked => Results.Conflict(new { message = "Trip is already booked by this user." }),
                _ => Results.BadRequest()
            };
        });

        // DELETE /api/trips/{id}/book
        readGroup.MapDelete("/{id:int}/book", async (int id, ClaimsPrincipal user, ITripRepository repo) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var result = await repo.UnbookTripAsync(id, userId);
            return result switch
            {
                TripBookingOperationResult.Success => Results.NoContent(),
                TripBookingOperationResult.TripNotFound => Results.NotFound(new { message = "Trip not found." }),
                TripBookingOperationResult.BookingNotFound => Results.NotFound(new { message = "Booking not found for this user." }),
                _ => Results.BadRequest()
            };
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
