using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace NewDawnPropertiesApi_V1.Models
{
    [FirestoreData] // Make sure this is here
    public class UserModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("email")]
        public string? Email { get; set; }

        [FirestoreProperty("password")]
        public string? Password { get; set; }

        [FirestoreProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [FirestoreProperty("name")]
        public string? Name { get; set; }

        [FirestoreProperty("located")]
        public string? Located { get; set; } = "Birchleigh";

        [FirestoreProperty("status")]
        public string? Status { get; set; } = "Active";

        [FirestoreProperty("userType")]
        public string? UserType { get; set; }

        [FirestoreProperty("preferences")]
        public Preferences preferences { get; set; } = new Preferences();

        [FirestoreProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }

    public class Preferences
{
    public string language { get; set; } = "en";
    public bool notifications { get; set; } = true;     
    public string theme { get; set; } = "light";
}
}
