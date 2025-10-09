using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Services;
using NewDawnPropertiesApi_V1.Models;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationsController : BaseFirestoreController<ApplicationModel>
    {
        public ApplicationsController(FirestoreService firestoreService)
            : base(firestoreService, "applications") { }
    }

}
