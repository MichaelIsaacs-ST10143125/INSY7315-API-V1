using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class PropertiesController : BaseFirestoreController<PropertyModel>
    {
        public PropertiesController(FirestoreDb firestore) : base(firestore, "Properties") { }
    }
}
