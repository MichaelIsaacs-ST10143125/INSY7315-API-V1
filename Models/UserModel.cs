using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace NewDawnPropertiesApi_V1.Models
{
    public class UserModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string UserName { get; set; } = null!;

        [FirestoreProperty]
        public string Password { get; set; } = null!;

        [FirestoreProperty]
        public string Email { get; set; } = null!;

        [FirestoreProperty]
        public string? PhoneNumber { get; set; }

        [FirestoreProperty]
        public string Role { get; set; } = null!;

        [FirestoreProperty]
        public string FName { get; set; } = null!;

        [FirestoreProperty]
        public string SName { get; set; } = null!;
    }
}
