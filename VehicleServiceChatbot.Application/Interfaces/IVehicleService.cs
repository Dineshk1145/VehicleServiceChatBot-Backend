using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Common;
using VehicleServiceChatbot.Application.DTOs.Vehicle;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleResponseDto> AddVehicleAsync(VehicleRequestDto request, int currentUserId, bool isAdmin);
    Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync(int currentUserId, bool isAdmin);
    Task<PagedResult<VehicleResponseDto>> GetVehiclesPagedAsync(int currentUserId, bool isAdmin, PaginationParams pagination);
    Task<VehicleResponseDto?> GetVehicleByIdAsync(int id, int currentUserId, bool isAdmin);
    Task<VehicleResponseDto?> UpdateVehicleAsync(int id, VehicleRequestDto request, int currentUserId, bool isAdmin);
    Task<bool> DeleteVehicleAsync(int id, int currentUserId, bool isAdmin);
}
