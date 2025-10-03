using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : BaseFirestoreController<UserModel>
    {
        public UsersController(FirestoreDb firestore) : base(firestore, "users") { }
    }

}
