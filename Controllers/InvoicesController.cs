using Microsoft.AspNetCore.Mvc;
using NewDawnPropertiesApi_V1.Models;
using NewDawnPropertiesApi_V1.Services;

namespace NewDawnPropertiesApi_V1.Controllers
{
    // Controllers/InvoicesController.cs
    [Route("api/[controller]")]
    public class InvoicesController : BaseFirestoreController<InvoiceModel>
    {
        public InvoicesController(FirestoreService firestoreService)
            : base(firestoreService, "invoices") { }
    }

}
