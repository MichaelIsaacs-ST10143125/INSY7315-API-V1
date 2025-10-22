using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly FirestoreDb _firestore;

        public MessagesController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        //[HttpGet("messages/mobile/{uid}/{receiverEmail}")]
        //public async Task<ActionResult<IEnumerable<object>>> GetChatLog(string uid, string receiverEmail)
        //{
        //    // curent user
        //    var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
        //    if (!userDoc.Exists)
        //        return NotFound($"User with UID '{uid}' not found.");

        //    string userEmail = userDoc.GetValue<string>("email");

        //    // receiver user
        //    //var receiverDoc = await _firestore.Collection("users").Document(receiverID).GetSnapshotAsync();
        //    //if (!receiverDoc.Exists)
        //    //    return NotFound($"User with UID '{receiverID}' not found.");

        //    //string receiverEmail = receiverDoc.GetValue<string>("email");

        //    // Create a consistent chat ID based on both emails
        //    // This ensures the chat ID is the same regardless of who is the sender or receiver
        //    var chatID = string.Compare(userEmail, receiverEmail) < 0
        //        ? $"{userEmail}_{receiverEmail}"
        //        : $"{receiverEmail}_{userEmail}";

        //    // Gets all the messages in the chat
        //    // Orders them by creation time
        //    var messagesSnapshot = await _firestore.Collection("messages")
        //        .WhereEqualTo("chatID", chatID)
        //        .OrderBy("createdAt")
        //        .Limit(10) // Limit to the latest 10 messages
        //        .GetSnapshotAsync();

        //    var messageResults = messagesSnapshot.Documents
        //        .Select(d => d.ConvertTo<MessageModel>())
        //        .ToList();

        //    return Ok(messageResults);
        //}

        //[HttpGet("users/mobile/{uid}")]
        //public async Task<ActionResult<IEnumerable<object>>> Getusers(string uid)
        //{
        //    var userDoc = await _firestore.Collection("users")
        //        .Document(uid)
        //        .GetSnapshotAsync();

        //    if (!userDoc.Exists)
        //        return NotFound($"User with UID '{uid}' not found.");

        //    string role = userDoc.GetValue<string>("userType");
        //    string fullLocated = userDoc.GetValue<string>("located");
        //    string located = fullLocated.Split(' ')[0];
        //    string email = userDoc.GetValue<string>("email");

        //    List<DocumentSnapshot> userDocuments = new List<DocumentSnapshot>();

        //    if (role == "Admin")
        //    {
        //        // Gets all the users
        //        var snapshot = await _firestore.Collection("users")
        //            .GetSnapshotAsync();

        //        // Adds the users to the list
        //        userDocuments.AddRange(snapshot.Documents);
        //    }
        //    else
        //    {
        //        // Gets all user based on location
        //        var locationSnapshot = await _firestore.Collection("users")
        //            .WhereGreaterThanOrEqualTo("located", located)
        //            .WhereLessThan("located", located + "\uf8ff") // Firestore trick for "starts with"
        //            .GetSnapshotAsync();

        //        // Adds the users to the list
        //        userDocuments.AddRange(locationSnapshot.Documents);

        //        // Gets the Admins user document
        //        var adminSnapshot = await _firestore.Collection("users")
        //            .WhereEqualTo("userType", "Admin")
        //            .GetSnapshotAsync();

        //        // Adds the Admin to the list
        //        userDocuments.AddRange(adminSnapshot.Documents);
        //    }

        //    // Exclude the current user from the results
        //    userDocuments = userDocuments.Where(d => d.GetValue<string>("email") != email).ToList();

        //    var results = userDocuments.Select(d => new UsersMessageModel
        //    {
        //        userEmail = d.GetValue<string>("email"),
        //        name = d.GetValue<string>("name")
        //    }).ToList();

        //    return Ok(results);
        //}

        [HttpGet("get/all/users/messages/{uid}")]
        public async Task<ActionResult<IEnumerable<MessageModel>>> GetAllUsersMessages(string uid)
        {
            var userDoc = await _firestore.Collection("users")
                .Document(uid)
                .GetSnapshotAsync();

            if (!userDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string email = userDoc.GetValue<string>("email");

            // Fetch all messages where user is either sender or receiver
            var sentMessages = await _firestore.Collection("messages")
                .WhereEqualTo("senderID", email)
                .GetSnapshotAsync();

            var receivedMessages = await _firestore.Collection("messages")
                .WhereEqualTo("receiverID", email)
                .GetSnapshotAsync();

            // Combine both sets into one list
            var allMessages = sentMessages.Documents
                .Concat(receivedMessages.Documents)
                .Select(d => d.ConvertTo<MessageModel>())
                .ToList();

            // Group by chatID, sort each group by CreatedAt descending, take latest 6 per group
            var latestSixPerChat = allMessages
                .GroupBy(m => m.ChatID)
                .SelectMany(g => g
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(6))
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            return Ok(latestSixPerChat);
        }

        [HttpPut("add")]
        public async Task<IActionResult> AddMessage([FromBody] AddMessageModel request)
        {
            if (string.IsNullOrEmpty(request.Uid) ||
                string.IsNullOrEmpty(request.ReceiverEmail) ||
                string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Missing one or more required fields: uid, receiverEmail, message.");
            }

            // Get the current user
            var userDoc = await _firestore.Collection("users").Document(request.Uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{request.Uid}' not found.");

            string senderEmail = userDoc.GetValue<string>("email");

            // Build consistent chatID based on both emails
            string chatID = string.Compare(senderEmail, request.ReceiverEmail, StringComparison.OrdinalIgnoreCase) < 0
                ? $"{senderEmail}_{request.ReceiverEmail}"
                : $"{request.ReceiverEmail}_{senderEmail}";

            // Create a new message document
            var messageData = new MessageModel
            {
                ChatID = chatID,
                CreatedAt = DateTime.UtcNow,
                Message = request.Message,
                SenderID = senderEmail,
                ReceiverID = request.ReceiverEmail
            };

            // Add to Firestore
            var messagesRef = _firestore.Collection("messages");
            await messagesRef.AddAsync(messageData);

            return Ok(new
            {
                success = true,
                chatID,
                sender = senderEmail,
                receiver = request.ReceiverEmail,
                createdAt = messageData.CreatedAt
            });
        }


    }

}
