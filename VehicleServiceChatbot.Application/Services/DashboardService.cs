using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Enums;

namespace VehicleServiceChatbot.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IServiceBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DashboardService(
        IServiceBookingRepository bookingRepository,
        IPaymentRepository paymentRepository)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var bookings = (await _bookingRepository.GetAllBookingsAsync()).ToList();
        var payments = (await _paymentRepository.GetAllPaymentsAsync()).ToList();

        int totalBookings = bookings.Count;
        int pending = bookings.Count(b => b.Status == ServiceBookingStatus.Pending);
        int confirmed = bookings.Count(b => b.Status == ServiceBookingStatus.Confirmed);
        int vehicleReceived = bookings.Count(b => b.Status == ServiceBookingStatus.VehicleReceived);
        int inProgress = bookings.Count(b => b.Status == ServiceBookingStatus.ServiceInProgress);
        int completed = bookings.Count(b => b.Status == ServiceBookingStatus.ServiceCompleted);
        int readyForDelivery = bookings.Count(b => b.Status == ServiceBookingStatus.ReadyForDelivery);
        int delivered = bookings.Count(b => b.Status == ServiceBookingStatus.Delivered);
        int cancelled = bookings.Count(b => b.Status == ServiceBookingStatus.Cancelled);

        // Revenue: Sum of TotalAmount of bookings that are completed or delivered or paid
        decimal totalRevenue = bookings
            .Where(b => b.PaymentStatus == "Completed" || b.Status == ServiceBookingStatus.ServiceCompleted || b.Status == ServiceBookingStatus.Delivered)
            .Sum(b => b.TotalAmount > 0 ? b.TotalAmount : b.Price);

        int completedPaymentsCount = payments.Count(p => p.PaymentStatus == "Completed");
        int pendingPaymentsCount = bookings.Count(b => b.PaymentStatus != "Completed" && b.Status != ServiceBookingStatus.Cancelled);

        return new DashboardStatsDto
        {
            TotalBookings = totalBookings,
            PendingBookings = pending,
            ConfirmedBookings = confirmed,
            VehicleReceivedBookings = vehicleReceived,
            InProgressServices = inProgress,
            CompletedServices = completed,
            ReadyForDeliveryBookings = readyForDelivery,
            DeliveredServices = delivered,
            CancelledBookings = cancelled,
            TotalRevenue = Math.Round(totalRevenue, 2),
            PendingPayments = pendingPaymentsCount,
            CompletedPayments = completedPaymentsCount
        };
    }
}
