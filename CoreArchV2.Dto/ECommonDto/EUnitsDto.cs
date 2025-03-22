namespace CoreArchV2.Dto.ECommonDto
{
    public class EUnitsDto
    {
        public EUnitDto Unit { get; set; }
        public EUnitDto ParentUnit { get; set; }
        public List<EUnitDto> UnitList { get; set; }
        public List<EUnitDto> UnitParentList { get; set; }
    }
}