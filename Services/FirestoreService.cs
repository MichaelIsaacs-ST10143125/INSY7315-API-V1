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

//using Google.Apis.Auth.OAuth2;
//using Google.Cloud.Firestore;
//using Google.Cloud.Firestore.V1;
//using Grpc.Auth;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.IO;
//using System.Text.Json;

//namespace NewDawnPropertiesApi_V1.Services
//{
//    public class FirestoreService
//    {
//        private readonly FirestoreDb _db;

//        public FirestoreService(IConfiguration config)
//        {
//            // Get Firebase project ID from configuration
//            string projectId = config["Firebase:ProjectId"];

//            // Build path to local Firebase config file
//            string firebaseConfigPath = Path.Combine(AppContext.BaseDirectory, "firebase-key.json");

//            // Verify file exists
//            if (!File.Exists(firebaseConfigPath))
//            {
//                throw new FileNotFoundException(
//                    $"Firebase config file not found at {firebaseConfigPath}. " +
//                    "Please ensure FIREBASE_CONFIG.json is in the project directory and set to 'Copy if newer'.");
//            }

//            // Read the service account JSON
//            string firebaseJson = File.ReadAllText(firebaseConfigPath);

//            // Create GoogleCredential from JSON file contents
//            var credential = GoogleCredential.FromJson(firebaseJson);

//            // Convert to gRPC channel credentials
//            var channelCredentials = credential.ToChannelCredentials();

//            // Build FirestoreClient using the credentials
//            var client = new FirestoreClientBuilder
//            {
//                ChannelCredentials = channelCredentials
//            }.Build();

//            // Initialize FirestoreDb
//            _db = FirestoreDb.Create(projectId, client);

//            Console.WriteLine("✅ Connected to Firestore successfully.");
//        }

//        public FirestoreDb GetDb() => _db;
//    }
//}
