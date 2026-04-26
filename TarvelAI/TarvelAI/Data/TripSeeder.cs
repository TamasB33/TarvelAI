using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TarvelAI.Models;

namespace TarvelAI.Data;

public static class TripSeeder
{
    public static async Task SeedAsync(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        if (await db.Trips.AnyAsync())
            return;

        // ── Seed users ────────────────────────────────────────────────────────
        var adminUser = await EnsureUser(userManager, "admin@travelai.com", "Admin@123", "Admin");
        var user1     = await EnsureUser(userManager, "alice@travelai.com",  "User@123",  "User");
        var user2     = await EnsureUser(userManager, "bob@travelai.com",    "User@123",  "User");

        // ── Seed hotels ───────────────────────────────────────────────────────
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Name    = "Hôtel Le Meurice",
                Address = "228 Rue de Rivoli",
                City    = "Paris",
                Country = "France",
                Rating  = 4.9,
                ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800"
            },
            new Hotel
            {
                Name    = "Park Hyatt Tokyo",
                Address = "3-7-1-2 Nishi Shinjuku",
                City    = "Tokyo",
                Country = "Japan",
                Rating  = 4.8,
                ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800"
            },
            new Hotel
            {
                Name    = "The Plaza Hotel",
                Address = "768 5th Ave",
                City    = "New York",
                Country = "USA",
                Rating  = 4.7,
                ImageUrl = "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800"
            },
            new Hotel
            {
                Name    = "Four Seasons Bali at Sayan",
                Address = "Sayan, Ubud",
                City    = "Bali",
                Country = "Indonesia",
                Rating  = 4.9,
                ImageUrl = "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800"
            },
            new Hotel
            {
                Name    = "Hotel Hassler Roma",
                Address = "Trinità dei Monti 6",
                City    = "Rome",
                Country = "Italy",
                Rating  = 4.8,
                ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800"
            }
        };

        await db.Hotels.AddRangeAsync(hotels);
        await db.SaveChangesAsync();

        // ── Seed flights ──────────────────────────────────────────────────────
        var flights = new List<Flight>
        {
            new Flight
            {
                FlightNumber        = "BA304",
                Airline             = "British Airways",
                OriginAirport       = "LHR",
                DestinationAirport  = "CDG",
                DepartureTime       = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc),
                ArrivalTime         = new DateTime(2025, 6, 1, 10, 30, 0, DateTimeKind.Utc)
            },
            new Flight
            {
                FlightNumber        = "JL044",
                Airline             = "Japan Airlines",
                OriginAirport       = "LHR",
                DestinationAirport  = "NRT",
                DepartureTime       = new DateTime(2025, 7, 10, 11, 0, 0, DateTimeKind.Utc),
                ArrivalTime         = new DateTime(2025, 7, 11, 8, 0, 0, DateTimeKind.Utc)
            },
            new Flight
            {
                FlightNumber        = "AA100",
                Airline             = "American Airlines",
                OriginAirport       = "LHR",
                DestinationAirport  = "JFK",
                DepartureTime       = new DateTime(2025, 8, 5, 9, 0, 0, DateTimeKind.Utc),
                ArrivalTime         = new DateTime(2025, 8, 5, 12, 0, 0, DateTimeKind.Utc)
            },
            new Flight
            {
                FlightNumber        = "GA880",
                Airline             = "Garuda Indonesia",
                OriginAirport       = "LHR",
                DestinationAirport  = "DPS",
                DepartureTime       = new DateTime(2025, 9, 1, 14, 0, 0, DateTimeKind.Utc),
                ArrivalTime         = new DateTime(2025, 9, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new Flight
            {
                FlightNumber        = "AZ202",
                Airline             = "Alitalia",
                OriginAirport       = "LHR",
                DestinationAirport  = "FCO",
                DepartureTime       = new DateTime(2025, 10, 3, 7, 0, 0, DateTimeKind.Utc),
                ArrivalTime         = new DateTime(2025, 10, 3, 10, 30, 0, DateTimeKind.Utc)
            }
        };

        await db.Flights.AddRangeAsync(flights);
        await db.SaveChangesAsync();

        // ── Seed trips ────────────────────────────────────────────────────────
        var trips = new List<Trip>
        {
            new Trip
            {
                Name         = "Paris Getaway",
                Destination  = "Paris, France",
                Description  = "A romantic escape to the city of lights. Explore the Eiffel Tower, Louvre and charming Montmartre.",
                ImageUrl     = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=800",
                BasePrice    = 1200m,
                DurationDays = 5,
                Status       = TripStatus.Confirmed,
                CreatedBy    = user1.Id,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            },
            new Trip
            {
                Name         = "Tokyo Explorer",
                Destination  = "Tokyo, Japan",
                Description  = "Discover the perfect blend of ancient temples and futuristic technology in Japan's vibrant capital.",
                ImageUrl     = "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=800",
                BasePrice    = 2400m,
                DurationDays = 10,
                Status       = TripStatus.Planning,
                CreatedBy    = user2.Id,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            },
            new Trip
            {
                Name         = "New York City Break",
                Destination  = "New York, USA",
                Description  = "Experience the energy of Manhattan — Times Square, Central Park, and world-class dining.",
                ImageUrl     = "https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?w=800",
                BasePrice    = 1800m,
                DurationDays = 7,
                Status       = TripStatus.Planning,
                CreatedBy    = user1.Id,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            },
            new Trip
            {
                Name         = "Bali Retreat",
                Destination  = "Bali, Indonesia",
                Description  = "Relax on stunning beaches, visit sacred temples and enjoy the lush rice terraces of Ubud.",
                ImageUrl     = "https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800",
                BasePrice    = 950m,
                DurationDays = 8,
                Status       = TripStatus.Confirmed,
                CreatedBy    = adminUser.Id,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            },
            new Trip
            {
                Name         = "Rome & Amalfi Coast",
                Destination  = "Rome, Italy",
                Description  = "Walk through ancient history in Rome then unwind along the breathtaking Amalfi coastline.",
                ImageUrl     = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?w=800",
                BasePrice    = 1500m,
                DurationDays = 9,
                Status       = TripStatus.Planning,
                CreatedBy    = user2.Id,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            }
        };

        await db.Trips.AddRangeAsync(trips);
        await db.SaveChangesAsync();

        // ── Seed hotel bookings ───────────────────────────────────────────────
        var hotelBookings = new List<HotelBooking>
        {
            new HotelBooking
            {
                TripId           = trips[0].Id,
                HotelId          = hotels[0].Id,
                RoomType         = "Deluxe Double",
                Guests           = 2,
                NumberOfRooms    = 1,
                CheckInDate      = new DateOnly(2025, 6, 1),
                CheckOutDate     = new DateOnly(2025, 6, 6),
                PricePerNight    = 450m,
                TotalPrice       = 2250m,
                Status           = BookingStatus.Booked
            },
            new HotelBooking
            {
                TripId           = trips[1].Id,
                HotelId          = hotels[1].Id,
                RoomType         = "Park Suite",
                Guests           = 1,
                NumberOfRooms    = 1,
                CheckInDate      = new DateOnly(2025, 7, 11),
                CheckOutDate     = new DateOnly(2025, 7, 21),
                PricePerNight    = 620m,
                TotalPrice       = 6200m,
                Status           = BookingStatus.Planned
            },
            new HotelBooking
            {
                TripId           = trips[2].Id,
                HotelId          = hotels[2].Id,
                RoomType         = "Central Park View",
                Guests           = 2,
                NumberOfRooms    = 1,
                CheckInDate      = new DateOnly(2025, 8, 5),
                CheckOutDate     = new DateOnly(2025, 8, 12),
                PricePerNight    = 800m,
                TotalPrice       = 5600m,
                Status           = BookingStatus.Planned
            },
            new HotelBooking
            {
                TripId           = trips[3].Id,
                HotelId          = hotels[3].Id,
                RoomType         = "River Suite",
                Guests           = 2,
                NumberOfRooms    = 1,
                CheckInDate      = new DateOnly(2025, 9, 2),
                CheckOutDate     = new DateOnly(2025, 9, 10),
                PricePerNight    = 550m,
                TotalPrice       = 4400m,
                Status           = BookingStatus.Booked
            },
            new HotelBooking
            {
                TripId           = trips[4].Id,
                HotelId          = hotels[4].Id,
                RoomType         = "Classic Room",
                Guests           = 2,
                NumberOfRooms    = 1,
                CheckInDate      = new DateOnly(2025, 10, 3),
                CheckOutDate     = new DateOnly(2025, 10, 12),
                PricePerNight    = 380m,
                TotalPrice       = 3420m,
                Status           = BookingStatus.Planned
            }
        };

        await db.HotelBookings.AddRangeAsync(hotelBookings);

        // ── Seed flight bookings ──────────────────────────────────────────────
        var flightBookings = new List<FlightBooking>
        {
            new FlightBooking
            {
                TripId             = trips[0].Id,
                FlightId           = flights[0].Id,
                CabinClass         = "Business",
                Passengers         = 2,
                Price              = 960m,
                ConfirmationNumber = "FCF-3001",
                Status             = BookingStatus.Booked
            },
            new FlightBooking
            {
                TripId             = trips[1].Id,
                FlightId           = flights[1].Id,
                CabinClass         = "Economy",
                Passengers         = 1,
                Price              = 580m,
                ConfirmationNumber = null,
                Status             = BookingStatus.Planned
            },
            new FlightBooking
            {
                TripId             = trips[2].Id,
                FlightId           = flights[2].Id,
                CabinClass         = "Economy",
                Passengers         = 2,
                Price              = 740m,
                ConfirmationNumber = null,
                Status             = BookingStatus.Planned
            },
            new FlightBooking
            {
                TripId             = trips[3].Id,
                FlightId           = flights[3].Id,
                CabinClass         = "Business",
                Passengers         = 2,
                Price              = 2400m,
                ConfirmationNumber = "FCF-4002",
                Status             = BookingStatus.Booked
            },
            new FlightBooking
            {
                TripId             = trips[4].Id,
                FlightId           = flights[4].Id,
                CabinClass         = "Economy",
                Passengers         = 2,
                Price              = 420m,
                ConfirmationNumber = null,
                Status             = BookingStatus.Planned
            }
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
