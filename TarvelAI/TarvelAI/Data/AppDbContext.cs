using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TarvelAI.Models;

namespace TarvelAI.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<HotelBooking> HotelBookings => Set<HotelBooking>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightBooking> FlightBookings => Set<FlightBooking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Trip>().HasOne(t => t.User).WithMany().HasForeignKey(t => t.CreatedBy);
    }
}
