using CoreArchV2.Core.Entity.Logistics;

namespace CoreArchV2.Dto.EReportDto
{
    public class RVehicleMaintenanceDto : RVehicleDto
    {
        public DateTime InvoiceDate { get; set; }
        public string InvoiceDate2 { get; set; }

        public List<MaintenanceType> MaintenanceTypeList { get; set; }
        public string AllMaintenanceTypeWithJoin { get; set; }

    }
}
