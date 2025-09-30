using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseFirestoreController<T> : ControllerBase where T : class
    {
        private readonly FirestoreDb _firestore;
        private readonly string _collection;

        public BaseFirestoreController(FirestoreDb firestore, string collection)
        {
            _firestore = firestore;
            _collection = collection;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> GetAll()
        {
            var snapshot = await _firestore.Collection(_collection).GetSnapshotAsync();
            return Ok(snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Get(string id)
        {
            var doc = await _firestore.Collection(_collection).Document(id).GetSnapshotAsync();
            if (!doc.Exists) return NotFound();
            return Ok(doc.ConvertTo<T>());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] T item)
        {
            var docRef = await _firestore.Collection(_collection).AddAsync(item);
            return CreatedAtAction(nameof(Get), new { id = docRef.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] T item)
        {
            var docRef = _firestore.Collection(_collection).Document(id);
            await docRef.SetAsync(item, SetOptions.Overwrite);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            await _firestore.Collection(_collection).Document(id).DeleteAsync();
            return NoContent();
        }
    }
}
