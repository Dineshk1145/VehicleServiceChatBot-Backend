using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces
{
    public interface IServicePackageRepository
    {
        Task<IEnumerable<ServicePackage>> GetAllAsync();
        Task<ServicePackage?> GetByIdAsync(string id);
    }
}
