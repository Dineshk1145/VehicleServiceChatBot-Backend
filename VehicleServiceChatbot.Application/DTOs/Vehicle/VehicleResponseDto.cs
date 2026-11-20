namespace VehicleServiceChatbot.Application.DTOs.Vehicle;

public class VehicleResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string NumberPlate { get; set; } = string.Empty;
}
