namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleTransferFileDto
    {
        public int Id { get; set; }
        public int FileUploadId { get; set; }
        public int VehicleTransferId { get; set; }

        //VehicleTransferLog
        public int VehicleId { get; set; }
        public decimal? SalesCost { get; set; }
        public int TransferTypeId { get; set; }
        public string TransferTypeName { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }

        public int DeleteUserId { get; set; }
        public DateTime DebitVehicleEndDate { get; set; }
    }
}