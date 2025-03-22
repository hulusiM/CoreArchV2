using CoreArchV2.Dto.ELogisticsDto;

namespace CoreArchV2.Dto.EReportDto
{
    public class RDashboardDto
    {
        //Araç rapor sayfası => Header
        public int AllVehicle { get; set; }
        public int DebitVehicleUser { get; set; }
        public List<EVehicleDto> EmptyVehicle { get; set; }
        public List<EVehicleDto> InPoolVehicle { get; set; }
        public List<EVehicleDto> RentVehicle { get; set; }
        public List<EVehicleDto> FixVehicle { get; set; }
        public List<EVehicleDto> ServiceInVehicle { get; set; }

        public string DateMonth { get; set; }

        //Son zimmetlenenler
        public int DebitUserId { get; set; }
        public int CreatedUserId { get; set; }
        public int VehicleDebitId { get; set; }
        public string Plate { get; set; }
        public string DebitNameSurname { get; set; }
        public DateTime DebitCreatedDate { get; set; }
        public string DebitCreatedUserNameSurname { get; set; }

        //Araç yakıt sayfası => Header
        public decimal TotalAmount { get; set; }
        public decimal? TotalKm { get; set; }
        public decimal? TotalLiter { get; set; }
        public decimal DiscountTotalAmount { get; set; }
        public EFuelLogDto MostPlateAmount { get; set; }
        public EFuelLogDto MostProjectAmount { get; set; }
        public EFuelLogDto MostStationAmount { get; set; }
        public string PlateCount { get; set; }
        public int DebitVehicleCount { get; set; }//Zimmetli araç sayısı
        public decimal? DifferenceAmount { get; set; }
        public decimal PercentageUnit { get; set; }
        public decimal PercentagePlate { get; set; }
        public decimal? UserFaultAmount { get; set; }

        //Trip
        public int TotalCount { get; set; }
    }
}