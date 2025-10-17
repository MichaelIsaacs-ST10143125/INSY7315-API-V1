using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [ApiController]
    [Route("api/manager/mobile/[controller]")]
    public class ManagerdashboardController : Controller
    {
        private readonly FirestoreDb _firestore;

        public ManagerdashboardController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("{uid}")]
        public async Task<ActionResult<ManagerDashboardModel>> GetDashboardSummary(string uid)
        {
            try
            {
                // Get user document by the uid
                var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
                if (!userDoc.Exists)
                    return NotFound($"User with UID '{uid}' not found.");

                string userEmail = userDoc.GetValue<string>("email");
                string role = userDoc.GetValue<string>("userType");

                // Only managers should access this endpoint
                if (role != "Manager")
                    return Forbid("Only managers can access this endpoint.");

                // Get the worker's stationed location
                var workerStation = await _firestore
                    .Collection("Workers-Stationed")
                    .Document(uid)
                    .GetSnapshotAsync();
                string stationedPropertyID = workerStation.ContainsField("Station")
                    ? workerStation.GetValue<string>("Station")
                    : string.Empty;


                // Get Tenants Count
                var propertyDoc = await _firestore.Collection("properties").Document(stationedPropertyID).GetSnapshotAsync();

                if (!propertyDoc.Exists)
                    return NotFound($"Property with ID '{stationedPropertyID}' not found.");

                var leaseDoc = await _firestore.Collection("leases")
                    .WhereEqualTo("propertyID", stationedPropertyID)
                    .GetSnapshotAsync();


                // This is how many leases are active
                int activeLeasesCount = leaseDoc.Count(d => d.GetValue<string>("status") == "Active");


                // Get Occupany Rate
                // This is how many leases are inactive
                int inactiveLeasesCount = leaseDoc.Count(d => d.GetValue<string>("status") == "Inactive");


                // Get leases count that are expiring within 2 months
                int expiringLeasesCount = leaseDoc.Count(d =>
                {
                    if (d.ContainsField("endDate"))
                    {
                        DateTime endDate = d.GetValue<DateTime>("endDate");
                        return endDate > DateTime.Now && endDate <= DateTime.Now.AddMonths(2);
                    }
                    return false;
                });

                // 1. Get leases for the property
                var leaseSnapshot = await _firestore.Collection("leases")
                    .WhereEqualTo("propertyID", stationedPropertyID)
                    .GetSnapshotAsync();

                // 2. Get all tenantIDs (non-empty and distinct)
                var tenants = leaseSnapshot.Documents
                    .Where(d => d.ContainsField("tenantID"))
                    .Select(d => d.GetValue<string>("tenantID"))
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                Console.WriteLine($"Tenants found: {string.Join(", ", tenants)}");

                int totalCollected = 0;
                int totalOverdue = 0;

                DateTime now = DateTime.Now;

                // Previous month
                DateTime firstDayPreviousMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                DateTime firstDayCurrentMonth = new DateTime(now.Year, now.Month, 1);

                // Previous 2 months
                DateTime firstDayTwoMonthsAgo = firstDayPreviousMonth.AddMonths(-1);


                // 3. Batch Firestore queries (10 tenants per batch)
                const int batchSize = 10;
                for (int i = 0; i < tenants.Count; i += batchSize)
                {
                    var tenantBatch = tenants.Skip(i).Take(batchSize).ToList();

                    var invoicesSnapshot = await _firestore.Collection("invoices")
                        .WhereIn("tenantID", tenantBatch)
                        .GetSnapshotAsync();

                    Console.WriteLine($"Invoices found in batch: {invoicesSnapshot.Count}");

                    // 4. Calculate rent collected and overdue
                    foreach (var doc in invoicesSnapshot.Documents)
                    {
                        if (!doc.ContainsField("status") || !doc.ContainsField("amount") || !doc.ContainsField("createdAt"))
                            continue;

                        string status = doc.GetValue<string>("status");
                        int amount = doc.GetValue<int>("amount");
                        DateTime createdAt = doc.GetValue<DateTime>("createdAt");

                        // Rent collected: Paid invoices in previous month
                        if (status == "Paid" && createdAt >= firstDayPreviousMonth && createdAt < firstDayCurrentMonth)
                            totalCollected += amount;

                        // Rent overdue: any non-Paid invoice in last 2 months
                        if (status != "Paid" && createdAt >= firstDayTwoMonthsAgo && createdAt < firstDayCurrentMonth)
                            totalOverdue += amount;
                    }

                }

                Console.WriteLine($"Total Collected: {totalCollected}, Total Overdue: {totalOverdue}");


                // Get Rent pending this month
                //Completed above

                // Get Active Maintenance Requests count for this property
                var maintenanceSnapshot = await _firestore.Collection("maintenanceRequests")
                    .WhereEqualTo("propertyID", stationedPropertyID)
                    .GetSnapshotAsync();

                int activeOrPendingCount = maintenanceSnapshot.Documents
                    .Count(d => d.ContainsField("status") && d.GetValue<string>("status") != "Completed");


                // Put it all together and send it back
                var summary = new ManagerDashboardModel
                {
                    tenantCount = activeLeasesCount,
                    occupancyRate = $"{activeLeasesCount} / {inactiveLeasesCount + activeLeasesCount}",
                    leasesExpiringSoon = expiringLeasesCount,
                    totalRentCollectedThisMonth = totalCollected,
                    totalOutstandingRent = totalOverdue,
                    maintenanceRequestsCount = activeOrPendingCount
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
