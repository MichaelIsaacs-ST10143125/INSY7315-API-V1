using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class LeaseModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("deposit")]
        public double Deposit { get; set; }

        [FirestoreProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [FirestoreProperty("endDate")]
        public DateTime? EndDate { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }

        [FirestoreProperty("tenantID")]
        public string? TenantID { get; set; }

        [FirestoreProperty("propertyID")]
        public string? PropertyID { get; set; }

        [FirestoreProperty("unitID")]
        public string? UnitID { get; set; }
    }

}
