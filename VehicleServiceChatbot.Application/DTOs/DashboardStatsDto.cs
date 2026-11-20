namespace VehicleServiceChatbot.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int VehicleReceivedBookings { get; set; }
    public int InProgressServices { get; set; }
    public int CompletedServices { get; set; }
    public int ReadyForDeliveryBookings { get; set; }
    public int DeliveredServices { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingPayments { get; set; }
    public int CompletedPayments { get; set; }
}
