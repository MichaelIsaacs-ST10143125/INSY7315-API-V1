using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    public class CaretakerAssignmentModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? UserID { get; set; }

        [FirestoreProperty]
        public string? PropID { get; set; }
    }
}
