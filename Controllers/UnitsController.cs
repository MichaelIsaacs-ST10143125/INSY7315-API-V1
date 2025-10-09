using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
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


        [HttpPut("{propertyId}/{unitId}")]
        public async Task<ActionResult> UpdateUnit(string propertyId, string unitId, [FromBody] UnitModel updatedUnit)
        {
            var docRef = _firestore
                .Collection("properties")
                .Document(propertyId)
                .Collection("units")
                .Document(unitId);

            var docSnapshot = await docRef.GetSnapshotAsync();

            if (!docSnapshot.Exists)
                return NotFound();

            await docRef.SetAsync(updatedUnit, SetOptions.Overwrite);

            return NoContent();
        }


    }
}
