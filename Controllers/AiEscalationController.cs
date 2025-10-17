using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [ApiController]
    [Route("api/ai/escalation/[controller]")]
    public class AiEscalationController : Controller
    {
        private readonly FirestoreDb _firestore;

        public AiEscalationController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("mobile/{uid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetEscalations(string uid)
        {
            try
            {
                var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
                if (!userDoc.Exists)
                    return NotFound($"User with UID '{uid}' not found.");

                string role = userDoc.GetValue<string>("userType");
                if (role != "Manager" && role != "Admin")
                    return Forbid("Only managers and admins can access this endpoint.");

                var escalationQuery = await _firestore.Collection("ai-escalations").GetSnapshotAsync();

                var escalations = escalationQuery.Documents
                    .Select(d => new AiEscalationsModel
                    {
                        id = "ID of the document",
                        chatID = d.GetValue<string>("chatID"),
                        reason = d.GetValue<string>("escalationReason"),
                        issue = d.GetValue<string>("issue"),
                        createdAt = d.GetValue<DateTime>("createdAt"),
                        status = d.GetValue<string>("status"),
                        urgency = d.GetValue<string>("urgency"),
                        user = d.GetValue<string>("user"),
                        category = d.GetValue<string>("category"),
                    }).ToList();

                return Ok(escalations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
