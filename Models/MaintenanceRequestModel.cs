using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class MaintenanceRequestModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("assignedTo")]
        public string? AssignedTo { get; set; }

        [FirestoreProperty("category")]
        public string? Category { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [FirestoreProperty("description")]
        public string? Description { get; set; }

        [FirestoreProperty("propertyID")]
        public string? PropertyID { get; set; }

        [FirestoreProperty("unitID")]
        public string? UnitID { get; set; }

        [FirestoreProperty("tenantID")]
        public string? TenantID { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }

        [FirestoreProperty("urgency")]
        public string? Urgency { get; set; }
    }

}
