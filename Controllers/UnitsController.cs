using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class UnitsController : Controller
    {
        private readonly FirestoreDb _firestore;

        public UnitsController(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        // GET api/units/{propertyId}
        [HttpGet("{propertyId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUnits(string propertyId)
        {
            var snapshot = await _firestore.Collection("properties")
                                            .Document(propertyId)
                                            .Collection("units")
                                            .GetSnapshotAsync();

            var units = snapshot.Documents.Select(d => d.ToDictionary()).ToList();
            return Ok(units);
        }

        // GET api/units/{propertyId}/{unitId}
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
