using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Services.Interfaces
{
    public interface IMenuService
    {
        List<EAuthorizationDto> AuthMenuList(int userId);

        List<EAuthorizationDto> GetAllAuthorization();

        List<EAuthorizationDto> GetAllAuthMenuList();
        string GetUserIdWithHomePage(int userId);
        List<EComboboxDto> GetRoleWithHomePageList(int roleId);
        List<User> GetAuthUserList();
    }
}