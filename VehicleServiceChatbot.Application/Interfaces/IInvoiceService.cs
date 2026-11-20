using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetInvoiceDataAsync(int currentUserId, bool isAdmin, int bookingId);
    Task<byte[]> GenerateInvoicePdfAsync(int currentUserId, bool isAdmin, int bookingId);
}
