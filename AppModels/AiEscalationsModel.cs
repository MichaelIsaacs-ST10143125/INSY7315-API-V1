using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.AppModels
{
    [FirestoreData]
    public class AiEscalationsModel
    {

        [FirestoreDocumentId]
        public string? id { get; set; }
        public string? chatID { get; set; }
        public string? reason { get; set; }
        public string? issue { get; set; }
        public DateTime? createdAt { get; set; }
        public string? status { get; set; }
        public string? urgency { get; set; }
        public string? user { get; set; }
        public string? category { get; set; }
    }
}
