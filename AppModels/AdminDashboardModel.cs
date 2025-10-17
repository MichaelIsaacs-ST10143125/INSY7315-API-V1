namespace NewDawnPropertiesApi_V1.AppModels
{
    public class AdminDashboardModel
    {
        public int? tenants { get; set; }
        public int? properties { get; set; }
        public string? occupancyRate { get; set; }
        public string? rentCollected { get; set; }
        public string? outstandingRent { get; set; }
        public int? maintenanceRequests { get; set; }
    }
}
