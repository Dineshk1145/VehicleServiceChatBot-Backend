using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IVehicleRepository
{
    Task AddVehicleAsync(Vehicle vehicle);
    Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
    Task<IEnumerable<Vehicle>> GetVehiclesByUserIdAsync(int userId);
    Task<(IEnumerable<Vehicle> Items, int TotalCount)> GetVehiclesPagedAsync(int pageNumber, int pageSize, int? userId = null);
    Task<Vehicle?> GetVehicleByIdAsync(int id);
    Task UpdateVehicleAsync(Vehicle vehicle);
    Task DeleteVehicleAsync(Vehicle vehicle);
}
