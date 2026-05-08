using TarvelAI.DTOs.Trip;

namespace TarvelAI.Repositories;

public interface ITripRepository
{
    Task<IEnumerable<TripDto>> GetAllAsync();
    Task<TripDto?>             GetByIdAsync(int id);
    Task<TripDto>              CreateAsync(CreateTripDto dto);
    Task<TripDto?>             UpdateAsync(int id, UpdateTripDto dto);
    Task<bool>                 DeleteAsync(int id);
    Task<TripBookingOperationResult> BookTripAsync(int tripId, string userId);
    Task<TripBookingOperationResult> UnbookTripAsync(int tripId, string userId);
    Task<IEnumerable<MyTripDto>> GetMyBookedTripsAsync(string userId);
}

public enum TripBookingOperationResult
{
    Success,
    TripNotFound,
    AlreadyBooked,
    BookingNotFound
}
