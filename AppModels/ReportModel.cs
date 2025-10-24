namespace NewDawnPropertiesApi_V1.AppModels
{
    public class ReportModel
    {
        // Overall Summary
        public int? TotalProperties { get; set; }
        public int? TotalUsers { get; set; }
        public int? TotalWorkers { get; set; }
        public int? AdminWorkers { get; set; }
        public int? ManagerWorkers { get; set; }
        public int? CaretakerWorkers { get; set; }
        public int? ActiveLeases { get; set; }
        public int? InactiveLeases { get; set; }
        public int? AiEscalationsActive { get; set; }
        public int? AiEscalationsResolved { get; set; }

        // Property-level summaries
        public List<PropertySummary>? PropertySummaries { get; set; }
    }

    public class PropertySummary
    {
        public string? PropertyId { get; set; }
        public string? PropertyName { get; set; }

        public int? MaintenanceInProgress { get; set; }
        public int? MaintenanceCompleted { get; set; }

        public int? LeasesActive { get; set; }
        public int? LeasesInactive { get; set; }

        public int? ApplicationsPending { get; set; }
        public int? ApplicationsApproved { get; set; }
        public int? ApplicationsRejected { get; set; }

        public int? UnpaidInvoices { get; set; }
        public int? PaidInvoices { get; set; }

        public int? WorkersActive { get; set; }
        public int? WorkersInactive { get; set; }
        public int? Managers { get; set; }
        public int? Caretakers { get; set; }
    }
}
