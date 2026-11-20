using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;

namespace VehicleServiceChatbot.Persistence.Repositories;

public class ServicePackageRepository : IServicePackageRepository
{
    private readonly ApplicationDbContext _context;

    public ServicePackageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServicePackage>> GetAllAsync()
    {
        return await _context.ServicePackages.ToListAsync();
    }

    public async Task<ServicePackage?> GetByIdAsync(string id)
    {
        return await _context.ServicePackages.FirstOrDefaultAsync(s => s.Id == id || s.Name == id);
    }
}
