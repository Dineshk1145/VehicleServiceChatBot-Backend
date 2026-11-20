using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
        Task<BookingDto?> GetBookingByIdAsync(string id);
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
        Task<BookingDto?> UpdateBookingStatusAsync(string id, string status);
        Task<bool> DeleteBookingAsync(string id);
        Task<IEnumerable<ServicePackage>> GetServicesAsync();
    }
}
