using TarvelAI.DTOs.Trip;

namespace TarvelAI.Repositories;

public interface ITripRepository
{
    Task<IEnumerable<TripDto>> GetAllAsync();
    Task<TripDto?>             GetByIdAsync(int id);
    Task<TripDto>              CreateAsync(CreateTripDto dto);
    Task<TripDto?>             UpdateAsync(int id, UpdateTripDto dto);
    Task<bool>                 DeleteAsync(int id);
}
