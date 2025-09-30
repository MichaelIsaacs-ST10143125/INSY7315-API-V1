using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class CaretakerAssignmentsController : BaseFirestoreController<CaretakerAssignmentModel>
    {
        public CaretakerAssignmentsController(FirestoreDb firestore) : base(firestore, "CaretakerAssignments") { }
    }
}
