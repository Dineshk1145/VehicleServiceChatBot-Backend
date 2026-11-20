using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IServiceBookingRepository
{
    Task AddBookingAsync(ServiceBooking booking);
    Task<IEnumerable<ServiceBooking>> GetAllBookingsAsync();
    Task<IEnumerable<ServiceBooking>> GetBookingsByUserIdAsync(int userId);
    Task<(IEnumerable<ServiceBooking> Items, int TotalCount)> GetBookingsPagedAsync(int pageNumber, int pageSize, int? userId = null);
    Task<ServiceBooking?> GetBookingByIdAsync(int id);
    Task UpdateBookingAsync(ServiceBooking booking);
    Task DeleteBookingAsync(ServiceBooking booking);
}
