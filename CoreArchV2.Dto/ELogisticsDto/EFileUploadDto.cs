namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EFileUploadDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Extention { get; set; }
        public long FileSize { get; set; }

        //Bakım/Oarım
        public int MaintenanceId { get; set; }
        public int MaintenanceFileId { get; set; }
        public int MaintenanceTypeId { get; set; }

        //Ceza
        public int CriminalId { get; set; }
        public int CriminalFileId { get; set; }
        public int CriminalTypeId { get; set; }

        //Lastik
        public int TireId { get; set; }
        public int TireFileId { get; set; }
        public int TireTypeId { get; set; }

        public int VehicleFileId { get; set; }

        //Tender
        public int TenderId { get; set; }
        public int TenderFileId { get; set; }

        //NoticeUnit
        public int NoticeUnitId { get; set; }
        public int NoticeUnitFileId { get; set; }


    }
}