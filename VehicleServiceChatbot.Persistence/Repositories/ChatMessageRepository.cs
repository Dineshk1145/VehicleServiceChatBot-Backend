using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;

namespace VehicleServiceChatbot.Persistence.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly ApplicationDbContext _context;

    public ChatMessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddChatMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetAllChatMessagesAsync()
    {
        return await _context.ChatMessages.ToListAsync();
    }
}
