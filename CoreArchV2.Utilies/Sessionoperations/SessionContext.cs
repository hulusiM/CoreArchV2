using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Utilies.SessionOperations
{
    public class SessionContext
    {
        public List<EAuthorizationDto> AuthMenuList { get; set; } //Yetkili olduğu menüler
        public string StringMenuList { get; set; }
        public List<EAuthorizationDto> GetAllAuthorizationList { get; set; }
        public EUserDto User { get; set; }
    }
}