using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [ApiController]
    [Route("api/admin/mobile/[controller]")]
    public class AdminDashboardController : Controller
    {
        private readonly FirestoreDb _firestore;

        public AdminDashboardController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("{uid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDashBoardSummary(string uid)
        {
            try
            {
                var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();

                if (!userDoc.Exists)
                    return NotFound($"User with UID '{uid}' not found.");

                string role = userDoc.GetValue<string>("userType");

                // Only admins should access this endpoint
                if (role != "Admin")
                    return Forbid("Only admins can access this endpoint.");

                // Get Tenants Count
                var tenantsSnapshot = await _firestore.Collection("users")
                    .GetSnapshotAsync();

                int tenantsCount = tenantsSnapshot.Count(d => d.GetValue<string>("userType") == "Tenant");

                // Get Property Count
                var propertiesSnapshot = await _firestore.Collection("properties").GetSnapshotAsync();

                int propertiesCount = propertiesSnapshot.Count;

                // Get Occupancy Rate
                var leasesSnapshot = await _firestore.Collection("leases")
                    .GetSnapshotAsync();

                int activeLeasesCount = leasesSnapshot.Count(d => d.GetValue<string>("status") == "Active");
                int inativeLeasesCount = leasesSnapshot.Count(d => d.GetValue<string>("status") == "Inactive");

                string occupany = $"{activeLeasesCount} / {activeLeasesCount + inativeLeasesCount}";

                // Rent Collected This Month & Outstanding Rent
                var paymentsSnapshot = await _firestore.Collection("invoices")
                    .GetSnapshotAsync();

                int rentCollectedThisMonth = 0;
                int outstandingRent = 0;

                foreach (var doc in paymentsSnapshot.Documents)
                {
                    string status = doc.GetValue<string>("status");

                    if (status == "Paid")
                    {
                        Timestamp paidDate = doc.GetValue<Timestamp>("dueDate");
                        DateTime paidDateTime = paidDate.ToDateTime();
                        if (paidDateTime.Month == DateTime.UtcNow.Month &&
                            paidDateTime.Year == DateTime.UtcNow.Year)
                        {
                            rentCollectedThisMonth += doc.GetValue<int>("amount");
                        }
                    }
                    else if (status == "Overdue")
                    {
                        outstandingRent += doc.GetValue<int>("amount");
                    }
                }

                // Maintenance Requests
                var maintenanceSnapshot = await _firestore.Collection("maintenanceRequests")
                    .GetSnapshotAsync();

                int maintenanceRequestsCount = maintenanceSnapshot.Count(d => d.GetValue<string>("status") != "Completed");

                var summary = new AdminDashboardModel
                {
                    tenants = tenantsCount,
                    properties = propertiesCount,
                    occupancyRate = occupany,
                    rentCollected = $"R {rentCollectedThisMonth}",
                    outstandingRent = $"R {outstandingRent}",
                    maintenanceRequests = maintenanceRequestsCount
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
