namespace NewDawnPropertiesApi_V1.AppModels
{
    public class ManagerDashboardModel
    {
        public int? tenantCount { get; set; }
        public string? occupancyRate { get; set; }
        public int? leasesExpiringSoon { get; set; }
        public int? totalRentCollectedThisMonth { get; set; }
        public int? totalOutstandingRent { get; set; }
        public int? maintenanceRequestsCount { get; set; }
    }
}
