using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationsController : BaseFirestoreController<ApplicationModel>
    {
        private readonly FirestoreDb _firestore;

        public ApplicationsController(FirestoreService firestoreService)
            : base(firestoreService, "applications")
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpPost("mobile/add/application")]
        public async Task<ActionResult> AddApplication([FromBody] AddApplicationModel application)
        {
            if (application == null)
                return BadRequest("Application data is required.");

            try
            {
                // Create a new document with auto-generated ID
                var docRef = _firestore.Collection("applications").Document();

                var propertyDoc = await _firestore.Collection("properties")
                .WhereEqualTo("name", application.propertyName)
                .GetSnapshotAsync();

                var propertyId = propertyDoc.Documents[0].Id;

                // Prepare the data to save
                var applicationData = new Dictionary<string, object>
                {
                    { "email", application.email },
                    { "name", application.name },
                    { "phone", application.phone },
                    { "propertyID", propertyId },
                    { "unitID", application.unitID },
                    { "status", "Pending" },
                    { "submittedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
                };

                // Save the document in Firestore
                await docRef.SetAsync(applicationData);

                return Ok(new { message = "Application submitted successfully.", id = docRef.Id });
            }
            catch (Exception ex)
            {
                // Handle any errors
                return StatusCode(500, $"Error saving application: {ex.Message}");
            }
        }

    }

}
