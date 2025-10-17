using Google.Cloud.Firestore;

namespace NewDawnPropertiesApi_V1.AppModels
{
    public class UpdateUnitAndLeaseModel
    {
        // Unit fields
        //public bool? IsAvailable { get; set; }
        public int? RentAmount { get; set; }
        public string? TenantID { get; set; }

        // Lease fields
        public int? Deposit { get; set; }
        //public string? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
