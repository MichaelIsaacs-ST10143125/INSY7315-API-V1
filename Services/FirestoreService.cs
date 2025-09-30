using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _db;

        public FirestoreService(IConfiguration config)
        {
            string projectId = config["Firebase:ProjectId"];
            _db = FirestoreDb.Create(projectId);
        }

        public FirestoreDb GetDb() => _db;
    }
}
