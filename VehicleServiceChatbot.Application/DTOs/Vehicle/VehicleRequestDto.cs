using System.ComponentModel.DataAnnotations;

namespace VehicleServiceChatbot.Application.DTOs.Vehicle;

public class VehicleRequestDto
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "VehicleType is required")]
    [StringLength(50, ErrorMessage = "VehicleType cannot exceed 50 characters")]
    public string VehicleType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Brand is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Brand must be between 1 and 50 characters")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Model must be between 1 and 50 characters")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "NumberPlate is required")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Number plate must be between 2 and 20 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Number plate can only contain alphanumeric characters, spaces, and hyphens")]
    public string NumberPlate { get; set; } = string.Empty;
}
