using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;
// Keep This Controller

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : BaseFirestoreController<UserModel>
    {
        private readonly FirestoreDb _firestore;

        public UsersController(FirestoreService firestoreService)
            : base(firestoreService, "maintenanceRequests")
        {
            _firestore = firestoreService.GetDb();
        }

        // GET api/users/mobile/admin/{uid}
        [HttpGet("mobile/admin/{uid}")]
        public async Task<ActionResult<IEnumerable<AdminUserModel>>> GetAllUsers(string uid)
        {
            // Get current user
            var currentUserDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
            if (!currentUserDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string role = currentUserDoc.GetValue<string>("userType");

            // Only admins can view all users
            if (role != "Admin")
                return Forbid("Only admins can view all users.");

            // Get all users
            var usersSnapshot = await _firestore.Collection("users").GetSnapshotAsync();

            // Map to model and exclude current user
            var allUsers = usersSnapshot.Documents
                .Where(d => d.Id != uid) // exclude current user
                .Select(d => new AdminUserModel
                {
                    FullName = d.ContainsField("name") ? d.GetValue<string>("name") : null,
                    Email = d.ContainsField("email") ? d.GetValue<string>("email") : null,
                    UserType = d.ContainsField("userType") ? d.GetValue<string>("userType") : null,
                    Located = d.ContainsField("located") ? d.GetValue<string>("located") : null
                })
                .ToList();

            return Ok(allUsers);
        }

        [HttpGet("mobile/user/list/{uid}")]
        public async Task<ActionResult<IEnumerable<UsersMessageModel>>> GetUserList(string uid)
        {
            // Get current user
            var currentUserDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();

            if (!currentUserDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string role = currentUserDoc.GetValue<string>("userType");
            string located = currentUserDoc.GetValue<string>("located");
            string[] locationParts = located.Split(" ");
            string currentLocation = locationParts[0];

            List<DocumentSnapshot> allUserDocs = new();

            if (role == "Admin")
            {
                // Admins get all users except themselves
                var usersSnapshot = await _firestore.Collection("users").GetSnapshotAsync();
                allUserDocs = usersSnapshot.Documents
                    .Where(d => d.Id != uid)
                    .ToList();
            }
            else
            {
                // Get users in the same complex
                var sameComplexSnapshot = await _firestore.Collection("users")
                    .WhereEqualTo("located", currentLocation)
                    .GetSnapshotAsync();

                // Get admins separately
                var adminsSnapshot = await _firestore.Collection("users")
                    .WhereEqualTo("userType", "Admin")
                    .GetSnapshotAsync();

                // Combine lists and remove duplicates (based on user ID)
                allUserDocs = sameComplexSnapshot.Documents
                    .Concat(adminsSnapshot.Documents)
                    .GroupBy(d => d.Id)
                    .Select(g => g.First())
                    .Where(d => d.Id != uid) // exclude current user
                    .ToList();
            }

            var userList = allUserDocs
                .Select(d => new UsersMessageModel
                {
                    name = d.ContainsField("name") ? d.GetValue<string>("name") : string.Empty,
                    userEmail = d.ContainsField("email") ? d.GetValue<string>("email") : string.Empty
                })
                .ToList();

            return Ok(userList);
        }



    }
}
