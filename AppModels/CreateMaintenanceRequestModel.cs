namespace NewDawnPropertiesApi_V1.AppModels
{
    public class CreateMaintenanceRequestModel
    {
        public string Uid { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
