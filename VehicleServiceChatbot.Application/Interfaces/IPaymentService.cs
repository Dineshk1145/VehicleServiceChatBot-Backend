using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(int currentUserId, ProcessPaymentDto request);
    Task<PaymentResponseDto?> GetPaymentByBookingIdAsync(int currentUserId, bool isAdmin, int bookingId);
}
