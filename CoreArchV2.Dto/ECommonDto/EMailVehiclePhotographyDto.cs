namespace CoreArchV2.Dto.ECommonDto
{
    public class EMailVehiclePhotographyDto
    {
        public int CreatedBy { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public int VehiclePhysicalImageId { get; set; }
        public string DifferentMail { get; set; }
        public string SenderMailTo { get; set; }
        public string SenderMailCc { get; set; }
        public string SenderMailBcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Root { get; set; }
    }
}
