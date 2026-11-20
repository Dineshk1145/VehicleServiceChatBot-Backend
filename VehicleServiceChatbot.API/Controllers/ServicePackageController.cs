using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServicePackageController : ControllerBase
{
    private readonly IServicePackageRepository _packageRepository;

    public ServicePackageController(IServicePackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPackages()
    {
        var packages = await _packageRepository.GetAllAsync();
        return Ok(packages);
    }
}
