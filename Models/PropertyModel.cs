using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class PropertyModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; } = null!;

        [FirestoreProperty("address")]
        public string Address { get; set; } = null!;

        [FirestoreProperty("amenities")]
        public List<string>? Amenities { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; } = null!;

        [FirestoreProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }
}
