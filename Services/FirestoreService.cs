using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using System;
using System.IO;

namespace NewDawnPropertiesApi_V1.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _db;

        public FirestoreService(IConfiguration config)
        {
            string projectId = config["Firebase:ProjectId"];
            GoogleCredential credential;

            // First, try to get credentials from environment variable (Render)
            var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            if (!string.IsNullOrEmpty(firebaseJson))
            {
                credential = GoogleCredential.FromJson(firebaseJson);
                Console.WriteLine("✅ Using FIREBASE_CONFIG from environment variable.");
            }
            else
            {
                // Fallback for local development
                var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-key.json");
                if (!File.Exists(localPath))
                    throw new FileNotFoundException(
                        "Firebase credentials not found. Please add firebase-key.json or set FIREBASE_CONFIG environment variable.");

                credential = GoogleCredential.FromFile(localPath);
                Console.WriteLine("✅ Using local firebase-key.json file.");
            }

            // Convert credentials to gRPC channel credentials
            var channelCredentials = credential.ToChannelCredentials();

            // Create FirestoreClient and pass to FirestoreDb
            var client = new FirestoreClientBuilder
            {
                ChannelCredentials = channelCredentials
            }.Build();

            _db = FirestoreDb.Create(projectId, client);

            Console.WriteLine("✅ Connected to Firestore successfully.");
        }

        public FirestoreDb GetDb() => _db;
    }
}
