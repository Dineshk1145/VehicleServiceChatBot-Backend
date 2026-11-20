using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.Application.Services;

public class PdfInvoiceService : IInvoiceService
{
    private readonly IServiceBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;

    public PdfInvoiceService(
        IServiceBookingRepository bookingRepository,
        IPaymentRepository paymentRepository)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<InvoiceDto?> GetInvoiceDataAsync(int currentUserId, bool isAdmin, int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null) return null;

        if (!isAdmin && booking.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this invoice.");
        }

        var payment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);

        string vehicleDetails = booking.Vehicle != null
            ? $"{booking.Vehicle.Brand} {booking.Vehicle.Model} ({booking.Vehicle.NumberPlate})"
            : "Vehicle N/A";

        return new InvoiceDto
        {
            InvoiceNumber = $"INV-{booking.BookingDate:yyyyMM}-{booking.Id:D4}",
            CustomerName = booking.User?.Name ?? "Valued Customer",
            CustomerEmail = booking.User?.Email ?? "customer@autocare.com",
            VehicleBrand = booking.Vehicle?.Brand ?? "N/A",
            VehicleModel = booking.Vehicle?.Model ?? "N/A",
            NumberPlate = booking.Vehicle?.NumberPlate ?? "N/A",
            VehicleType = booking.Vehicle?.VehicleType ?? "Auto",
            ServiceName = booking.ServiceType,
            ServiceDate = booking.BookingDate,
            ServicePrice = booking.Price,
            Tax = booking.Tax,
            Discount = booking.Discount,
            TotalAmount = booking.TotalAmount > 0 ? booking.TotalAmount : booking.Price,
            PaymentStatus = booking.PaymentStatus,
            TransactionId = payment?.TransactionId ?? "N/A",
            InvoiceDate = payment?.PaymentDate ?? DateTime.UtcNow
        };
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int currentUserId, bool isAdmin, int bookingId)
    {
        var invoice = await GetInvoiceDataAsync(currentUserId, isAdmin, bookingId);
        if (invoice == null)
        {
            throw new Exception($"Invoice for Booking #{bookingId} not found.");
        }

        return GenerateMinimalPdf(invoice);
    }

    private static byte[] GenerateMinimalPdf(InvoiceDto data)
    {
        // Build raw valid PDF 1.4 document stream
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII);

        var textLines = new List<string>
        {
            "AUTOCARE SERVICE CENTER - INVOICE",
            "--------------------------------------------------",
            $"Invoice No    : {data.InvoiceNumber}",
            $"Invoice Date  : {data.InvoiceDate:yyyy-MM-dd}",
            $"Payment Status: {data.PaymentStatus.ToUpper()}",
            $"Transaction ID: {data.TransactionId}",
            "",
            "CUSTOMER DETAILS",
            "--------------------------------------------------",
            $"Name : {data.CustomerName}",
            $"Email: {data.CustomerEmail}",
            "",
            "VEHICLE DETAILS",
            "--------------------------------------------------",
            $"Vehicle     : {data.VehicleBrand} {data.VehicleModel}",
            $"Number Plate: {data.NumberPlate}",
            $"Type        : {data.VehicleType}",
            "",
            "SERVICE & BILLING BREAKDOWN",
            "--------------------------------------------------",
            $"Service     : {data.ServiceName}",
            $"Date        : {data.ServiceDate:yyyy-MM-dd}",
            $"Base Price  : INR {data.ServicePrice:F2}",
            $"Tax         : INR {data.Tax:F2}",
            $"Discount    : -INR {data.Discount:F2}",
            "--------------------------------------------------",
            $"TOTAL AMOUNT: INR {data.TotalAmount:F2}",
            "--------------------------------------------------",
            "",
            "Thank you for choosing AutoCare Smart Service Portal!"
        };

        // Construct standard PDF stream
        var pdfContent = new StringBuilder();
        pdfContent.AppendLine("%PDF-1.4");

        // Object 1: Catalog
        pdfContent.AppendLine("1 0 obj");
        pdfContent.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
        pdfContent.AppendLine("endobj");

        // Object 2: Pages
        pdfContent.AppendLine("2 0 obj");
        pdfContent.AppendLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        pdfContent.AppendLine("endobj");

        // Object 3: Page
        pdfContent.AppendLine("3 0 obj");
        pdfContent.AppendLine("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>");
        pdfContent.AppendLine("endobj");

        // Object 4: Font
        pdfContent.AppendLine("4 0 obj");
        pdfContent.AppendLine("<< /Type /Font /Subtype /Type1 /BaseFont /Courier-Bold >>");
        pdfContent.AppendLine("endobj");

        // Build Stream Content for text rendering
        var streamText = new StringBuilder();
        streamText.AppendLine("BT");
        streamText.AppendLine("/F1 12 Tf");
        streamText.AppendLine("14 TL");
        streamText.AppendLine("50 800 Td");

        foreach (var line in textLines)
        {
            string safeLine = line.Replace("(", "\\(").Replace(")", "\\)");
            streamText.AppendLine($"({safeLine}) '");
        }

        streamText.AppendLine("ET");

        string streamStr = streamText.ToString();
        byte[] streamBytes = Encoding.ASCII.GetBytes(streamStr);

        // Object 5: Stream
        pdfContent.AppendLine("5 0 obj");
        pdfContent.AppendLine($"<< /Length {streamBytes.Length} >>");
        pdfContent.AppendLine("stream");
        pdfContent.Append(streamStr);
        pdfContent.AppendLine("endstream");
        pdfContent.AppendLine("endobj");

        // Xref Table & Trailer
        string pdfTextBeforeXref = pdfContent.ToString();
        byte[] pdfBytesBeforeXref = Encoding.ASCII.GetBytes(pdfTextBeforeXref);

        int xrefOffset = pdfBytesBeforeXref.Length;

        var xref = new StringBuilder();
        xref.AppendLine("xref");
        xref.AppendLine("0 6");
        xref.AppendLine("0000000000 65535 f ");
        xref.AppendLine("0000000009 00000 n ");
        xref.AppendLine("0000000058 00000 n ");
        xref.AppendLine("0000000115 00000 n ");
        xref.AppendLine("0000000244 00000 n ");
        xref.AppendLine("0000000326 00000 n ");
        xref.AppendLine("trailer");
        xref.AppendLine("<< /Size 6 /Root 1 0 R >>");
        xref.AppendLine("startxref");
        xref.AppendLine(xrefOffset.ToString());
        xref.AppendLine("%%EOF");

        pdfContent.Append(xref.ToString());

        return Encoding.ASCII.GetBytes(pdfContent.ToString());
    }
}
