using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Chatbot;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Services;

public class ChatbotService : IChatbotService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IServiceBookingRepository _bookingRepository;
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatbotService(
        IVehicleRepository vehicleRepository,
        IServiceBookingRepository bookingRepository,
        IChatMessageRepository chatMessageRepository)
    {
        _vehicleRepository = vehicleRepository;
        _bookingRepository = bookingRepository;
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<ChatbotResponseDto> AskChatbotAsync(ChatbotRequestDto request)
    {
        var msg = request.Message.ToLower().Trim();
        string reply = string.Empty;

        // Security: Use parameterized query filtering by UserId directly at the database level
        var userVehicles = (await _vehicleRepository.GetVehiclesByUserIdAsync(request.UserId)).ToList();
        var userBookings = (await _bookingRepository.GetBookingsByUserIdAsync(request.UserId)).ToList();

        // 1. Booking Status / My Booking
        if (msg.Contains("booking status") || msg.Contains("my booking"))
        {
            if (userBookings.Count == 0)
            {
                reply = "I couldn't find any service bookings under your account. Would you like to book a service? You can create one in the Service Booking tab.";
            }
            else
            {
                var bookingLines = userBookings.Select(b =>
                {
                    var vehicleText = "";
                    var veh = userVehicles.FirstOrDefault(v => v.Id == b.VehicleId);
                    if (veh != null)
                    {
                        vehicleText = $"for {veh.Brand} {veh.Model}";
                    }
                    return $"- Booking #{b.Id} {vehicleText} ({b.ServiceType}) scheduled on {b.BookingDate:yyyy-MM-dd HH:mm}. Status: **{b.Status}**";
                });
                reply = $"I found the following service bookings for you:\n{string.Join("\n", bookingLines)}";
            }
        }
        // 2. My Vehicle
        else if (msg.Contains("my vehicle") || msg.Contains("my car") || msg.Contains("my bike") || msg.Contains("what vehicles"))
        {
            if (userVehicles.Count == 0)
            {
                reply = "You don't have any vehicles registered in our system. Please go to the Vehicle Management page to add one!";
            }
            else
            {
                var vehicleLines = userVehicles.Select(v => $"- {v.Brand} {v.Model} ({v.VehicleType}) - Plate: **{v.NumberPlate}**");
                reply = $"You have {userVehicles.Count} registered vehicle(s):\n{string.Join("\n", vehicleLines)}";
            }
        }
        // 3. Needs Service / Vehicle Service
        else if (msg.Contains("needs service") || msg.Contains("vehicle service") || msg.Contains("service due"))
        {
            if (userVehicles.Count == 0)
            {
                reply = "It looks like you don't have any vehicles registered yet. Please add a vehicle first, and then you can book a service!";
            }
            else
            {
                var mainVehicle = userVehicles.First();
                reply = $"Your {mainVehicle.Brand} {mainVehicle.Model} ({mainVehicle.NumberPlate}) service can be scheduled. Would you like to book a service?";
            }
        }
        // 4. Greetings / Help
        else if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("hey") || msg.Contains("greetings") || msg.Contains("oii"))
        {
            reply = "Hello! I am your Vehicle Service Chatbot assistant. How can I help you today? You can ask me about your registered vehicles ('my vehicles'), check booking statuses ('my bookings'), or check if a vehicle needs service ('needs service').";
        }
        else if (msg.Contains("thank") || msg.Contains("thanks"))
        {
            reply = "You're welcome! Let me know if you need anything else.";
        }
        // Default fall-back response
        else
        {
            reply = "I'm not sure how to answer that, but I can help you with vehicles and bookings! Try asking: \n- *'My vehicle needs service'*\n- *'Show my vehicles'*\n- *'Check my booking status'*\nOr feel free to schedule a booking directly from the Service Booking page.";
        }

        // Save conversation history to ChatMessages using parameterized query
        var chatMessage = new ChatMessage
        {
            UserId = request.UserId,
            Message = request.Message,
            Response = reply,
            CreatedAt = DateTime.UtcNow
        };

        await _chatMessageRepository.AddChatMessageAsync(chatMessage);

        return new ChatbotResponseDto
        {
            Reply = reply
        };
    }
}
