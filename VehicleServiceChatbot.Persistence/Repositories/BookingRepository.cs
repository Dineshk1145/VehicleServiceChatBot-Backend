using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;

namespace VehicleServiceChatbot.Persistence.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly VehicleServiceDbContext _context;

        public BookingRepository(VehicleServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings.ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(string id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task<Booking> AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
