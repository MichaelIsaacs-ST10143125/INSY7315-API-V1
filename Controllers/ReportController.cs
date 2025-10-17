using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly FirestoreDb _firestore;

        public ReportController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("generate-report/mobile/{uid}")]
        public async Task<ActionResult<ReportModel>> GenerateReport(string uid)
        {
            // Validate user
            var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string role = userDoc.GetValue<string>("userType");
            if (role != "Admin")
                return Forbid("Only admins can generate reports.");

            // ------------------------------
            // 1. Global Overview
            // ------------------------------

            var propertiesSnapshot = await _firestore.Collection("properties").GetSnapshotAsync();
            var usersSnapshot = await _firestore.Collection("users").GetSnapshotAsync();
            var leasesSnapshot = await _firestore.Collection("leases").GetSnapshotAsync();

            int totalProperties = propertiesSnapshot.Count;
            int totalUsers = usersSnapshot.Count;

            int adminWorkers = usersSnapshot.Documents.Count(d => d.GetValue<string>("userType") == "Admin");
            int managerWorkers = usersSnapshot.Documents.Count(d => d.GetValue<string>("userType") == "Manager");
            int caretakerWorkers = usersSnapshot.Documents.Count(d => d.GetValue<string>("userType") == "Caretaker");
            int totalWorkers = adminWorkers + managerWorkers + caretakerWorkers;

            int activeLeases = leasesSnapshot.Documents
                .Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Active");

            int inactiveLeases = leasesSnapshot.Documents
                .Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Inactive");

            // AI Escalations (global)
            var escalationQuery = await _firestore.Collection("ai-escalations").GetSnapshotAsync();

            int escalationsActive = escalationQuery.Count(d => d.ContainsField("status") && d.GetValue<string>("status") != "Resolved");
            int escalationsResolved = escalationQuery.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Resolved");

            var report = new ReportModel
            {
                TotalProperties = totalProperties,
                TotalUsers = totalUsers,
                AdminWorkers = adminWorkers,
                ManagerWorkers = managerWorkers,
                CaretakerWorkers = caretakerWorkers,
                TotalWorkers = totalWorkers,
                ActiveLeases = activeLeases,
                InactiveLeases = inactiveLeases,
                AiEscalationsActive = escalationsActive,
                AiEscalationsResolved = escalationsResolved,
                PropertySummaries = new List<PropertySummary>()
            };

            // ------------------------------
            // Load all leases and invoices once for mapping
            // ------------------------------
            var leaseDict = leasesSnapshot.Documents
                .Where(d => d.ContainsField("propertyID"))
                .ToDictionary(
                    d => d.Id,
                    d => d.GetValue<string>("propertyID")
                );

            var invoicesSnapshot = await _firestore.Collection("invoices").GetSnapshotAsync();
            var invoices = invoicesSnapshot.Documents
                .Where(d => d.ContainsField("leaseID") && d.ContainsField("status"))
                .Select(d => new
                {
                    LeaseID = d.GetValue<string>("leaseID"),
                    Status = d.GetValue<string>("status")
                })
                .ToList();

            // ------------------------------
            // 2. Per-Property Operations Summary
            // ------------------------------
            foreach (var propertyDoc in propertiesSnapshot.Documents)
            {
                string propertyId = propertyDoc.Id;
                string propertyName = propertyDoc.ContainsField("name") ? propertyDoc.GetValue<string>("name") : "Unnamed Property";

                // Maintenance
                var maintenanceQuery = await _firestore.Collection("maintenanceRequests")
                    .WhereEqualTo("propertyID", propertyName)
                    .GetSnapshotAsync();

                int maintenanceInProgress = maintenanceQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") != "Completed");
                int maintenanceCompleted = maintenanceQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Completed");

                // Leases
                var leaseQuery = await _firestore.Collection("leases")
                    .WhereEqualTo("propertyID", propertyName)
                    .GetSnapshotAsync();

                int leaseActive = leaseQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Active");
                int leaseInactive = leaseQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Inactive");

                // Applications
                var appQuery = await _firestore.Collection("applications")
                    .WhereEqualTo("propertyID", propertyName)
                    .GetSnapshotAsync();

                int appPending = appQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Pending");
                int appApproved = appQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Approved");
                int appRejected = appQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Rejected");

                // ------------------------------
                // Invoices (via lease mapping)
                // ------------------------------
                var propertyInvoices = invoices
                    .Where(i => leaseDict.TryGetValue(i.LeaseID, out var propId) && propId == propertyName)
                    .ToList();

                int unpaidInvoices = propertyInvoices.Count(i => i.Status == "Overdue");
                int paidInvoices = propertyInvoices.Count(i => i.Status == "Paid");

                // ------------------------------
                // Workers
                // ------------------------------
                var workerQuery = await _firestore.Collection("Workers-Stationed")
                    .WhereEqualTo("Stationed", propertyName)
                    .GetSnapshotAsync();

                int activeWorkers = workerQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Active");
                int inactiveWorkers = workerQuery.Documents.Count(d => d.ContainsField("status") && d.GetValue<string>("status") == "Inactive");
                int managers = workerQuery.Documents.Count(d => d.ContainsField("role") && d.GetValue<string>("role") == "Manager");
                int caretakers = workerQuery.Documents.Count(d => d.ContainsField("role") && d.GetValue<string>("role") == "Caretaker");

                var propertySummary = new PropertySummary
                {
                    PropertyId = propertyId,
                    PropertyName = propertyName,
                    MaintenanceInProgress = maintenanceInProgress,
                    MaintenanceCompleted = maintenanceCompleted,
                    LeasesActive = leaseActive,
                    LeasesInactive = leaseInactive,
                    ApplicationsPending = appPending,
                    ApplicationsApproved = appApproved,
                    ApplicationsRejected = appRejected,
                    UnpaidInvoices = unpaidInvoices,
                    PaidInvoices = paidInvoices,
                    WorkersActive = activeWorkers,
                    WorkersInactive = inactiveWorkers,
                    Managers = managers,
                    Caretakers = caretakers
                };

                report.PropertySummaries.Add(propertySummary);
            }

            return Ok(report);
        }
    }
}
