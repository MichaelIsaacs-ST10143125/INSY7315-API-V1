using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Services;

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
        public async Task<ActionResult<IEnumerable<object>>> GetUnits(string propertyId)
        {
            var snapshot = await _firestore.Collection("properties")
                                           .Document(propertyId)
                                           .Collection("units")
                                           .GetSnapshotAsync();

            return Ok(snapshot.Documents.Select(d => d.ToDictionary()).ToList());
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
    }
}
