using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;

namespace VehicleServiceChatbot.Persistence.Repositories;

public class ServiceBookingRepository : IServiceBookingRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceBookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddBookingAsync(ServiceBooking booking)
    {
        _context.ServiceBookings.Add(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ServiceBooking>> GetAllBookingsAsync()
    {
        // Parameterized queries via EF Core LINQ
        return await _context.ServiceBookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceBooking>> GetBookingsByUserIdAsync(int userId)
    {
        // Parameterized query: userId is passed as a SQL parameter
        return await _context.ServiceBookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ServiceBooking> Items, int TotalCount)> GetBookingsPagedAsync(int pageNumber, int pageSize, int? userId = null)
    {
        var query = _context.ServiceBookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(b => b.UserId == userId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.BookingDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ServiceBooking?> GetBookingByIdAsync(int id)
    {
        return await _context.ServiceBookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task UpdateBookingAsync(ServiceBooking booking)
    {
        _context.Entry(booking).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBookingAsync(ServiceBooking booking)
    {
        _context.ServiceBookings.Remove(booking);
        await _context.SaveChangesAsync();
    }
}
