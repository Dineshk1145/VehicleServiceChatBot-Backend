using System;

namespace VehicleServiceChatbot.Application.DTOs;

public class InvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string VehicleBrand { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string NumberPlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal ServicePrice { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
}
