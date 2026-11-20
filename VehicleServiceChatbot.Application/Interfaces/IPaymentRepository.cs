using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IPaymentRepository
{
    Task AddPaymentAsync(Payment payment);
    Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
    Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
}
