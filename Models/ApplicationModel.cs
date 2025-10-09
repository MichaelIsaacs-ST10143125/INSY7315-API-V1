using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class ApplicationModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("email")]
        public string? Email { get; set; }

        [FirestoreProperty("name")]
        public string? Name { get; set; }

        [FirestoreProperty("phone")]
        public string? Phone { get; set; }

        [FirestoreProperty("propertyID")]
        public string? PropertyID { get; set; }

        [FirestoreProperty("unitID")]
        public string? UnitID { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }

        [FirestoreProperty("submittedAt")]
        public DateTime? SubmittedAt { get; set; }
    }

}
