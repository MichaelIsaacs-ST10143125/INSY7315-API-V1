using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace NewDawnPropertiesApi_V1.Models
{
    public class RoomModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Block { get; set; }

        [FirestoreProperty]
        public string? PropID { get; set; }
    }
}
