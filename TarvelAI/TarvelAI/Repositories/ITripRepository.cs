using TarvelAI.DTOs.Trip;

namespace TarvelAI.Repositories;

public interface ITripRepository
{
    Task<IEnumerable<TripDto>> GetAllAsync();
    Task<TripDto?> GetByIdAsync(int id);

    /// <summary>First template package row per type (<see cref="Models.HotelBooking.TripBookingId"/> / Flight null), if any.</summary>
    Task<(int? HotelId, int? FlightId)> GetTemplateLinkIdsAsync(int tripId);

    Task<TripDto> CreateAsync(CreateTripDto dto);
    Task<TripDto?> UpdateAsync(int id, UpdateTripDto dto);
    Task<bool> DeleteAsync(int id);
    Task<TripBookingOperationResult> BookTripAsync(int tripId, string userId, TripBillingInput? billing = null);
    Task<TripBookingOperationResult> UnbookTripAsync(int tripId, string userId);
    Task<IEnumerable<MyTripDto>> GetMyBookedTripsAsync(string userId);

    Task<IReadOnlyList<PlanningTripAdminDto>> GetPlanningTripsForAdminAsync();
    Task<TripPlanningAdminResult> AdminConfirmPlanningHotelAsync(int tripId);
    Task<TripPlanningAdminResult> AdminConfirmPlanningAirlineAsync(int tripId);
    Task<TripPlanningAdminResult> AdminConfirmPlanningTripAsync(int tripId);
}

public enum TripBookingOperationResult
{
    Success,
    TripNotFound,
    TripNotAvailable,
    AlreadyBooked,
    IncompleteBilling,
    BookingNotFound,
    /// <summary>Confirmed trips cannot be cancelled on or after the itinerary start date (UTC).</summary>
    CancellationNotAllowedTripInProgress
}

public enum TripPlanningAdminResult
{
    Success,
    TripNotFound,
    NotInPlanningState,
    FinalRequiresBothConfirmations,
    AirlineRequiresHotelConfirmation
}
