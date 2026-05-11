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
    public DbSet<TripBooking> TripBookings => Set<TripBooking>();
    public DbSet<TripBookingInvoice> TripBookingInvoices => Set<TripBookingInvoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Trip>().HasOne(t => t.User).WithMany().HasForeignKey(t => t.CreatedBy);

        builder.Entity<TripBooking>()
            .HasOne(tb => tb.Trip)
            .WithMany(t => t.TripBookings)
            .HasForeignKey(tb => tb.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TripBooking>()
            .HasOne(tb => tb.User)
            .WithMany()
            .HasForeignKey(tb => tb.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TripBooking>()
            .HasIndex(tb => new { tb.TripId, tb.UserId })
            .IsUnique();

        builder.Entity<TripBooking>()
            .HasOne(tb => tb.Invoice)
            .WithOne(i => i.TripBooking)
            .HasForeignKey<TripBookingInvoice>(i => i.TripBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<HotelBooking>()
            .HasOne(h => h.TripBooking)
            .WithMany(tb => tb.UserHotelBookings)
            .HasForeignKey(h => h.TripBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FlightBooking>()
            .HasOne(f => f.TripBooking)
            .WithMany(tb => tb.UserFlightBookings)
            .HasForeignKey(f => f.TripBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
