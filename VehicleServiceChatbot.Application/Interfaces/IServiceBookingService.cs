using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Common;
using VehicleServiceChatbot.Application.DTOs.ServiceBooking;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IServiceBookingService
{
    Task<ServiceBookingResponseDto> AddBookingAsync(ServiceBookingRequestDto request, int currentUserId, bool isAdmin);
    Task<IEnumerable<ServiceBookingResponseDto>> GetAllBookingsAsync(int currentUserId, bool isAdmin);
    Task<PagedResult<ServiceBookingResponseDto>> GetBookingsPagedAsync(int currentUserId, bool isAdmin, PaginationParams pagination);
    Task<ServiceBookingResponseDto?> GetBookingByIdAsync(int id, int currentUserId, bool isAdmin);
    Task<ServiceBookingResponseDto?> UpdateBookingAsync(int id, ServiceBookingRequestDto request, int currentUserId, bool isAdmin);
    Task<ServiceBookingResponseDto?> UpdateBookingStatusAsync(int id, string newStatus);
    Task<bool> DeleteBookingAsync(int id, int currentUserId, bool isAdmin);
}
