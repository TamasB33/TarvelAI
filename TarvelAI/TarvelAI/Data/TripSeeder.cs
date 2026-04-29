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
    }

    // ── Wipes all travel data then re-seeds (useful during dev) ──────────────
    public static async Task ResetAndSeedAsync(AppDbContext db, UserManager<IdentityUser> userManager)
    {
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
            new() { Name = "Hotel Le Meurice",            Address = "228 Rue de Rivoli",      City = "Paris",    Country = "France",    Rating = 4.9, ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800" },
            new() { Name = "Park Hyatt Tokyo",             Address = "3-7-1-2 Nishi Shinjuku", City = "Tokyo",    Country = "Japan",     Rating = 4.8, ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800" },
            new() { Name = "The Plaza Hotel",              Address = "768 5th Ave",             City = "New York", Country = "USA",       Rating = 4.7, ImageUrl = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800" },
            new() { Name = "Four Seasons Bali at Sayan",   Address = "Sayan, Ubud",             City = "Bali",     Country = "Indonesia", Rating = 4.9, ImageUrl = "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800" },
            new() { Name = "Hotel Hassler Roma",           Address = "Trinita dei Monti 6",     City = "Rome",     Country = "Italy",     Rating = 4.8, ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800" },
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
            new() { FlightNumber = "BA304", Airline = "British Airways",   OriginAirport = "LHR", DestinationAirport = "CDG", DepartureTime = new DateTime(2025,  6,  1,  8,  0, 0, DateTimeKind.Utc), ArrivalTime = new DateTime(2025,  6,  1, 10, 30, 0, DateTimeKind.Utc) },
            new() { FlightNumber = "JL044", Airline = "Japan Airlines",    OriginAirport = "LHR", DestinationAirport = "NRT", DepartureTime = new DateTime(2025,  7, 10, 11,  0, 0, DateTimeKind.Utc), ArrivalTime = new DateTime(2025,  7, 11,  8,  0, 0, DateTimeKind.Utc) },
            new() { FlightNumber = "AA100", Airline = "American Airlines", OriginAirport = "LHR", DestinationAirport = "JFK", DepartureTime = new DateTime(2025,  8,  5,  9,  0, 0, DateTimeKind.Utc), ArrivalTime = new DateTime(2025,  8,  5, 12,  0, 0, DateTimeKind.Utc) },
            new() { FlightNumber = "GA880", Airline = "Garuda Indonesia",  OriginAirport = "LHR", DestinationAirport = "DPS", DepartureTime = new DateTime(2025,  9,  1, 14,  0, 0, DateTimeKind.Utc), ArrivalTime = new DateTime(2025,  9,  2, 10,  0, 0, DateTimeKind.Utc) },
            new() { FlightNumber = "AZ202", Airline = "ITA Airways",       OriginAirport = "LHR", DestinationAirport = "FCO", DepartureTime = new DateTime(2025, 10,  3,  7,  0, 0, DateTimeKind.Utc), ArrivalTime = new DateTime(2025, 10,  3, 10, 30, 0, DateTimeKind.Utc) },
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

        var trips = new List<Trip>
        {
            new()
            {
                Name         = "Paris Getaway",
                Destination  = "Paris, France",
                Description  = "A romantic escape to the city of lights. Explore the Eiffel Tower, Louvre and charming Montmartre.",
                ImageUrl     = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800",
                BasePrice    = 1200m, DurationDays = 5, Status = TripStatus.Confirmed,
                CreatedBy = alice.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name         = "Tokyo Explorer",
                Destination  = "Tokyo, Japan",
                Description  = "Discover the perfect blend of ancient temples and futuristic technology in Japan's vibrant capital.",
                ImageUrl     = "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=800",
                BasePrice    = 2400m, DurationDays = 10, Status = TripStatus.Planning,
                CreatedBy = bob.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name         = "New York City Break",
                Destination  = "New York, USA",
                Description  = "Experience the energy of Manhattan - Times Square, Central Park, and world-class dining.",
                ImageUrl     = "https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?w=800",
                BasePrice    = 1800m, DurationDays = 7, Status = TripStatus.Planning,
                CreatedBy = alice.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name         = "Bali Retreat",
                Destination  = "Bali, Indonesia",
                Description  = "Relax on stunning beaches, visit sacred temples and enjoy the lush rice terraces of Ubud.",
                ImageUrl     = "https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800",
                BasePrice    = 950m, DurationDays = 8, Status = TripStatus.Confirmed,
                CreatedBy = admin.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name         = "Rome and Amalfi Coast",
                Destination  = "Rome, Italy",
                Description  = "Walk through ancient history in Rome then unwind along the breathtaking Amalfi coastline.",
                ImageUrl     = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?w=800",
                BasePrice    = 1500m, DurationDays = 9, Status = TripStatus.Planning,
                CreatedBy = bob.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
        };

        await db.Trips.AddRangeAsync(trips);
        await db.SaveChangesAsync();

        // Hotel bookings - each trip has a matching hotel at the destination
        var hotelBookings = new List<HotelBooking>
        {
            new() { TripId = trips[0].Id, HotelId = hotels[0].Id, RoomType = "Deluxe Double",    Guests = 2, NumberOfRooms = 1, CheckInDate = new DateOnly(2025,  6,  1), CheckOutDate = new DateOnly(2025,  6,  6), PricePerNight = 450m, TotalPrice = 2250m, Status = BookingStatus.Booked,  ConfirmationNumber = "HTL-1001" },
            new() { TripId = trips[1].Id, HotelId = hotels[1].Id, RoomType = "Park Suite",        Guests = 1, NumberOfRooms = 1, CheckInDate = new DateOnly(2025,  7, 11), CheckOutDate = new DateOnly(2025,  7, 21), PricePerNight = 620m, TotalPrice = 6200m, Status = BookingStatus.Planned, ConfirmationNumber = null },
            new() { TripId = trips[2].Id, HotelId = hotels[2].Id, RoomType = "Central Park View", Guests = 2, NumberOfRooms = 1, CheckInDate = new DateOnly(2025,  8,  5), CheckOutDate = new DateOnly(2025,  8, 12), PricePerNight = 800m, TotalPrice = 5600m, Status = BookingStatus.Planned, ConfirmationNumber = null },
            new() { TripId = trips[3].Id, HotelId = hotels[3].Id, RoomType = "River Suite",       Guests = 2, NumberOfRooms = 1, CheckInDate = new DateOnly(2025,  9,  2), CheckOutDate = new DateOnly(2025,  9, 10), PricePerNight = 550m, TotalPrice = 4400m, Status = BookingStatus.Booked,  ConfirmationNumber = "HTL-4002" },
            new() { TripId = trips[4].Id, HotelId = hotels[4].Id, RoomType = "Classic Room",      Guests = 2, NumberOfRooms = 1, CheckInDate = new DateOnly(2025, 10,  3), CheckOutDate = new DateOnly(2025, 10, 12), PricePerNight = 380m, TotalPrice = 3420m, Status = BookingStatus.Planned, ConfirmationNumber = null },
        };

        await db.HotelBookings.AddRangeAsync(hotelBookings);

        // Flight bookings - each trip has a matching outbound flight
        var flightBookings = new List<FlightBooking>
        {
            new() { TripId = trips[0].Id, FlightId = flights[0].Id, CabinClass = "Business", Passengers = 2, Price =  960m, ConfirmationNumber = "FCF-3001", Status = BookingStatus.Booked  },
            new() { TripId = trips[1].Id, FlightId = flights[1].Id, CabinClass = "Economy",  Passengers = 1, Price =  580m, ConfirmationNumber = null,       Status = BookingStatus.Planned },
            new() { TripId = trips[2].Id, FlightId = flights[2].Id, CabinClass = "Economy",  Passengers = 2, Price =  740m, ConfirmationNumber = null,       Status = BookingStatus.Planned },
            new() { TripId = trips[3].Id, FlightId = flights[3].Id, CabinClass = "Business", Passengers = 2, Price = 2400m, ConfirmationNumber = "FCF-4002", Status = BookingStatus.Booked  },
            new() { TripId = trips[4].Id, FlightId = flights[4].Id, CabinClass = "Economy",  Passengers = 2, Price =  420m, ConfirmationNumber = null,       Status = BookingStatus.Planned },
        };

        await db.FlightBookings.AddRangeAsync(flightBookings);
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
