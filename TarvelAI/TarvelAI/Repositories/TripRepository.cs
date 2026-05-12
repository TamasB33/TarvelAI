using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Trip;
using TarvelAI.Mappers;
using TarvelAI.Models;

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
            .FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return null;

        var displayNames = await LoadCreatorDisplayNamesAsync([trip.CreatedBy]);
        return MapToDto(trip, displayNames);
    }

    public async Task<(int? HotelId, int? FlightId)> GetTemplateLinkIdsAsync(int tripId)
    {
        var hotelId = await db.HotelBookings.AsNoTracking()
            .Where(h => h.TripId == tripId && h.TripBookingId == null)
            .OrderBy(h => h.Id)
            .Select(h => (int?)h.HotelId)
            .FirstOrDefaultAsync();

        var flightId = await db.FlightBookings.AsNoTracking()
            .Where(f => f.TripId == tripId && f.TripBookingId == null)
            .OrderBy(f => f.Id)
            .Select(f => (int?)f.FlightId)
            .FirstOrDefaultAsync();

        return (hotelId, flightId);
    }

    public async Task<TripDto> CreateAsync(CreateTripDto dto)
    {
        await EnsureValidTemplateReferencesAsync(dto.TemplateHotelId, dto.TemplateFlightId);

        var trip = new Models.Trip
        {
            Name = dto.Name,
            Destination = dto.Destination,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl ?? "",
            BasePrice = dto.BasePrice,
            DurationDays = dto.DurationDays,
            Status = dto.Status,
            AdminHotelConfirmed = false,
            AdminAirlineConfirmed = false,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        AddDefaultTemplateBookings(trip.Id, dto.TemplateHotelId, dto.TemplateFlightId, trip.BasePrice, trip.DurationDays);
        await db.SaveChangesAsync();

        return await GetByIdAsync(trip.Id) ?? throw new InvalidOperationException("Created trip could not be loaded.");
    }

    public async Task<TripDto?> UpdateAsync(int id, UpdateTripDto dto)
    {
        await EnsureValidTemplateReferencesAsync(dto.TemplateHotelId, dto.TemplateFlightId);

        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return null;

        var (currentHotelId, currentFlightId) = await GetTemplateLinkIdsAsync(id);
        var needTemplateWrite = dto.TemplateHotelId != currentHotelId
            || dto.TemplateFlightId != currentFlightId
            || currentHotelId is null
            || currentFlightId is null;

        if (needTemplateWrite)
        {
            var hasBookings = await db.TripBookings.AnyAsync(tb => tb.TripId == id);
            if (hasBookings)
            {
                throw new InvalidOperationException(
                    "Cannot change or add package hotel/flight while the trip has user bookings.");
            }

            var oldHotels = await db.HotelBookings.Where(h => h.TripId == id && h.TripBookingId == null).ToListAsync();
            var oldFlights = await db.FlightBookings.Where(f => f.TripId == id && f.TripBookingId == null).ToListAsync();
            db.HotelBookings.RemoveRange(oldHotels);
            db.FlightBookings.RemoveRange(oldFlights);
            await db.SaveChangesAsync();

            AddDefaultTemplateBookings(id, dto.TemplateHotelId, dto.TemplateFlightId, dto.BasePrice, dto.DurationDays);
            await db.SaveChangesAsync();
        }

        dto.UpdateEntity(trip);

        if (dto.Status == TripStatus.Available && !await HasPublishTemplatesAsync(id))
        {
            throw new InvalidOperationException(
                "A trip can only be set to Available after linking at least one hotel and one flight template.");
        }

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

    private async Task<bool> HasPublishTemplatesAsync(int tripId)
    {
        var hasHotelTemplate = await db.HotelBookings.AnyAsync(h => h.TripId == tripId && h.TripBookingId == null);
        if (!hasHotelTemplate) return false;

        var hasFlightTemplate = await db.FlightBookings.AnyAsync(f => f.TripId == tripId && f.TripBookingId == null);
        return hasFlightTemplate;
    }

    private async Task EnsureValidTemplateReferencesAsync(int hotelId, int flightId)
    {
        if (hotelId < 1 || flightId < 1)
            throw new InvalidOperationException("Select a hotel and a flight from the catalog.");

        if (!await db.Hotels.AsNoTracking().AnyAsync(h => h.Id == hotelId))
            throw new InvalidOperationException("Selected hotel was not found.");

        if (!await db.Flights.AsNoTracking().AnyAsync(f => f.Id == flightId))
            throw new InvalidOperationException("Selected flight was not found.");
    }

    /// <summary>Package template rows (TripBookingId null), aligned with <see cref="Data.TripSeeder"/> heuristics.</summary>
    private void AddDefaultTemplateBookings(
        int tripId,
        int hotelId,
        int flightId,
        decimal basePrice,
        int durationDays)
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14));
        var checkOut = checkIn.AddDays(durationDays);
        var nightly = Math.Round(
            Math.Max(120m, basePrice / Math.Max(1, durationDays) * 0.35m),
            2,
            MidpointRounding.AwayFromZero);

        db.HotelBookings.Add(new HotelBooking
        {
            TripId = tripId,
            HotelId = hotelId,
            RoomType = "Standard Double",
            Guests = 2,
            NumberOfRooms = 1,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            PricePerNight = nightly,
            TotalPrice = nightly * durationDays,
            Status = BookingStatus.Planned,
            ConfirmationNumber = null,
            TripBookingId = null
        });

        db.FlightBookings.Add(new FlightBooking
        {
            TripId = tripId,
            FlightId = flightId,
            CabinClass = "Economy",
            Passengers = 2,
            Price = Math.Round(Math.Max(180m, basePrice * 0.45m), 2, MidpointRounding.AwayFromZero),
            Status = BookingStatus.Planned,
            ConfirmationNumber = null,
            TripBookingId = null
        });
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
            AdminHotelConfirmed = t.AdminHotelConfirmed,
            AdminAirlineConfirmed = t.AdminAirlineConfirmed,
            CreatedAt = t.CreatedAt,
            CreatedBy = displayNames.TryGetValue(t.CreatedBy, out var displayName)
                ? displayName
                : !string.IsNullOrWhiteSpace(t.User.UserName)
                    ? t.User.UserName
                    : !string.IsNullOrWhiteSpace(t.User.Email)
                        ? t.User.Email
                        : t.CreatedBy
        };

    public async Task<TripBookingOperationResult> BookTripAsync(int tripId, string userId, TripBillingInput? billing = null)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            if (billing is not null && !TripBillingInput.HasAllRequiredFields(billing))
            {
                await transaction.RollbackAsync();
                return TripBookingOperationResult.IncompleteBilling;
            }

            var tripEntity = await db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
            if (tripEntity is null)
            {
                await transaction.RollbackAsync();
                return TripBookingOperationResult.TripNotFound;
            }

            var alreadyBooked = await db.TripBookings.AnyAsync(tb => tb.TripId == tripId && tb.UserId == userId);
            if (alreadyBooked)
            {
                await transaction.RollbackAsync();
                return TripBookingOperationResult.AlreadyBooked;
            }

            if (tripEntity.Status != TripStatus.Available)
            {
                await transaction.RollbackAsync();
                return TripBookingOperationResult.TripNotAvailable;
            }

            var tripBooking = new Models.TripBooking
            {
                TripId = tripId,
                UserId = userId,
                BookedAtUtc = DateTime.UtcNow
            };

            db.TripBookings.Add(tripBooking);
            await db.SaveChangesAsync();

            var hotelTemplates = await db.HotelBookings
                .Where(h => h.TripId == tripId && h.TripBookingId == null)
                .ToListAsync();

            var flightTemplates = await db.FlightBookings
                .Where(f => f.TripId == tripId && f.TripBookingId == null)
                .ToListAsync();

            foreach (var template in hotelTemplates)
            {
                db.HotelBookings.Add(new HotelBooking
                {
                    TripId = template.TripId,
                    HotelId = template.HotelId,
                    RoomType = template.RoomType,
                    Guests = template.Guests,
                    NumberOfRooms = template.NumberOfRooms,
                    CheckInDate = template.CheckInDate,
                    CheckOutDate = template.CheckOutDate,
                    PricePerNight = template.PricePerNight,
                    TotalPrice = template.TotalPrice,
                    Status = BookingStatus.Booked,
                    ConfirmationNumber = $"HTL-{tripBooking.Id}-{template.HotelId}",
                    TripBookingId = tripBooking.Id
                });
            }

            foreach (var template in flightTemplates)
            {
                db.FlightBookings.Add(new FlightBooking
                {
                    TripId = template.TripId,
                    FlightId = template.FlightId,
                    CabinClass = template.CabinClass,
                    Passengers = template.Passengers,
                    Price = template.Price,
                    Status = BookingStatus.Booked,
                    ConfirmationNumber = $"FLT-{tripBooking.Id}-{template.FlightId}",
                    TripBookingId = tripBooking.Id
                });
            }

            if (billing is not null)
            {
                // Attach via navigation so EF ties the 1:1 dependent to the already-tracked TripBooking.
                tripBooking.Invoice = new TripBookingInvoice
                {
                    LegalName = billing.LegalName.Trim(),
                    AddressLine1 = billing.AddressLine1.Trim(),
                    AddressLine2 = string.IsNullOrWhiteSpace(billing.AddressLine2) ? null : billing.AddressLine2.Trim(),
                    City = billing.City.Trim(),
                    PostalCode = billing.PostalCode.Trim(),
                    Country = billing.Country.Trim(),
                    Email = billing.Email.Trim(),
                    Phone = string.IsNullOrWhiteSpace(billing.Phone) ? null : billing.Phone.Trim()
                };
            }

            tripEntity.Status = TripStatus.Planning;
            tripEntity.AdminHotelConfirmed = false;
            tripEntity.AdminAirlineConfirmed = false;
            tripEntity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return TripBookingOperationResult.Success;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TripBookingOperationResult> UnbookTripAsync(int tripId, string userId)
    {
        var booking = await db.TripBookings
            .Include(tb => tb.Trip)
            .Include(tb => tb.UserHotelBookings)
            .Include(tb => tb.UserFlightBookings).ThenInclude(f => f.Flight)
            .FirstOrDefaultAsync(tb => tb.TripId == tripId && tb.UserId == userId);

        if (booking is null)
        {
            var tripExists = await db.Trips.AnyAsync(t => t.Id == tripId);
            return tripExists
                ? TripBookingOperationResult.BookingNotFound
                : TripBookingOperationResult.TripNotFound;
        }

        if (IsConfirmedCancellationBlocked(booking.Trip.Status, ComputeItineraryStart(booking)))
            return TripBookingOperationResult.CancellationNotAllowedTripInProgress;

        db.TripBookings.Remove(booking);
        await db.SaveChangesAsync();

        var anyBookingsLeft = await db.TripBookings.AnyAsync(tb => tb.TripId == tripId);
        if (!anyBookingsLeft)
        {
            var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
            if (trip is not null)
            {
                trip.Status = TripStatus.Available;
                trip.AdminHotelConfirmed = false;
                trip.AdminAirlineConfirmed = false;
                trip.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        return TripBookingOperationResult.Success;
    }

    public async Task<IReadOnlyList<PlanningTripAdminDto>> GetPlanningTripsForAdminAsync() =>
        await db.Trips
            .AsNoTracking()
            .Where(t => t.Status == TripStatus.Planning && t.TripBookings.Any())
            .OrderBy(t => t.Id)
            .Select(t => new PlanningTripAdminDto
            {
                Id = t.Id,
                Name = t.Name,
                Destination = t.Destination,
                AdminHotelConfirmed = t.AdminHotelConfirmed,
                AdminAirlineConfirmed = t.AdminAirlineConfirmed,
                ActiveBookingCount = t.TripBookings.Count
            })
            .ToListAsync();

    public async Task<TripPlanningAdminResult> AdminConfirmPlanningHotelAsync(int tripId)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return TripPlanningAdminResult.TripNotFound;
        if (trip.Status != TripStatus.Planning) return TripPlanningAdminResult.NotInPlanningState;

        trip.AdminHotelConfirmed = true;
        trip.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return TripPlanningAdminResult.Success;
    }

    public async Task<TripPlanningAdminResult> AdminConfirmPlanningAirlineAsync(int tripId)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return TripPlanningAdminResult.TripNotFound;
        if (trip.Status != TripStatus.Planning) return TripPlanningAdminResult.NotInPlanningState;
        if (!trip.AdminHotelConfirmed) return TripPlanningAdminResult.AirlineRequiresHotelConfirmation;

        trip.AdminAirlineConfirmed = true;
        trip.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return TripPlanningAdminResult.Success;
    }

    public async Task<TripPlanningAdminResult> AdminConfirmPlanningTripAsync(int tripId)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return TripPlanningAdminResult.TripNotFound;
        if (trip.Status != TripStatus.Planning) return TripPlanningAdminResult.NotInPlanningState;
        if (!trip.AdminHotelConfirmed || !trip.AdminAirlineConfirmed)
            return TripPlanningAdminResult.FinalRequiresBothConfirmations;

        trip.Status = TripStatus.Confirmed;
        trip.AdminHotelConfirmed = false;
        trip.AdminAirlineConfirmed = false;
        trip.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return TripPlanningAdminResult.Success;
    }

    public async Task<IEnumerable<MyTripDto>> GetMyBookedTripsAsync(string userId)
    {
        var rows = await db.TripBookings
            .AsNoTracking()
            .Where(tb => tb.UserId == userId)
            .Include(tb => tb.Trip)
            .Include(tb => tb.UserHotelBookings)
            .Include(tb => tb.UserFlightBookings).ThenInclude(f => f.Flight)
            .OrderByDescending(tb => tb.BookedAtUtc)
            .ToListAsync();

        return rows.Select(tb => new MyTripDto
        {
            Id = tb.Trip.Id,
            Name = tb.Trip.Name,
            Destination = tb.Trip.Destination,
            Description = tb.Trip.Description,
            ImageUrl = tb.Trip.ImageUrl,
            BasePrice = tb.Trip.BasePrice,
            DurationDays = tb.Trip.DurationDays,
            Status = tb.Trip.Status,
            CreatedAt = tb.Trip.CreatedAt,
            BookedAtUtc = tb.BookedAtUtc,
            HotelBookingsCount = tb.UserHotelBookings.Count,
            FlightBookingsCount = tb.UserFlightBookings.Count,
            ItineraryStartDate = ComputeItineraryStart(tb)
        }).ToList();
    }

    private static DateOnly? ComputeItineraryStart(Models.TripBooking booking)
    {
        DateOnly? min = null;
        foreach (var h in booking.UserHotelBookings)
            min = min is null ? h.CheckInDate : (min.Value < h.CheckInDate ? min.Value : h.CheckInDate);
        foreach (var f in booking.UserFlightBookings)
        {
            var d = DateOnly.FromDateTime(f.Flight.DepartureTime);
            min = min is null ? d : (min.Value < d ? min.Value : d);
        }

        return min;
    }

    private static bool IsConfirmedCancellationBlocked(TripStatus tripStatus, DateOnly? itineraryStart)
    {
        if (tripStatus != TripStatus.Confirmed) return false;
        if (itineraryStart is null) return false;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return itineraryStart.Value <= today;
    }
}
