using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using System;

namespace NewDawnPropertiesApi_V1.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _db;

        public FirestoreService(IConfiguration config)
        {
            // Get Firebase project ID from configuration
            string projectId = config["Firebase:ProjectId"];

            // Get Firebase credentials JSON from environment variable
            var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

            if (string.IsNullOrEmpty(firebaseJson))
            {
                throw new InvalidOperationException(
                    "FIREBASE_CONFIG environment variable is not set. " +
                    "Please add your Firebase service account JSON as an environment variable.");
            }

            // Create GoogleCredential from JSON
            var credential = GoogleCredential.FromJson(firebaseJson);

            // Convert to gRPC channel credentials
            var channelCredentials = credential.ToChannelCredentials();

            // Build FirestoreClient using the credentials
            var client = new FirestoreClientBuilder
            {
                ChannelCredentials = channelCredentials
            }.Build();

            // Initialize FirestoreDb with client
            _db = FirestoreDb.Create(projectId, client);

            Console.WriteLine("✅ Connected to Firestore successfully.");
        }

        public FirestoreDb GetDb() => _db;
    }
}
