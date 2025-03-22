namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleContractWithAmountDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? FirstKm { get; set; }
        public decimal? VehicleLastKm { get; set; }
        public decimal? MaxKmLimit { get; set; }
        public string CustomButton { get; set; }
        public List<EVehicleAmountDto> VehicleAmountList { get; set; }
    }
}