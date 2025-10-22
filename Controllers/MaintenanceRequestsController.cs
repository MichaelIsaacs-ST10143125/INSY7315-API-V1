using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;
using NewDawnPropertiesApi_V1.AppModels;
using System.Linq;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/all/[controller]")]
    public class MaintenanceRequestsController : BaseFirestoreController<MaintenanceRequestModel>
    {
        private readonly FirestoreDb _firestore;

        public MaintenanceRequestsController(FirestoreService firestoreService)
            : base(firestoreService, "maintenanceRequests")
        {
            _firestore = firestoreService.GetDb();
        }

        // GET api/maintenanceRequests/user/{uid}
        // Returns all the maintenance requests
        // The data returned is filtered based on the user's role:\
        [HttpGet("user/mobile/{uid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserRequests(string uid)
        {
            var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string userEmail = userDoc.GetValue<string>("email");
            string role = userDoc.GetValue<string>("userType");

            Query query = _firestore.Collection("maintenanceRequests");

            // Filter data per role
            if (role == "Tenant")
            {
                query = query.WhereEqualTo("tenantID", userEmail);
            }
            else if (role == "Caretaker")
            {
                query = query.WhereEqualTo("assignedTo", userEmail);
            }
            else if (role == "Manager")
            {
                // Get the worker's stationed location
                var workerStation = await _firestore
                    .Collection("Workers-Stationed")
                    .Document(uid)
                    .GetSnapshotAsync();

                string stationedPropertyID = workerStation.ContainsField("Stationed")
                    ? workerStation.GetValue<string>("Stationed")
                    : string.Empty;

                // Optional: fallback to userDoc.propertyID if needed
                string estatePropertyID = userDoc.ContainsField("propertyID")
                    ? userDoc.GetValue<string>("propertyID")
                    : string.Empty;

                // Decide which one to use for filtering
                string propertyFilter = !string.IsNullOrEmpty(stationedPropertyID)
                    ? stationedPropertyID
                    : estatePropertyID;

                // Apply the filter to the query
                query = query.WhereEqualTo("propertyID", propertyFilter);
            }

            else if (role == "Admin")
            {
                // Admin should not get data from this endpoint
                return Ok(new List<object>());
            }

            var snapshot = await query.GetSnapshotAsync();

            // Return only fields relevant to each role
            var results = snapshot.Documents.Select<DocumentSnapshot, object>(d =>
            {
                if (role == "Tenant")
                {
                    return new
                    {
                        Issue = d.GetValue<string>("description"),
                        Status = d.GetValue<string>("status"),
                        ReportDate = d.GetValue<DateTime?>("createdAt")
                    };
                }
                else if (role == "Caretaker")
                {
                    return new
                    {
                        ID = d.Id,
                        Unit = d.GetValue<string>("unitID"),
                        Status = d.GetValue<string>("status"),
                        FixDate = d.ContainsField("fixedAt") ? d.GetValue<DateTime?>("fixedAt") : null,
                        Issue = d.GetValue<string>("description")
                    };
                }
                else if (role == "Manager")
                {
                    return new
                    {
                        Unit = d.GetValue<string>("unitID"),
                        Status = d.GetValue<string>("status"),
                        ReportedDate = d.GetValue<DateTime?>("createdAt"),
                        Issue = d.GetValue<string>("description")
                    };
                }
                else
                {
                    return null;
                }
            }).Where(r => r != null).ToList();

            return Ok(results);
        }

        [HttpPost]
        [Route("add/mobile")]
        public async Task<ActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestModel model)
        {
            if (model == null)
                return BadRequest("Request body is empty.");

            // Fetch the user document
            var userDoc = await _firestore.Collection("users").Document(model.Uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{model.Uid}' not found.");

            string tenantEmail = userDoc.GetValue<string>("email");
            string located = userDoc.ContainsField("located")
                ? userDoc.GetValue<string>("located")
                : string.Empty;

            if (string.IsNullOrEmpty(located))
                return BadRequest("User's located field is missing or invalid.");

            // Split located field into propertyID and unitID
            var parts = located.Split(' ', 2); // Split into 2 parts only
            string propertyID = parts[0];
            string unitID = parts.Length > 1 ? parts[1] : string.Empty;

            // Find the caretaker responsible for this property
            var workerQuery = _firestore.Collection("Workers-Stationed")
                .WhereEqualTo("Stationed", propertyID)
                .WhereEqualTo("role", "Caretaker");

            var workerSnapshot = await workerQuery.GetSnapshotAsync();

            if (workerSnapshot.Documents.Count == 0)
                return NotFound($"No caretaker assigned to property '{located}'.");

            string assignedToEmail = workerSnapshot.Documents[0].GetValue<string>("WorkerID"); // assuming the caretaker doc has an 'email' field

            // Build the maintenance request
            var request = new MaintenanceRequestModel
                {
                    TenantID = tenantEmail,
                    PropertyID = propertyID,
                    UnitID = unitID,
                    Description = model.Description,
                    Urgency = model.Urgency,
                    Category = model.Category,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    AssignedTo = assignedToEmail // find the caretaker for the property
            };

            // Add the document to Firestore
            var docRef = await _firestore.Collection("maintenanceRequests").AddAsync(request);

            return CreatedAtAction(nameof(Get), new { id = docRef.Id }, request);
        }

    }
}
