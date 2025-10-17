using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly FirestoreDb _firestore;

        public ProfileController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("user/{uid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserProfileInfo(string uid)
        {
            try
            {
                var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
                if (!userDoc.Exists)
                    return NotFound($"User with UID '{uid}' not found.");

                string email = userDoc.GetValue<string>("email");
                string theme = userDoc.GetValue<string>("preferences.theme");
                string language = userDoc.GetValue<string>("preferences.language");
                string name = userDoc.GetValue<string>("name");
                string role = userDoc.GetValue<string>("userType");
                string unit = "";

                if (role == "Tenant")
                {
                    var leaseQuery = _firestore.Collection("leases").WhereEqualTo("tenantID", email);
                    var leaseSnapshot = await leaseQuery.GetSnapshotAsync();

                    if (leaseSnapshot.Count == 0)
                        return NotFound($"No lease found for user with email '{email}'.");

                    var leaseDoc = leaseSnapshot.Documents.First();
                    unit = leaseDoc.GetValue<string>("unitID");
                }

                var profileInfo = new ProfileModel
                {
                    FullName = name,
                    language = language,
                    theme = theme,
                    unitID = unit
                };
                return Ok(profileInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
