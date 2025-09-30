using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace NewDawnPropertiesApi_V1.Models
{
    public class TenantAssignmentModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? UserID { get; set; }

        [FirestoreProperty]
        public string? PropID { get; set; }

        [FirestoreProperty]
        public string? RoomID { get; set; }
    }
}
