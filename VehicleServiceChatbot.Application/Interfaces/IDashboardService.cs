using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
