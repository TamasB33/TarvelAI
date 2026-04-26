using TarvelAI.DTOs.Hotel;

namespace TarvelAI.Repositories;

public interface IHotelRepository
{
    Task<IEnumerable<HotelDto>> GetAllAsync();
    Task<HotelDto?> GetByIdAsync(int id);
    Task<HotelDto> CreateAsync(CreateHotelDto dto);
    Task<HotelDto?> UpdateAsync(int id, UpdateHotelDto dto);
    Task<bool> DeleteAsync(int id);
}
