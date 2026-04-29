using TarvelAI.DTOs.Flight;

namespace TarvelAI.Repositories;

public interface IFlightRepository
{
    Task<IEnumerable<FlightDto>> GetAllAsync();
    Task<FlightDto?> GetByIdAsync(int id);
    Task<FlightDto> CreateAsync(CreateFlightDto dto);
    Task<FlightDto?> UpdateAsync(int id, UpdateFlightDto dto);
    Task<bool> DeleteAsync(int id);
}
