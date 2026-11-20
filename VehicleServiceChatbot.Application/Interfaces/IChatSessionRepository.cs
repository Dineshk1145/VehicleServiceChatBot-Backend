using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces
{
    public interface IChatSessionRepository
    {
        Task<ChatSession?> GetByIdAsync(string sessionId);
        Task SaveAsync(ChatSession session);
        Task DeleteAsync(string sessionId);
    }
}
