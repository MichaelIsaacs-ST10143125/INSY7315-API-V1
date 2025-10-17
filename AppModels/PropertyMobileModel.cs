namespace NewDawnPropertiesApi_V1.AppModels
{
    public class PropertyMobileModel
    {
        public Boolean? isAvailable { get; set; }
        public string? unitNumber { get; set; }
        public string? propertyName { get; set; }
        public string? propertyAddress { get; set; }
        public string[]? amenities { get; set; }
    }
}
