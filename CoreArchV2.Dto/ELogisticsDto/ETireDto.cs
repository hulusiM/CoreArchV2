using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class ETireDto : EIdDto
    {
        public int VehicleId { get; set; }
        public int? VehicleId2 { get; set; }
        public int TireDebitId { get; set; }
        public int? WareHouseId { get; set; }
        public int TargetWareHouseId { get; set; }
        public int TireChangeType { get; set; }
        public int DimensionTypeId { get; set; }
        public int TireTypeId { get; set; }
        public int State { get; set; }
        public int TireCount { get; set; }
        public string Description { get; set; }

        public string DimensionTypeName { get; set; }
        public string TireTypeName { get; set; }
        public string WareHouseName { get; set; }
        public string Plate { get; set; }
        public string Message { get; set; }


        public List<EFileUploadDto> files { get; set; }

        public List<EFileUploadDto> FileUploads { get; set; }
    }
}