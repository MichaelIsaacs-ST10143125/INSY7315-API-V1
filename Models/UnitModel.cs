using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class UnitModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("isAvailable")]
        public bool IsAvailable { get; set; }

        // If you want Firestore references to be stored as strings, this works
        [FirestoreProperty("leaseID")]
        public string? LeaseID { get; set; }

        [FirestoreProperty("rentAmount")]
        public string RentAmount { get; set; } = null!;

        [FirestoreProperty("tenantID")]
        public string? TenantID { get; set; }

        [FirestoreProperty("unitNumber")]
        public string UnitNumber { get; set; } = null!;
    }
}
