using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Models;

public class HotelBooking
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string RoomType { get; set; }

    [Range(1, 20)]
    public int Guests { get; set; } = 1;

    [Range(1, 100)]
    public int NumberOfRooms { get; set; } = 1;

    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PricePerNight { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; } = 0;


    public BookingStatus Status { get; set; } = BookingStatus.Planned;

    // FK to Trip
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    // FK to Hotel
    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;
}
