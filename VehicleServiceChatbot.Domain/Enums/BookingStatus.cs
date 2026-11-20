namespace VehicleServiceChatbot.Domain.Enums
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        VehicleReceived,
        ServiceInProgress,
        ServiceCompleted,
        ReadyForDelivery,
        Delivered,
        Cancelled
    }

    public static class ServiceBookingStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string VehicleReceived = "Vehicle Received";
        public const string ServiceInProgress = "Service In Progress";
        public const string ServiceCompleted = "Service Completed";
        public const string ReadyForDelivery = "Ready for Delivery";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static readonly string[] ValidStatuses = new[]
        {
            Pending,
            Confirmed,
            VehicleReceived,
            ServiceInProgress,
            ServiceCompleted,
            ReadyForDelivery,
            Delivered,
            Cancelled
        };

        public static bool IsValidTransition(string currentStatus, string newStatus)
        {
            if (currentStatus == newStatus) return true;
            if (newStatus == Cancelled) return currentStatus != Delivered;

            int currentIndex = Array.IndexOf(ValidStatuses, currentStatus);
            int newIndex = Array.IndexOf(ValidStatuses, newStatus);

            if (currentIndex == -1 || newIndex == -1) return false;
            // Admin can advance status forward or set cancelled
            return newIndex > currentIndex;
        }
    }
}

