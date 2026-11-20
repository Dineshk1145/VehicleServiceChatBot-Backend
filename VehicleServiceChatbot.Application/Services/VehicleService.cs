using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Common;
using VehicleServiceChatbot.Application.DTOs.Vehicle;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleResponseDto> AddVehicleAsync(VehicleRequestDto request, int currentUserId, bool isAdmin)
    {
        // Security: Customers can only register vehicles for themselves.
        // Prevents IDOR where an attacker submits another user's UserId.
        int targetUserId = isAdmin && request.UserId > 0 ? request.UserId : currentUserId;

        var vehicle = new Vehicle
        {
            UserId = targetUserId,
            VehicleType = request.VehicleType.Trim(),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            NumberPlate = request.NumberPlate.Trim().ToUpperInvariant()
        };

        await _vehicleRepository.AddVehicleAsync(vehicle);

        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            UserId = vehicle.UserId,
            VehicleType = vehicle.VehicleType,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            NumberPlate = vehicle.NumberPlate
        };
    }

    public async Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync(int currentUserId, bool isAdmin)
    {
        // Security: Non-admin users only see their own vehicles.
        var vehicles = isAdmin
            ? await _vehicleRepository.GetAllVehiclesAsync()
            : await _vehicleRepository.GetVehiclesByUserIdAsync(currentUserId);

        return vehicles.Select(v => new VehicleResponseDto
        {
            Id = v.Id,
            UserId = v.UserId,
            VehicleType = v.VehicleType,
            Brand = v.Brand,
            Model = v.Model,
            NumberPlate = v.NumberPlate
        });
    }

    public async Task<PagedResult<VehicleResponseDto>> GetVehiclesPagedAsync(int currentUserId, bool isAdmin, PaginationParams pagination)
    {
        int? filterUserId = isAdmin ? null : currentUserId;
        var (items, totalCount) = await _vehicleRepository.GetVehiclesPagedAsync(pagination.PageNumber, pagination.PageSize, filterUserId);

        var dtos = items.Select(v => new VehicleResponseDto
        {
            Id = v.Id,
            UserId = v.UserId,
            VehicleType = v.VehicleType,
            Brand = v.Brand,
            Model = v.Model,
            NumberPlate = v.NumberPlate
        });

        return new PagedResult<VehicleResponseDto>(dtos, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<VehicleResponseDto?> GetVehicleByIdAsync(int id, int currentUserId, bool isAdmin)
    {
        var v = await _vehicleRepository.GetVehicleByIdAsync(id);
        if (v == null) return null;

        // Security check: verify ownership
        if (!isAdmin && v.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this vehicle.");
        }

        return new VehicleResponseDto
        {
            Id = v.Id,
            UserId = v.UserId,
            VehicleType = v.VehicleType,
            Brand = v.Brand,
            Model = v.Model,
            NumberPlate = v.NumberPlate
        };
    }

    public async Task<VehicleResponseDto?> UpdateVehicleAsync(int id, VehicleRequestDto request, int currentUserId, bool isAdmin)
    {
        var vehicle = await _vehicleRepository.GetVehicleByIdAsync(id);
        if (vehicle == null) return null;

        // Security check: prevent modifying another user's vehicle
        if (!isAdmin && vehicle.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this vehicle.");
        }

        if (isAdmin && request.UserId > 0)
        {
            vehicle.UserId = request.UserId;
        }

        vehicle.VehicleType = request.VehicleType.Trim();
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.NumberPlate = request.NumberPlate.Trim().ToUpperInvariant();

        await _vehicleRepository.UpdateVehicleAsync(vehicle);

        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            UserId = vehicle.UserId,
            VehicleType = vehicle.VehicleType,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            NumberPlate = vehicle.NumberPlate
        };
    }

    public async Task<bool> DeleteVehicleAsync(int id, int currentUserId, bool isAdmin)
    {
        var vehicle = await _vehicleRepository.GetVehicleByIdAsync(id);
        if (vehicle == null) return false;

        // Security check: prevent deleting another user's vehicle
        if (!isAdmin && vehicle.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this vehicle.");
        }

        await _vehicleRepository.DeleteVehicleAsync(vehicle);
        return true;
    }
}
