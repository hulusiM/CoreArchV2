using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehiclePhysicalImageDto
    {
        public int CreatedBy { get; set; }
        public int VehicleId { get; set; }
        public List<string> PhotoList { get; set; }
        //public List<IFormFile> PhotoList { get; set; }
    }

    public class EVehiclePhysicalImageLoadDto
    {
        public IList<IFormFile> files { get; set; }
        public int VehicleId { get; set; }
        public int CreatedBy { get; set; }
        public int TypeId { get; set; }
    }
}
