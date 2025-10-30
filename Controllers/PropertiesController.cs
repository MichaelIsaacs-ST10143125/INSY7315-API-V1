using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.AppModels;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;
// Keep This Controller

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class PropertiesController : BaseFirestoreController<PropertyModel>
    {
        private readonly FirestoreDb _firestore;

        public PropertiesController(FirestoreService firestoreService)
            : base(firestoreService, "invoices")
        {
            _firestore = firestoreService.GetDb();
        }

        [HttpGet("user/mobile/tenant/guest")]
        public async Task<ActionResult<IEnumerable<PropertyMobileModel>>> GetPropertyUnits()
        {
            var result = new List<PropertyMobileModel>();

            // Get all properties
            var propertiesSnapshot = await _firestore.Collection("properties").GetSnapshotAsync();

            foreach (var propertyDoc in propertiesSnapshot.Documents)
            {
                if (!propertyDoc.Exists) continue;

                var propertyData = propertyDoc.ToDictionary();
                var propertyName = propertyData.ContainsKey("name") ? propertyData["name"]?.ToString() : null;
                var propertyAddress = propertyData.ContainsKey("address") ? propertyData["address"]?.ToString() : null;
                var amenities = propertyData.ContainsKey("amenities")
                    ? ((List<object>)propertyData["amenities"]).Select(a => a.ToString() ?? string.Empty)
                    .ToArray()
                    : Array.Empty<string>();

                // Get units subcollection
                var unitsCollection = propertyDoc.Reference.Collection("units");
                var unitsSnapshot = await unitsCollection.WhereEqualTo("isAvailable", true).GetSnapshotAsync();

                foreach (var unitDoc in unitsSnapshot.Documents)
                {
                    if (!unitDoc.Exists) continue;

                    var unitData = unitDoc.ToDictionary();
                    var unitNumber = unitData.ContainsKey("unitNumber") ? unitData["unitNumber"]?.ToString() : null;

                    var mobileModel = new PropertyMobileModel
                    {
                        unitNumber = unitNumber,
                        propertyName = propertyName,
                        propertyAddress = propertyAddress,
                        amenities = amenities
                    };

                    result.Add(mobileModel);
                }
            }

            return Ok(result);
        }

        [HttpGet("user/mobile/manager")]
        public async Task<ActionResult<IEnumerable<PropertyMobileModel>>> GetAllPropertyUnits()
        {
            var result = new List<PropertyMobileModel>();

            // Get all properties
            var propertiesSnapshot = await _firestore.Collection("properties").GetSnapshotAsync();

            foreach (var propertyDoc in propertiesSnapshot.Documents)
            {
                if (!propertyDoc.Exists) continue;

                var propertyData = propertyDoc.ToDictionary();
                var propertyName = propertyData.ContainsKey("name") ? propertyData["name"]?.ToString() : null;
                var propertyAddress = propertyData.ContainsKey("address") ? propertyData["address"]?.ToString() : null;
                var amenities = propertyData.ContainsKey("amenities")
                    ? ((List<object>)propertyData["amenities"]).Select(a => a.ToString() ?? string.Empty).ToArray()
                    : Array.Empty<string>();

                // Get units subcollection
                var unitsCollection = propertyDoc.Reference.Collection("units");
                var unitsSnapshot = await unitsCollection.GetSnapshotAsync();

                foreach (var unitDoc in unitsSnapshot.Documents)
                {
                    if (!unitDoc.Exists) continue;

                    var unitData = unitDoc.ToDictionary();
                    var unitNumber = unitData.ContainsKey("unitNumber") ? unitData["unitNumber"]?.ToString() : null;
                    var isAvailable = unitData.ContainsKey("isAvailable") ? (bool?)unitData["isAvailable"] : null;

                    var mobileModel = new PropertyMobileModel
                    {
                        isAvailable = isAvailable,
                        unitNumber = unitNumber,
                        propertyName = propertyName,
                        propertyAddress = propertyAddress,
                        amenities = amenities
                    };

                    result.Add(mobileModel);
                }
            }

            return Ok(result);
        }

        [HttpGet("manager/mobile/units/{uid}")]
        public async Task<ActionResult<IEnumerable<PropertyMobileModel>>> GetAllPropertyUnitsForManager(string uid)
        {
            // Step 1: Verify user exists and is a Manager
            var userDoc = await _firestore.Collection("users").Document(uid).GetSnapshotAsync();
            if (!userDoc.Exists)
                return NotFound($"User with UID '{uid}' not found.");

            var role = userDoc.GetValue<string>("userType");
            if (role != "Manager")
                return Forbid("Only managers can access this endpoint.");

            // Step 2: Get manager’s stationed property
            var stationedDoc = await _firestore.Collection("Workers-Stationed").Document(uid).GetSnapshotAsync();
            if (!stationedDoc.Exists)
                return NotFound($"Worker station for UID '{uid}' not found.");

            var stationedPropertyID = stationedDoc.GetValue<string>("Stationed");

            // Step 3: Get property document
            var propertyDocRef = _firestore.Collection("properties").Document(stationedPropertyID);
            var propertySnapshot = await propertyDocRef.GetSnapshotAsync();

            if (!propertySnapshot.Exists)
                return NotFound($"Property '{stationedPropertyID}' not found.");

            // Extract property-level fields
            var propertyData = propertySnapshot.ToDictionary();
            var propertyName = propertyData.ContainsKey("name") ? propertyData["name"]?.ToString() : null;
            var propertyAddress = propertyData.ContainsKey("address") ? propertyData["address"]?.ToString() : null;
            var amenities = propertyData.ContainsKey("amenities")
                ? ((List<object>)propertyData["amenities"]).Select(a => a.ToString()).ToArray()
                : Array.Empty<string>();

            // Step 4: Get all units under the property
            var unitsSnapshot = await propertyDocRef.Collection("units").GetSnapshotAsync();

            var result = new List<PropertyMobileModel>();
            foreach (var unitDoc in unitsSnapshot.Documents)
            {
                if (!unitDoc.Exists) continue;

                var unitData = unitDoc.ToDictionary();
                var unitNumber = unitData.ContainsKey("unitNumber") ? unitData["unitNumber"]?.ToString() : null;
                var isAvailable = unitData.ContainsKey("isAvailable") ? Convert.ToBoolean(unitData["isAvailable"]) : (bool?)null;

                // Step 5: Combine property + unit data into one model
                result.Add(new PropertyMobileModel
                {
                    isAvailable = isAvailable,
                    unitNumber = unitNumber,
                    propertyName = propertyName,
                    propertyAddress = propertyAddress,
                    amenities = amenities
                });
            }

            return Ok(result);
        }

        [HttpPut("manager/update/unit/{propertyId}/{unitId}")]
        public async Task<ActionResult> UpdateUnitAvailability(string propertyId, string unitId, [FromBody] UpdatePropertyListing updateModel)
        {
            if (updateModel == null)
                return BadRequest("Update data is required.");

            var propertyDocRef = _firestore.Collection("properties").Document(propertyId);
            var unitDocRef = propertyDocRef.Collection("units").Document(unitId);

            var unitSnapshot = await unitDocRef.GetSnapshotAsync();
            if (!unitSnapshot.Exists)
                return NotFound($"Unit with ID '{unitId}' not found in property '{propertyId}'.");

            var updates = new Dictionary<string, object>
            {
                { "isAvailable", updateModel.isAvailable }
            };
            await unitDocRef.UpdateAsync(updates);
            return Ok("Unit availability updated successfully.");
        }

    }
}
