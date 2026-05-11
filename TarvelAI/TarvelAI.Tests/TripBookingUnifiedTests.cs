using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TarvelAI.Data;
using TarvelAI.DTOs.Trip;
using TarvelAI.Models;
using TarvelAI.Repositories;
using Xunit;

namespace TarvelAI.Tests;

public sealed class TripBookingUnifiedTests
{
    private static async Task<(AppDbContext Db, TripRepository Repo)> SeedWithTemplatesAsync(AppDbContext db)
    {
        const string creatorId = "creator-1";
        const string guestId = "guest-1";
        db.Users.AddRange(
            NewUser(creatorId, "creator@x.com"),
            NewUser(guestId, "guest@x.com"));
        db.Hotels.Add(new Hotel
        {
            Name = "H",
            Address = "a",
            City = "c",
            Country = "c",
            Rating = 4
        });
        db.Flights.Add(new Flight
        {
            FlightNumber = "FN1",
            Airline = "A",
            OriginAirport = "AAA",
            DestinationAirport = "BBB",
            DepartureTime = DateTime.UtcNow,
            ArrivalTime = DateTime.UtcNow.AddHours(2)
        });
        await db.SaveChangesAsync();

        var hotelId = db.Hotels.Single().Id;
        var flightId = db.Flights.Single().Id;

        db.Trips.Add(new Trip
        {
            Name = "T",
            Destination = "D",
            BasePrice = 100,
            DurationDays = 3,
            Status = TripStatus.Available,
            CreatedBy = creatorId
        });
        await db.SaveChangesAsync();
        var tripId = db.Trips.Single().Id;

        db.HotelBookings.Add(new HotelBooking
        {
            TripId = tripId,
            HotelId = hotelId,
            RoomType = "Std",
            CheckInDate = new DateOnly(2026, 6, 1),
            CheckOutDate = new DateOnly(2026, 6, 5),
            PricePerNight = 50,
            ConfirmationNumber = "TPL-H"
        });
        db.FlightBookings.Add(new FlightBooking
        {
            TripId = tripId,
            FlightId = flightId,
            CabinClass = "Eco",
            Price = 200,
            ConfirmationNumber = "TPL-F"
        });
        await db.SaveChangesAsync();

        return (db, new TripRepository(db));
    }

    private static IdentityUser NewUser(string id, string email) =>
        new()
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = "x",
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0
        };

    [Fact]
    public async Task Book_twice_same_user_returns_AlreadyBooked_and_no_duplicate_rows()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "guest-1"));
        var tripAfterBook = await db.Trips.AsNoTracking().SingleAsync(t => t.Id == tripId);
        Assert.Equal(TripStatus.Planning, tripAfterBook.Status);
        Assert.False(tripAfterBook.AdminHotelConfirmed);
        Assert.False(tripAfterBook.AdminAirlineConfirmed);

        Assert.Equal(TripBookingOperationResult.AlreadyBooked, await repo.BookTripAsync(tripId, "guest-1"));

        Assert.Single(db.TripBookings.Where(tb => tb.UserId == "guest-1"));
        var tb = db.TripBookings.Single(x => x.UserId == "guest-1");
        Assert.Single(db.HotelBookings.Where(h => h.TripBookingId == tb.Id));
        Assert.Single(db.FlightBookings.Where(f => f.TripBookingId == tb.Id));
    }

    [Fact]
    public async Task Unbook_removes_user_rows_and_keeps_templates()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "guest-1"));

        var templateHotelCount = await db.HotelBookings.CountAsync(h => h.TripId == tripId && h.TripBookingId == null);
        var templateFlightCount = await db.FlightBookings.CountAsync(f => f.TripId == tripId && f.TripBookingId == null);
        Assert.True(templateHotelCount > 0 && templateFlightCount > 0);

        Assert.Equal(TripBookingOperationResult.Success, await repo.UnbookTripAsync(tripId, "guest-1"));

        Assert.False(await db.TripBookings.AnyAsync(tb => tb.UserId == "guest-1"));
        Assert.Equal(templateHotelCount, await db.HotelBookings.CountAsync(h => h.TripId == tripId && h.TripBookingId == null));
        Assert.Equal(templateFlightCount, await db.FlightBookings.CountAsync(f => f.TripId == tripId && f.TripBookingId == null));
        Assert.False(db.HotelBookings.Any(h => h.TripBookingId != null));
        Assert.False(db.FlightBookings.Any(f => f.TripBookingId != null));

        var tripAfterUnbook = await db.Trips.AsNoTracking().SingleAsync(t => t.Id == tripId);
        Assert.Equal(TripStatus.Available, tripAfterUnbook.Status);
        Assert.False(tripAfterUnbook.AdminHotelConfirmed);
        Assert.False(tripAfterUnbook.AdminAirlineConfirmed);
    }

    [Fact]
    public async Task Book_with_no_templates_still_Success()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();

        db.Users.Add(NewUser("c1", "c@x.com"));
        db.Trips.Add(new Trip
        {
            Name = "EmptyPkg",
            Destination = "X",
            BasePrice = 1,
            DurationDays = 1,
            Status = TripStatus.Available,
            CreatedBy = "c1"
        });
        await db.SaveChangesAsync();
        var tripId = db.Trips.Single().Id;

        var repo = new TripRepository(db);
        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "c1"));
        Assert.Single(db.TripBookings);
        Assert.Empty(db.HotelBookings.Where(h => h.TripBookingId != null));
        Assert.Empty(db.FlightBookings.Where(f => f.TripBookingId != null));
    }

    [Fact]
    public async Task Trip_not_available_rejected_without_TripBooking()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);

        var trip = db.Trips.Single();
        trip.Status = TripStatus.Planning;
        await db.SaveChangesAsync();

        Assert.Equal(TripBookingOperationResult.TripNotAvailable, await repo.BookTripAsync(trip.Id, "guest-1"));
        Assert.Empty(db.TripBookings.Where(tb => tb.UserId == "guest-1"));
    }

    [Fact]
    public async Task GetMyBookedTrips_counts_only_user_rows()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;
        await repo.BookTripAsync(tripId, "guest-1");

        var dto = (await repo.GetMyBookedTripsAsync("guest-1")).Single();
        Assert.Equal(1, dto.HotelBookingsCount);
        Assert.Equal(1, dto.FlightBookingsCount);
    }

    [Fact]
    public async Task Book_with_billing_persists_invoice_row()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        var billing = new TripBillingInput
        {
            LegalName = "Test User",
            AddressLine1 = "1 Main St",
            City = "Budapest",
            PostalCode = "1011",
            Country = "Hungary",
            Email = "test@example.com",
            Phone = "+361234567"
        };

        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "guest-1", billing));

        var booking = await db.TripBookings.SingleAsync(tb => tb.UserId == "guest-1");
        var invoice = await db.TripBookingInvoices.SingleAsync(i => i.TripBookingId == booking.Id);
        Assert.Equal("Test User", invoice.LegalName);
        Assert.Equal("test@example.com", invoice.Email);
        Assert.Equal("+361234567", invoice.Phone);
    }

    [Fact]
    public async Task Book_with_incomplete_billing_returns_IncompleteBilling_and_no_booking()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        var billing = new TripBillingInput
        {
            LegalName = "Only name",
            AddressLine1 = "",
            City = "",
            PostalCode = "",
            Country = "",
            Email = ""
        };

        Assert.Equal(TripBookingOperationResult.IncompleteBilling, await repo.BookTripAsync(tripId, "guest-1", billing));
        Assert.False(await db.TripBookings.AnyAsync(tb => tb.UserId == "guest-1"));
    }

    [Fact]
    public async Task Admin_planning_confirm_sequence_sets_trip_Confirmed_and_clears_flags()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "guest-1"));

        Assert.Equal(TripPlanningAdminResult.Success, await repo.AdminConfirmPlanningHotelAsync(tripId));
        Assert.Equal(TripPlanningAdminResult.Success, await repo.AdminConfirmPlanningAirlineAsync(tripId));

        var beforeFinal = await db.Trips.AsNoTracking().SingleAsync(t => t.Id == tripId);
        Assert.Equal(TripStatus.Planning, beforeFinal.Status);
        Assert.True(beforeFinal.AdminHotelConfirmed);
        Assert.True(beforeFinal.AdminAirlineConfirmed);

        Assert.Equal(TripPlanningAdminResult.Success, await repo.AdminConfirmPlanningTripAsync(tripId));

        var after = await db.Trips.AsNoTracking().SingleAsync(t => t.Id == tripId);
        Assert.Equal(TripStatus.Confirmed, after.Status);
        Assert.False(after.AdminHotelConfirmed);
        Assert.False(after.AdminAirlineConfirmed);
    }

    [Fact]
    public async Task Admin_confirm_airline_before_hotel_is_rejected()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;
        await repo.BookTripAsync(tripId, "guest-1");

        Assert.Equal(TripPlanningAdminResult.AirlineRequiresHotelConfirmation,
            await repo.AdminConfirmPlanningAirlineAsync(tripId));
    }

    [Fact]
    public async Task GetPlanningTripsForAdmin_lists_only_Planning_with_bookings()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        Assert.Empty(await repo.GetPlanningTripsForAdminAsync());

        await repo.BookTripAsync(tripId, "guest-1");
        var rows = await repo.GetPlanningTripsForAdminAsync();
        Assert.Single(rows);
        Assert.Equal(tripId, rows[0].Id);
        Assert.Equal(1, rows[0].ActiveBookingCount);
    }

    [Fact]
    public async Task Unbook_Confirmed_on_or_after_itinerary_start_is_blocked()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "guest-1"));

        var trip = db.Trips.Single();
        trip.Status = TripStatus.Confirmed;
        var tb = await db.TripBookings.SingleAsync(x => x.UserId == "guest-1");
        var hotel = await db.HotelBookings.FirstAsync(h => h.TripBookingId == tb.Id);
        hotel.CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync();

        Assert.Equal(TripBookingOperationResult.CancellationNotAllowedTripInProgress,
            await repo.UnbookTripAsync(tripId, "guest-1"));
        Assert.Single(db.TripBookings);
    }

    [Fact]
    public async Task Unbook_Confirmed_before_itinerary_start_succeeds()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await connection.OpenAsync();
        await db.Database.EnsureCreatedAsync();
        var (_, repo) = await SeedWithTemplatesAsync(db);
        var tripId = db.Trips.Single().Id;

        Assert.Equal(TripBookingOperationResult.Success, await repo.BookTripAsync(tripId, "guest-1"));

        var trip = db.Trips.Single();
        trip.Status = TripStatus.Confirmed;
        var tb = await db.TripBookings.SingleAsync(x => x.UserId == "guest-1");
        var hotel = await db.HotelBookings.FirstAsync(h => h.TripBookingId == tb.Id);
        hotel.CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
        var flightBooking = await db.FlightBookings.Include(f => f.Flight).FirstAsync(f => f.TripBookingId == tb.Id);
        flightBooking.Flight.DepartureTime = DateTime.UtcNow.AddDays(25);
        await db.SaveChangesAsync();

        Assert.Equal(TripBookingOperationResult.Success, await repo.UnbookTripAsync(tripId, "guest-1"));
        Assert.False(await db.TripBookings.AnyAsync(x => x.UserId == "guest-1"));
    }
}
