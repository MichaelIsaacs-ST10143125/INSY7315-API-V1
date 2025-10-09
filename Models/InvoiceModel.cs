using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class InvoiceModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("amount")]
        public double Amount { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [FirestoreProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [FirestoreProperty("leaseID")]
        public string? LeaseID { get; set; }

        [FirestoreProperty("tenantID")]
        public string? TenantID { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }
    }

}
