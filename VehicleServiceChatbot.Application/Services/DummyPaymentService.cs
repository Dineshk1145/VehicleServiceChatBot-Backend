using System;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Services;

public class DummyPaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IServiceBookingRepository _bookingRepository;
    private readonly INotificationService _notificationService;

    public DummyPaymentService(
        IPaymentRepository paymentRepository,
        IServiceBookingRepository bookingRepository,
        INotificationService notificationService)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _notificationService = notificationService;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(int currentUserId, ProcessPaymentDto request)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(request.BookingId);
        if (booking == null)
        {
            throw new Exception($"Booking with ID {request.BookingId} was not found.");
        }

        // Customer can only pay for their own booking
        if (booking.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to pay for this booking.");
        }

        // Prevent duplicate payment
        if (booking.PaymentStatus == "Completed")
        {
            var existingPayment = await _paymentRepository.GetPaymentByBookingIdAsync(booking.Id);
            if (existingPayment != null)
            {
                return new PaymentResponseDto
                {
                    Id = existingPayment.Id,
                    PaymentId = existingPayment.PaymentId,
                    BookingId = existingPayment.BookingId,
                    UserId = existingPayment.UserId,
                    Amount = existingPayment.Amount,
                    PaymentStatus = existingPayment.PaymentStatus,
                    TransactionId = existingPayment.TransactionId,
                    PaymentDate = existingPayment.PaymentDate,
                    PaymentMethod = existingPayment.PaymentMethod,
                    Message = "Payment has already been completed for this booking."
                };
            }
        }

        bool isSuccess = !request.SimulateFailure;
        string status = isSuccess ? "Completed" : "Failed";
        string paymentId = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
        string transactionId = $"TXN-{Random.Shared.Next(1000000, 9999999)}";

        var payment = new Payment
        {
            PaymentId = paymentId,
            BookingId = booking.Id,
            UserId = currentUserId,
            Amount = booking.TotalAmount > 0 ? booking.TotalAmount : booking.Price,
            PaymentStatus = status,
            TransactionId = transactionId,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Credit/Debit Card" : request.PaymentMethod
        };

        await _paymentRepository.AddPaymentAsync(payment);

        if (isSuccess)
        {
            booking.PaymentStatus = "Completed";
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateBookingAsync(booking);

            // Send notification
            await _notificationService.CreateNotificationAsync(
                currentUserId,
                booking.Id,
                $"Payment of ₹{payment.Amount:F2} for booking #{booking.Id} was successful (Txn: {transactionId})."
            );
        }
        else
        {
            booking.PaymentStatus = "Failed";
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateBookingAsync(booking);

            // Send notification
            await _notificationService.CreateNotificationAsync(
                currentUserId,
                booking.Id,
                $"Payment for booking #{booking.Id} failed. Please try again."
            );
        }

        return new PaymentResponseDto
        {
            Id = payment.Id,
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            PaymentStatus = payment.PaymentStatus,
            TransactionId = payment.TransactionId,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            Message = isSuccess ? "Payment processed successfully!" : "Payment transaction failed."
        };
    }

    public async Task<PaymentResponseDto?> GetPaymentByBookingIdAsync(int currentUserId, bool isAdmin, int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null) return null;

        if (!isAdmin && booking.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this payment.");
        }

        var payment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);
        if (payment == null) return null;

        return new PaymentResponseDto
        {
            Id = payment.Id,
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            PaymentStatus = payment.PaymentStatus,
            TransactionId = payment.TransactionId,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            Message = "Payment details retrieved successfully."
        };
    }
}
