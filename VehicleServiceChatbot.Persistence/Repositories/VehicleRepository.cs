using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;

namespace VehicleServiceChatbot.Persistence.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddVehicleAsync(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
    {
        // Parameterized query via EF Core LINQ
        return await _context.Vehicles.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetVehiclesByUserIdAsync(int userId)
    {
        // Parameterized query: userId is passed as a SQL parameter (@__userId_0)
        return await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Vehicle> Items, int TotalCount)> GetVehiclesPagedAsync(int pageNumber, int pageSize, int? userId = null)
    {
        // Parameterized query with pagination
        var query = _context.Vehicles.AsNoTracking().AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(v => v.UserId == userId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        return await _context.Vehicles.FindAsync(id);
    }

    public async Task UpdateVehicleAsync(Vehicle vehicle)
    {
        _context.Entry(vehicle).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteVehicleAsync(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
    }
}
