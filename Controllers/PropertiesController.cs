using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;
// Keep This Controller

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class PropertiesController : BaseFirestoreController<PropertyModel>
    {
        public PropertiesController(FirestoreService firestoreService)
            : base(firestoreService, "properties") { }
    }
}
