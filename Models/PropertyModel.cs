using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    public class PropertyModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? PropName { get; set; }

        [FirestoreProperty]
        public string ListPrice { get; set; } = null!;

        [FirestoreProperty]
        public string Address { get; set; } = null!;

        [FirestoreProperty]
        public string City { get; set; } = null!;

        [FirestoreProperty]
        public string UserID { get; set; } = null!; // Firestore uses string doc IDs

        [FirestoreProperty]
        public int RoomsCount { get; set; }

        [FirestoreProperty]
        public string? ListImageBase64 { get; set; } // store as base64 (or use Firebase Storage)

        [FirestoreProperty]
        public string? ListVideoBase64 { get; set; }
    }
}
