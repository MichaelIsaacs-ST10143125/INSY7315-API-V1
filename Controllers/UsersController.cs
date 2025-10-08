using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : BaseFirestoreController<UserModel>
    {
        public UsersController(FirestoreService firestoreService)
            : base(firestoreService, "users") { }
    }
}
