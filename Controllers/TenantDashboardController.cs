using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [ApiController]
    [Route("api/mobile/[controller]")]
    public class TenantDashboardController : Controller
    {
        private readonly FirestoreDb _firestore;

        public TenantDashboardController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("{uid}")]
        public async Task<ActionResult<TenantDashboardModel>> GetDashboardSummary(string uid)
        {
            try
            {
                // Step 1: Get user document by UID
                var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
                if (!userDoc.Exists)
                    return NotFound($"User with UID '{uid}' not found.");

                string userEmail = userDoc.GetValue<string>("email");

                // Step 2: Get the latest invoice linked to this user (tenantID == userEmail)
                var invoicesRef = _firestore.Collection("invoices");
                var invoicesQuery = invoicesRef
                    .WhereEqualTo("tenantID", userEmail)
                    .OrderByDescending("createdAt")
                    .Limit(1);

                var invoicesSnapshot = await invoicesQuery.GetSnapshotAsync();
                var latestInvoiceDoc = invoicesSnapshot.Documents.FirstOrDefault();

                string? latestInvoiceStatus = null;
                DateTime? latestInvoiceDueDate = null;
                decimal? latestInvoiceAmount = null;

                if (latestInvoiceDoc != null)
                {
                    latestInvoiceStatus = latestInvoiceDoc.GetValue<string>("status");
                    latestInvoiceAmount = Convert.ToDecimal(latestInvoiceDoc.GetValue<double>("amount"));
                    latestInvoiceDueDate = latestInvoiceDoc.GetValue<Timestamp>("dueDate").ToDateTime();
                }

                // Step 3: Count maintenance requests that are "Active" or "In Progress"
                var maintenanceRef = _firestore.Collection("maintenanceRequests");
                var maintenanceQuery = maintenanceRef
                    .WhereEqualTo("tenantID", userEmail)
                    .WhereIn("status", new[] { "Active", "In Progress" });

                var maintenanceSnapshot = await maintenanceQuery.GetSnapshotAsync();
                int activeMaintenanceCount = maintenanceSnapshot.Count;

                // Step 4: Build response
                var summary = new TenantDashboardModel
                {
                    LatestInvoiceStatus = latestInvoiceStatus,
                    LatestInvoiceDueDate = latestInvoiceDueDate,
                    LatestInvoiceAmount = latestInvoiceAmount,
                    ActiveMaintenanceCount = activeMaintenanceCount
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching dashboard data: {ex.Message}");
            }
        }
    }
}
