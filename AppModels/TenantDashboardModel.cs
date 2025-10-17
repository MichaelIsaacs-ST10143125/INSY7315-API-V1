namespace NewDawnPropertiesApi_V1.AppModels
{
    public class TenantDashboardModel
    {
        public string? LatestInvoiceStatus { get; set; }
        public DateTime? LatestInvoiceDueDate { get; set; }
        public decimal? LatestInvoiceAmount { get; set; }
        public int ActiveMaintenanceCount { get; set; }
    }
}
