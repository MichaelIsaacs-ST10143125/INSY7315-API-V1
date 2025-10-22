using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class LeasesController : BaseFirestoreController<LeaseModel>
    {
        private readonly FirestoreDb _firestore;

        public LeasesController(FirestoreService firestoreService)
            : base(firestoreService, "leases")
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("manager/mobile/{uid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserLeases(string uid)
        {
            // Get user document
            var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string userEmail = userDoc.GetValue<string>("email");
            string role = userDoc.GetValue<string>("userType");

            // Only managers should see leases
            if (role != "Manager")
                return Forbid("Only managers can view leases.");

            // Get the worker's stationed location
            var workerStation = await _firestore
                .Collection("Workers-Stationed")
                .Document(uid)
                .GetSnapshotAsync();

            string stationedPropertyID = workerStation.GetValue<string>("Stationed");

            // Query leases for this tenant
            var query = _firestore.Collection("leases")
                .WhereEqualTo("propertyID", stationedPropertyID);

            var snapshot = await query.GetSnapshotAsync();

            // Map to only the needed fields
            var results = snapshot.Documents.Select(d => new
            {
                LeaseID = d.Id,
                PropertyID = d.GetValue<string>("propertyID"),
                UnitID = d.GetValue<string>("unitID"),  
                TenantID = d.GetValue<string>("tenantID"),
                StartDate = d.ContainsField("startDate")
                    ? d.GetValue<DateTime?>("startDate")
                    : null,
                EndDate = d.ContainsField("endDate")
                    ? d.GetValue<DateTime?>("endDate")
                    : null,
                RentAmount = d.GetValue<int?>("rentAmount"),
                Deposit = d.GetValue<int>("deposit"),
                Status = d.GetValue<string>("status")
            }).ToList();
            return Ok(results);
        }

    }
}

