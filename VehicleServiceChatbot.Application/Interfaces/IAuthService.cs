using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Auth;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
}
