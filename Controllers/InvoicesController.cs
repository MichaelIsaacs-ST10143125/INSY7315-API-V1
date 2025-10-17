using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class InvoicesController : BaseFirestoreController<InvoiceModel>
    {
        private readonly FirestoreDb _firestore;

        public InvoicesController(FirestoreService firestoreService)
            : base(firestoreService, "invoices")
        {
            _firestore = firestoreService.GetDb();
        }

        // GET api/invoices/user/{uid}
        // Returns all the invoices linked to the user (Tenant)
        [HttpGet("user/mobile/{uid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserInvoices(string uid)
        {
            // 1️⃣ Get user document
            var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            string userEmail = userDoc.GetValue<string>("email");
            string role = userDoc.GetValue<string>("userType");

            // 2️⃣ Only tenants should see invoices
            if (role != "Tenant")
                return Forbid("Only tenants can view invoices.");

            // 3️⃣ Query invoices for this tenant
            var query = _firestore.Collection("invoices")
                .WhereEqualTo("tenantID", userEmail);

            var snapshot = await query.GetSnapshotAsync();

            // 4️⃣ Map to only the needed fields
            var results = snapshot.Documents.Select(d => new
            {
                Amount = d.GetValue<double?>("amount"),
                Status = d.GetValue<string>("status"),
                PaymentDate = d.ContainsField("dueDate")
                    ? d.GetValue<DateTime?>("dueDate")
                    : null
            }).ToList();

            return Ok(results);
        }

        [HttpPost("api/user/mobile/add-payment")]
        public async Task<ActionResult> AddPayment([FromBody] AddInvoiceModel invoice)
        {
            if (invoice == null)
                return BadRequest("Invoice data is required.");

            var userDoc = await _firestore.Collection("users").Document(invoice.uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{invoice.uid}' not found.");
            string userEmail = userDoc.GetValue<string>("email");

            var leaseDoc = await _firestore.Collection("leases").WhereEqualTo("tenantID", userEmail).GetSnapshotAsync();
            string leaseId = leaseDoc.Documents.FirstOrDefault()?.Id ?? string.Empty;

            var newInvoice = new InvoiceModel
            {
                TenantID = userEmail,
                LeaseID = leaseId,
                Amount = invoice.amount,
                DueDate = DateTime.UtcNow,
                Status = "Paid",
                CreatedAt = DateTime.UtcNow
            };

            await _firestore.Collection("invoices").AddAsync(newInvoice);
            return CreatedAtAction(nameof(Get), new { id = newInvoice.TenantID }, newInvoice);
        }
    }
}
