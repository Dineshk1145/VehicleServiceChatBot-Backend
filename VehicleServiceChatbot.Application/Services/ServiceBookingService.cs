using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Common;
using VehicleServiceChatbot.Application.DTOs.ServiceBooking;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Domain.Enums;

namespace VehicleServiceChatbot.Application.Services;

public class ServiceBookingService : IServiceBookingService
{
    private readonly IServiceBookingRepository _bookingRepository;
    private readonly IServicePackageRepository _packageRepository;
    private readonly INotificationService _notificationService;

    public ServiceBookingService(
        IServiceBookingRepository bookingRepository,
        IServicePackageRepository packageRepository,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _packageRepository = packageRepository;
        _notificationService = notificationService;
    }

    public async Task<ServiceBookingResponseDto> AddBookingAsync(ServiceBookingRequestDto request, int currentUserId, bool isAdmin)
    {
        // Security: Non-admin users can ONLY create bookings for themselves.
        // Prevents IDOR where an attacker specifies another user's UserId.
        int targetUserId = isAdmin && request.UserId > 0 ? request.UserId : currentUserId;

        // Calculate pricing
        decimal price = request.CustomPrice ?? 99.99m;

        // Try to match service package price if custom price wasn't passed
        if (!request.CustomPrice.HasValue)
        {
            var packages = await _packageRepository.GetAllAsync();
            var matched = packages.FirstOrDefault(p =>
                p.Name.Equals(request.ServiceType, StringComparison.OrdinalIgnoreCase) ||
                p.Id.Equals(request.ServiceType, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                price = matched.Price;
            }
            else if (request.ServiceType.Contains("Oil", StringComparison.OrdinalIgnoreCase)) price = 49.99m;
            else if (request.ServiceType.Contains("Brake", StringComparison.OrdinalIgnoreCase)) price = 179.99m;
            else if (request.ServiceType.Contains("Routine", StringComparison.OrdinalIgnoreCase) || request.ServiceType.Contains("General", StringComparison.OrdinalIgnoreCase)) price = 119.99m;
            else if (request.ServiceType.Contains("AC", StringComparison.OrdinalIgnoreCase)) price = 89.99m;
            else if (request.ServiceType.Contains("Detail", StringComparison.OrdinalIgnoreCase)) price = 149.99m;
        }

        decimal discount = request.Discount ?? 0m;
        decimal tax = Math.Round(price * 0.10m, 2); // 10% tax rate
        decimal totalAmount = Math.Max(0, price + tax - discount);

        var booking = new ServiceBooking
        {
            UserId = targetUserId,
            VehicleId = request.VehicleId,
            ServiceType = request.ServiceType.Trim(),
            BookingDate = request.BookingDate,
            Status = string.IsNullOrWhiteSpace(request.Status) ? ServiceBookingStatus.Pending : request.Status.Trim(),
            Price = price,
            Tax = tax,
            Discount = discount,
            TotalAmount = totalAmount,
            PaymentStatus = "Pending",
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.AddBookingAsync(booking);

        // Fetch complete record with navigation properties
        var createdBooking = await _bookingRepository.GetBookingByIdAsync(booking.Id);
        return MapToResponseDto(createdBooking ?? booking);
    }

    public async Task<IEnumerable<ServiceBookingResponseDto>> GetAllBookingsAsync(int currentUserId, bool isAdmin)
    {
        // Security: Non-admin users only see their own bookings.
        IEnumerable<ServiceBooking> bookings = isAdmin
            ? await _bookingRepository.GetAllBookingsAsync()
            : await _bookingRepository.GetBookingsByUserIdAsync(currentUserId);

        return bookings.Select(MapToResponseDto);
    }

    public async Task<PagedResult<ServiceBookingResponseDto>> GetBookingsPagedAsync(int currentUserId, bool isAdmin, PaginationParams pagination)
    {
        int? filterUserId = isAdmin ? null : currentUserId;
        var (items, totalCount) = await _bookingRepository.GetBookingsPagedAsync(pagination.PageNumber, pagination.PageSize, filterUserId);

        var dtos = items.Select(MapToResponseDto);
        return new PagedResult<ServiceBookingResponseDto>(dtos, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<ServiceBookingResponseDto?> GetBookingByIdAsync(int id, int currentUserId, bool isAdmin)
    {
        var b = await _bookingRepository.GetBookingByIdAsync(id);
        if (b == null) return null;

        // Security check: verify ownership
        if (!isAdmin && b.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this booking.");
        }

        return MapToResponseDto(b);
    }

    public async Task<ServiceBookingResponseDto?> UpdateBookingAsync(int id, ServiceBookingRequestDto request, int currentUserId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(id);
        if (booking == null) return null;

        // Security check: verify ownership
        if (!isAdmin && booking.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this booking.");
        }

        booking.VehicleId = request.VehicleId;
        booking.ServiceType = request.ServiceType.Trim();
        booking.BookingDate = request.BookingDate;
        booking.Notes = request.Notes?.Trim();
        if (request.CustomPrice.HasValue) booking.Price = request.CustomPrice.Value;
        if (request.Discount.HasValue) booking.Discount = request.Discount.Value;

        booking.Tax = Math.Round(booking.Price * 0.10m, 2);
        booking.TotalAmount = Math.Max(0, booking.Price + booking.Tax - booking.Discount);
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateBookingAsync(booking);

        var updated = await _bookingRepository.GetBookingByIdAsync(id);
        return MapToResponseDto(updated ?? booking);
    }

    public async Task<ServiceBookingResponseDto?> UpdateBookingStatusAsync(int id, string newStatus)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(id);
        if (booking == null) return null;

        string oldStatus = booking.Status;
        if (!ServiceBookingStatus.IsValidTransition(oldStatus, newStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from '{oldStatus}' to '{newStatus}'.");
        }

        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateBookingAsync(booking);

        // Notify customer on status update
        await _notificationService.CreateNotificationAsync(
            booking.UserId,
            booking.Id,
            $"Your vehicle service status for booking #{booking.Id} has been updated to: '{newStatus}'."
        );

        var updated = await _bookingRepository.GetBookingByIdAsync(id);
        return MapToResponseDto(updated ?? booking);
    }

    public async Task<bool> DeleteBookingAsync(int id, int currentUserId, bool isAdmin)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(id);
        if (booking == null) return false;

        // Security check: verify ownership
        if (!isAdmin && booking.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this booking.");
        }

        await _bookingRepository.DeleteBookingAsync(booking);
        return true;
    }

    private static ServiceBookingResponseDto MapToResponseDto(ServiceBooking b)
    {
        string vehicleDetails = b.Vehicle != null
            ? $"{b.Vehicle.Brand} {b.Vehicle.Model} ({b.Vehicle.NumberPlate})"
            : $"Vehicle #{b.VehicleId}";

        bool isInvoiceAvailable = b.PaymentStatus == "Completed" ||
                                  b.Status == ServiceBookingStatus.ServiceCompleted ||
                                  b.Status == ServiceBookingStatus.Delivered;

        return new ServiceBookingResponseDto
        {
            Id = b.Id,
            UserId = b.UserId,
            CustomerName = b.User?.Name ?? $"Customer #{b.UserId}",
            CustomerEmail = b.User?.Email ?? string.Empty,
            VehicleId = b.VehicleId,
            VehicleDetails = vehicleDetails,
            ServiceType = b.ServiceType,
            BookingDate = b.BookingDate,
            Status = b.Status,
            Price = b.Price,
            Tax = b.Tax,
            Discount = b.Discount,
            TotalAmount = b.TotalAmount > 0 ? b.TotalAmount : b.Price,
            PaymentStatus = b.PaymentStatus,
            Notes = b.Notes,
            CreatedAt = b.CreatedAt,
            IsInvoiceAvailable = isInvoiceAvailable
        };
    }
}
