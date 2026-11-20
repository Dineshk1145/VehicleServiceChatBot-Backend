using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;
using VehicleServiceChatbot.Persistence.Context;

namespace VehicleServiceChatbot.Persistence.Repositories
{
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly VehicleServiceDbContext _context;

        public ChatSessionRepository(VehicleServiceDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession?> GetByIdAsync(string sessionId)
        {
            return await _context.ChatSessions.FindAsync(sessionId);
        }

        public async Task SaveAsync(ChatSession session)
        {
            var existing = await _context.ChatSessions.FindAsync(session.SessionId);
            if (existing == null)
            {
                await _context.ChatSessions.AddAsync(session);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(session);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string sessionId)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                _context.ChatSessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }
    }
}
