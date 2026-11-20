using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(string id);
        Task<Booking> AddAsync(Booking booking);
        Task<Booking?> UpdateAsync(Booking booking);
        Task<bool> DeleteAsync(string id);
    }
}
