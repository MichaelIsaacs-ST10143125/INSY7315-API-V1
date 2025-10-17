using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData]
    public class MessageModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("chatID")]
        public string? ChatID { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [FirestoreProperty("message")]
        public string? Message { get; set; }

        [FirestoreProperty("senderID")]
        public string? SenderID { get; set; }

        [FirestoreProperty("receiverID")]
        public string? ReceiverID { get; set; }
    }

}
