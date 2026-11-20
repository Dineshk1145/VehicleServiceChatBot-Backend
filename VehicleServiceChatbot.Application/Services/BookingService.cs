using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Domain.Enums;

namespace VehicleServiceChatbot.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServicePackageRepository _servicePackageRepository;

        public BookingService(IBookingRepository bookingRepository, IServicePackageRepository servicePackageRepository)
        {
            _bookingRepository = bookingRepository;
            _servicePackageRepository = servicePackageRepository;
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Select(MapToDto);
        }

        public async Task<BookingDto?> GetBookingByIdAsync(string id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking != null ? MapToDto(booking) : null;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
        {
            var service = await _servicePackageRepository.GetByIdAsync(dto.ServiceId);
            if (service == null)
            {
                throw new ArgumentException("Invalid service selected.");
            }

            var booking = new Booking
            {
                Id = "BK-" + new Random().Next(100000, 999999),
                Vehicle = dto.Vehicle,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate,
                ServiceId = service.Id,
                ServiceName = service.Name,
                Price = service.Price,
                Date = dto.Date,
                Time = dto.Time,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerEmail = dto.CustomerEmail,
                Notes = dto.Notes,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _bookingRepository.AddAsync(booking);
            return MapToDto(created);
        }

        public async Task<BookingDto?> UpdateBookingStatusAsync(string id, string status)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return null;

            if (Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
            {
                booking.Status = parsedStatus;
                booking.UpdatedAt = DateTime.UtcNow;
                var updated = await _bookingRepository.UpdateAsync(booking);
                return updated != null ? MapToDto(updated) : null;
            }

            throw new ArgumentException($"Invalid status value: {status}");
        }

        public async Task<bool> DeleteBookingAsync(string id)
        {
            return await _bookingRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ServicePackage>> GetServicesAsync()
        {
            return await _servicePackageRepository.GetAllAsync();
        }

        private static BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                Id = b.Id,
                Vehicle = b.Vehicle,
                Year = b.Year,
                LicensePlate = b.LicensePlate,
                ServiceId = b.ServiceId,
                ServiceName = b.ServiceName,
                Price = b.Price,
                Date = b.Date,
                Time = b.Time,
                CustomerName = b.CustomerName,
                CustomerPhone = b.CustomerPhone,
                CustomerEmail = b.CustomerEmail,
                Notes = b.Notes,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt
            };
        }
    }
}
