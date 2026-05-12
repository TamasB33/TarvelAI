using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TarvelAI.Models;

namespace TarvelAI.Data;

public static class TripSeeder
{
    // ── Called on startup — only inserts what is missing ─────────────────────
    public static async Task SeedAsync(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        var (admin, alice, bob) = await EnsureUsersAsync(userManager);
        var hotels              = await EnsureHotelsAsync(db);
        var flights             = await EnsureFlightsAsync(db);
        await EnsureTripsAsync(db, admin, alice, bob, hotels, flights);
        await EnsureMyTripsDemoDataAsync(db, userManager);
    }

    // ── Wipes all travel data then re-seeds (useful during dev) ──────────────
    public static async Task ResetAndSeedAsync(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        db.TripBookings.RemoveRange(db.TripBookings);
        db.FlightBookings.RemoveRange(db.FlightBookings);
        db.HotelBookings.RemoveRange(db.HotelBookings);
        db.Trips.RemoveRange(db.Trips);
        db.Flights.RemoveRange(db.Flights);
        db.Hotels.RemoveRange(db.Hotels);
        await db.SaveChangesAsync();

        foreach (var email in new[] { "alice@travelai.com", "bob@travelai.com" })
        {
            var u = await userManager.FindByEmailAsync(email);
            if (u is not null) await userManager.DeleteAsync(u);
        }

        var (admin, alice, bob) = await EnsureUsersAsync(userManager);
        var hotels              = await EnsureHotelsAsync(db);
        var flights             = await EnsureFlightsAsync(db);
        await EnsureTripsAsync(db, admin, alice, bob, hotels, flights);
        await EnsureMyTripsDemoDataAsync(db, userManager);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task<(IdentityUser admin, IdentityUser alice, IdentityUser bob)>
        EnsureUsersAsync(UserManager<IdentityUser> userManager)
    {
        var admin = await EnsureUser(userManager, "admin@travelai.com", "Admin@123", "Admin");
        var alice = await EnsureUser(userManager, "alice@travelai.com", "User@123",  "User");
        var bob   = await EnsureUser(userManager, "bob@travelai.com",   "User@123",  "User");
        return (admin, alice, bob);
    }

    private static async Task<List<Hotel>> EnsureHotelsAsync(AppDbContext db)
    {
        if (await db.Hotels.AnyAsync())
            return await db.Hotels.OrderBy(h => h.Id).ToListAsync();

        var hotels = new List<Hotel>
        {
            new() { Name = "Hotel Le Meurice",         Address = "228 Rue de Rivoli",      City = "Paris",      Country = "France",         Rating = 4.9, ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800" },
            new() { Name = "Hotel Hassler Roma",       Address = "Trinita dei Monti 6",    City = "Rome",       Country = "Italy",          Rating = 4.8, ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800" },
            new() { Name = "The Grand Budapest",       Address = "Castle District",        City = "Budapest",   Country = "Hungary",        Rating = 4.7, ImageUrl = "https://images.unsplash.com/photo-1444201983204-c43cbd584d93?w=800" },
            new() { Name = "Hotel Adlon Berlin",       Address = "Unter den Linden 77",    City = "Berlin",     Country = "Germany",        Rating = 4.7, ImageUrl = "https://images.unsplash.com/photo-1519710164239-da123dc03ef4?w=800" },
            new() { Name = "The Westin Palace",        Address = "Plaza de las Cortes 7",  City = "Madrid",     Country = "Spain",          Rating = 4.6, ImageUrl = "https://images.unsplash.com/photo-1559599238-308793637427?w=800" },
            new() { Name = "Hotel Metropole",          Address = "Place de Brouckere 31",  City = "Brussels",   Country = "Belgium",        Rating = 4.5, ImageUrl = "https://images.unsplash.com/photo-1522798514-97ceb8c4f1c8?w=800" },
            new() { Name = "Porto Riverside Suites",   Address = "Ribeira 12",             City = "Porto",      Country = "Portugal",       Rating = 4.6, ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800" },
            new() { Name = "Nordic Harbor Hotel",      Address = "Skeppsbron 18",          City = "Stockholm",  Country = "Sweden",         Rating = 4.6, ImageUrl = "https://images.unsplash.com/photo-1501117716987-c8e1ecb210c2?w=800" },
            new() { Name = "Alpine Crown Zurich",      Address = "Bahnhofstrasse 40",      City = "Zurich",     Country = "Switzerland",    Rating = 4.8, ImageUrl = "https://images.unsplash.com/photo-1470246973918-29a93221c455?w=800" },
            new() { Name = "Canal View Amsterdam",     Address = "Prinsengracht 104",      City = "Amsterdam",  Country = "Netherlands",    Rating = 4.7, ImageUrl = "https://images.unsplash.com/photo-1512470876302-972faa2aa9a4?w=800" }
        };

        await db.Hotels.AddRangeAsync(hotels);
        await db.SaveChangesAsync();
        return hotels;
    }

    private static async Task<List<Flight>> EnsureFlightsAsync(AppDbContext db)
    {
        if (await db.Flights.AnyAsync())
            return await db.Flights.OrderBy(f => f.Id).ToListAsync();

        var flights = new List<Flight>
        {
            new() { FlightNumber = "BA304", Airline = "British Airways", OriginAirport = "LHR", DestinationAirport = "CDG", DepartureTime = DateTime.UtcNow.AddDays(7),  ArrivalTime = DateTime.UtcNow.AddDays(7).AddHours(2) },
            new() { FlightNumber = "AZ202", Airline = "ITA Airways",     OriginAirport = "LHR", DestinationAirport = "FCO", DepartureTime = DateTime.UtcNow.AddDays(9),  ArrivalTime = DateTime.UtcNow.AddDays(9).AddHours(2) },
            new() { FlightNumber = "LH811", Airline = "Lufthansa",       OriginAirport = "LHR", DestinationAirport = "BER", DepartureTime = DateTime.UtcNow.AddDays(11), ArrivalTime = DateTime.UtcNow.AddDays(11).AddHours(2) },
            new() { FlightNumber = "IB371", Airline = "Iberia",          OriginAirport = "LHR", DestinationAirport = "MAD", DepartureTime = DateTime.UtcNow.AddDays(13), ArrivalTime = DateTime.UtcNow.AddDays(13).AddHours(2) },
            new() { FlightNumber = "TP135", Airline = "TAP Air Portugal",OriginAirport = "LHR", DestinationAirport = "OPO", DepartureTime = DateTime.UtcNow.AddDays(15), ArrivalTime = DateTime.UtcNow.AddDays(15).AddHours(2) },
            new() { FlightNumber = "SK528", Airline = "SAS",             OriginAirport = "LHR", DestinationAirport = "ARN", DepartureTime = DateTime.UtcNow.AddDays(17), ArrivalTime = DateTime.UtcNow.AddDays(17).AddHours(2) },
            new() { FlightNumber = "KL100", Airline = "KLM",             OriginAirport = "LHR", DestinationAirport = "AMS", DepartureTime = DateTime.UtcNow.AddDays(19), ArrivalTime = DateTime.UtcNow.AddDays(19).AddHours(2) },
            new() { FlightNumber = "LX319", Airline = "Swiss",           OriginAirport = "LHR", DestinationAirport = "ZRH", DepartureTime = DateTime.UtcNow.AddDays(21), ArrivalTime = DateTime.UtcNow.AddDays(21).AddHours(2) },
            new() { FlightNumber = "LO286", Airline = "LOT",             OriginAirport = "LHR", DestinationAirport = "WAW", DepartureTime = DateTime.UtcNow.AddDays(23), ArrivalTime = DateTime.UtcNow.AddDays(23).AddHours(2) },
            new() { FlightNumber = "OS456", Airline = "Austrian",        OriginAirport = "LHR", DestinationAirport = "VIE", DepartureTime = DateTime.UtcNow.AddDays(25), ArrivalTime = DateTime.UtcNow.AddDays(25).AddHours(2) }
        };

        await db.Flights.AddRangeAsync(flights);
        await db.SaveChangesAsync();
        return flights;
    }

    private static async Task EnsureTripsAsync(
        AppDbContext db,
        IdentityUser admin, IdentityUser alice, IdentityUser bob,
        List<Hotel> hotels, List<Flight> flights)
    {
        if (await db.Trips.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var tripSeeds = new (string Name, string Destination, string Description, string ImageUrl, decimal Price, int DurationDays, TripStatus Status, string CreatedBy)[]
        {
            ("Paris Getaway", "Paris, France", "A romantic escape to the city of lights.", "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800", 1200m, 5, TripStatus.Available, alice.Id),
            ("Rome and Amalfi Coast", "Rome, Italy", "Historic city walks and coastal relaxation.", "https://images.unsplash.com/photo-1552832230-c0197dd311b5?w=800", 1500m, 9, TripStatus.Available, bob.Id),
            ("Budapest City Lights", "Budapest, Hungary", "Thermal baths, Danube views, and ruin bars.", "https://images.unsplash.com/photo-1565426873117-0de6d2f5c12e?w=800", 980m, 4, TripStatus.Available, alice.Id),
            ("Berlin Culture Week", "Berlin, Germany", "Museums, neighborhoods, and nightlife.", "https://images.unsplash.com/photo-1560969184-10fe8719e047?w=800", 1100m, 6, TripStatus.Available, admin.Id),
            ("Madrid Tapas Escape", "Madrid, Spain", "Food-focused city break with art museums.", "https://images.unsplash.com/photo-1539037116277-4db20889f2d4?w=800", 1050m, 5, TripStatus.Available, bob.Id),
            ("Porto Riverside", "Porto, Portugal", "Riverside strolls, wine cellars, and coastal day trips.", "https://images.unsplash.com/photo-1555881400-74d7acaacd8b?w=800", 990m, 5, TripStatus.Available, alice.Id),
            ("Stockholm Archipelago", "Stockholm, Sweden", "Nordic design and island hopping.", "https://images.unsplash.com/photo-1509356843151-3e7d96241e11?w=800", 1450m, 7, TripStatus.Available, bob.Id),
            ("Zurich Alpine Break", "Zurich, Switzerland", "City comfort with nearby alpine routes.", "https://images.unsplash.com/photo-1521292270410-a8c4d716d518?w=800", 1800m, 6, TripStatus.Available, alice.Id),
            ("Amsterdam Canals", "Amsterdam, Netherlands", "Canals, museums, and bike-friendly neighborhoods.", "https://images.unsplash.com/photo-1534351590666-13e3e96b5017?w=800", 1300m, 5, TripStatus.Available, bob.Id),
            ("Vienna Classical Tour", "Vienna, Austria", "Imperial architecture and classical music evenings.", "https://images.unsplash.com/photo-1516550893923-42d28e5677af?w=800", 1350m, 6, TripStatus.Available, admin.Id),
            ("Brussels Weekend", "Brussels, Belgium", "Historic squares, chocolate, and modern art.", "https://images.unsplash.com/photo-1559127452-5a0f4f4d2f6f?w=800", 970m, 4, TripStatus.Available, alice.Id),
            ("Prague Old Town", "Prague, Czech Republic", "Castle district and riverside evenings.", "https://images.unsplash.com/photo-1541849546-216549ae216d?w=800", 1020m, 5, TripStatus.Available, bob.Id),
            ("Athens Heritage", "Athens, Greece", "Acropolis, local cuisine, and seaside sunsets.", "https://images.unsplash.com/photo-1555993539-1732b0258235?w=800", 1180m, 6, TripStatus.Confirmed, alice.Id),
            ("Dublin Discovery", "Dublin, Ireland", "Historic pubs, museums, and nearby cliffs.", "https://images.unsplash.com/photo-1520637836862-4d197d17c57a?w=800", 1250m, 5, TripStatus.Confirmed, bob.Id),
            ("Krakow Heritage", "Krakow, Poland", "Old town charm and cultural landmarks.", "https://images.unsplash.com/photo-1580327344181-c1163234e5a0?w=800", 980m, 5, TripStatus.Confirmed, admin.Id),
            ("Copenhagen Design Trip", "Copenhagen, Denmark", "Scandinavian design and culinary scene.", "https://images.unsplash.com/photo-1513622470522-26c3c8a854bc?w=800", 1420m, 5, TripStatus.Completed, alice.Id),
            ("Helsinki Calm Retreat", "Helsinki, Finland", "Saunas, sea views, and modern Nordic architecture.", "https://images.unsplash.com/photo-1467269204594-9661b134dd2b?w=800", 1380m, 5, TripStatus.Completed, bob.Id),
            ("Dubrovnik Coastline", "Dubrovnik, Croatia", "Old town walls and Adriatic coastline vistas.", "https://images.unsplash.com/photo-1505765050516-f72dcac9c60e?w=800", 1600m, 6, TripStatus.Cancelled, admin.Id)
        };

        var trips = tripSeeds.Select(seed => new Trip
        {
            Name = seed.Name,
            Destination = seed.Destination,
            Description = seed.Description,
            ImageUrl = seed.ImageUrl,
            BasePrice = seed.Price,
            DurationDays = seed.DurationDays,
            Status = seed.Status,
            CreatedBy = seed.CreatedBy,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        await db.Trips.AddRangeAsync(trips);
        await db.SaveChangesAsync();

        var hotelBookings = new List<HotelBooking>();
        var flightBookings = new List<FlightBooking>();
        for (var i = 0; i < trips.Count; i++)
        {
            var trip = trips[i];
            var hotel = hotels[i % hotels.Count];
            var flight = flights[i % flights.Count];

            var checkIn = DateOnly.FromDateTime(now.Date.AddDays(14 + (i * 4)));
            var checkOut = checkIn.AddDays(trip.DurationDays);
            var nightly = Math.Round(Math.Max(120m, trip.BasePrice / Math.Max(1, trip.DurationDays) * 0.35m), 2);

            hotelBookings.Add(new HotelBooking
            {
                TripId = trip.Id,
                HotelId = hotel.Id,
                RoomType = "Standard Double",
                Guests = 2,
                NumberOfRooms = 1,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                PricePerNight = nightly,
                TotalPrice = nightly * trip.DurationDays,
                Status = BookingStatus.Planned,
                ConfirmationNumber = null
            });

            flightBookings.Add(new FlightBooking
            {
                TripId = trip.Id,
                FlightId = flight.Id,
                CabinClass = "Economy",
                Passengers = 2,
                Price = Math.Round(Math.Max(180m, trip.BasePrice * 0.45m), 2),
                Status = BookingStatus.Planned,
                ConfirmationNumber = null
            });
        }

        await db.HotelBookings.AddRangeAsync(hotelBookings);
        await db.FlightBookings.AddRangeAsync(flightBookings);

        if (!await db.TripBookings.AnyAsync())
        {
            var aliceTripIndexes = new[] { 0, 1, 2, 3, 4, 5 };
            var bobTripIndexes = new[] { 4, 5, 6, 7, 8, 9 };

            var tripBookings = new List<TripBooking>();
            for (var i = 0; i < aliceTripIndexes.Length; i++)
            {
                tripBookings.Add(new TripBooking
                {
                    TripId = trips[aliceTripIndexes[i]].Id,
                    UserId = alice.Id,
                    BookedAtUtc = now.AddDays(-(14 - (i * 2)))
                });
            }

            for (var i = 0; i < bobTripIndexes.Length; i++)
            {
                tripBookings.Add(new TripBooking
                {
                    TripId = trips[bobTripIndexes[i]].Id,
                    UserId = bob.Id,
                    BookedAtUtc = now.AddDays(-(13 - (i * 2)))
                });
            }

            await db.TripBookings.AddRangeAsync(tripBookings);
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureMyTripsDemoDataAsync(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        var users = await userManager.GetUsersInRoleAsync("User");
        if (users.Count == 0) return;

        var trips = await db.Trips
            .Where(t => t.Status == TripStatus.Available)
            .OrderBy(t => t.Id)
            .Take(12)
            .ToListAsync();

        if (trips.Count == 0) return;

        foreach (var user in users)
        {
            var bookingCount = await db.TripBookings.CountAsync(tb => tb.UserId == user.Id);
            if (bookingCount >= 6) continue;

            var existingTripIds = await db.TripBookings
                .Where(tb => tb.UserId == user.Id)
                .Select(tb => tb.TripId)
                .ToListAsync();

            var candidates = trips.Where(t => !existingTripIds.Contains(t.Id)).Take(6 - bookingCount).ToList();
            var toAdd = candidates.Select((trip, idx) => new TripBooking
            {
                TripId = trip.Id,
                UserId = user.Id,
                BookedAtUtc = DateTime.UtcNow.AddDays(-((idx + 1) * 2))
            });

            await db.TripBookings.AddRangeAsync(toAdd);
        }

        await db.SaveChangesAsync();
    }

    private static async Task<IdentityUser> EnsureUser(
        UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }
}
