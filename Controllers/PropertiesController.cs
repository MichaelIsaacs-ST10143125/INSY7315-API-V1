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
                    ? ((List<object>)propertyData["amenities"]).Select(a => a.ToString()).ToArray()
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
                    ? ((List<object>)propertyData["amenities"]).Select(a => a.ToString()).ToArray()
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
