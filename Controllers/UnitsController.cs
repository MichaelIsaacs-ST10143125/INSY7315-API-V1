using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;
// Keep This Controller

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly FirestoreDb _firestore;

        public UnitsController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("{propertyId}")]
        public async Task<ActionResult<IEnumerable<UnitModel>>> GetUnits(string propertyId)
        {
            // Retrieve the units collection snapshot
            var snapshot = await _firestore
                .Collection("properties")
                .Document(propertyId)
                .Collection("units")
                .GetSnapshotAsync();

            var units = snapshot.Documents
                .Select(d => d.ConvertTo<UnitModel>())
                .ToList();

            return Ok(units);
        }


        [HttpGet("{propertyId}/{unitId}")]
        public async Task<ActionResult<object>> GetUnit(string propertyId, string unitId)
        {
            var doc = await _firestore.Collection("properties")
                                       .Document(propertyId)
                                       .Collection("units")
                                       .Document(unitId)
                                       .GetSnapshotAsync();

            if (!doc.Exists) return NotFound();
            return Ok(doc.ToDictionary());
        }


        [HttpPut("add/tenant/mobile/{propertyName}/{unitId}")]
        public async Task<ActionResult> UpdateUnitAndLease(
            string propertyName,
            string unitId,
            [FromBody] UpdateUnitAndLeaseModel request)
        {
            var propertyDoc = await _firestore.Collection("properties")
                .WhereEqualTo("name", propertyName)
                .GetSnapshotAsync();

            var propertyId = propertyDoc.Documents[0].Id;

            var unitRef = _firestore
                .Collection("properties")
                .Document(propertyId)
                .Collection("units")
                .Document(unitId);

            var unitSnapshot = await unitRef.GetSnapshotAsync();
            if (!unitSnapshot.Exists)
                return NotFound("Unit not found");

            // Get the lease ID from the unit document
            string? leaseId = unitSnapshot.ContainsField("leaseID")
                ? unitSnapshot.GetValue<string>("leaseID")
                : null;

            if (string.IsNullOrEmpty(leaseId))
                return BadRequest("Unit does not have an associated leaseID");

            // Prepare unit updates
            var unitUpdates = new Dictionary<string, object?>
            {
                { "isAvailable", false },
                { "rentAmount", request.RentAmount },
                { "tenantID", request.TenantID }
            };

            // Apply updates to the unit document
            await unitRef.SetAsync(unitUpdates, SetOptions.MergeAll);

            // Get lease document reference
            var leaseRef = _firestore
                .Collection("leases")
                .Document(leaseId);

            // Prepare lease updates
            var leaseUpdates = new Dictionary<string, object?>
            {
                { "deposit", request.Deposit },
                { "rentAmount", request.RentAmount },
                { "status", "Active" },
                { "tenantID", request.TenantID },
                { "startDate", request.StartDate },
                { "endDate", request.EndDate }
            };

            // Apply updates to the lease document
            await leaseRef.SetAsync(leaseUpdates, SetOptions.MergeAll);

            return NoContent();
        }

        [HttpPut("remove/tenant/mobile/{propertyName}/{unitId}")]
        public async Task<ActionResult> RemoveTenantUnitAndLease(
            string propertyName,
            string unitId)
        {
            var propertyDoc = await _firestore.Collection("properties")
                .WhereEqualTo("name", propertyName)
                .GetSnapshotAsync();

            var propertyId = propertyDoc.Documents[0].Id;

            var unitRef = _firestore
                .Collection("properties")
                .Document(propertyId)
                .Collection("units")
                .Document(unitId);

            var unitSnapshot = await unitRef.GetSnapshotAsync();
            if (!unitSnapshot.Exists)
                return NotFound("Unit not found");

            // Get the lease ID from the unit document
            string? leaseId = unitSnapshot.ContainsField("leaseID")
                ? unitSnapshot.GetValue<string>("leaseID")
                : null;

            if (string.IsNullOrEmpty(leaseId))
                return BadRequest("Unit does not have an associated leaseID");

            // Prepare unit updates
            var unitUpdates = new Dictionary<string, object?>
            {
                { "isAvailable", true },
                { "rentAmount", 0 },
                { "tenantID", "" }
            };

            // Apply updates to the unit document
            await unitRef.SetAsync(unitUpdates, SetOptions.MergeAll);

            // Get lease document reference
            var leaseRef = _firestore
                .Collection("leases")
                .Document(leaseId);

            // Prepare lease updates
            var leaseUpdates = new Dictionary<string, object?>
            {
                { "deposit", 0 },
                { "rentAmount", 0 },
                { "status", "Inactive" },
                { "tenantID", "" },
                { "startDate", DateTime.UtcNow },
                { "endDate", DateTime.UtcNow }
            };

            // Apply updates to the lease document
            await leaseRef.SetAsync(leaseUpdates, SetOptions.MergeAll);

            return NoContent();
        }
    }
}
