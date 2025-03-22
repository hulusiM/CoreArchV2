using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies.SessionOperations;

namespace CoreArchV2.Services.Interfaces
{
    public interface IRoleService
    {
        SessionContext _workContext { get; set; }
        PagedList<ERoleDto> GetAllWithPaged(int? page, ERoleDto filterModel);

        List<EAuthorizationDto> RoleWithChildrenAuthorizationList(int roleId);

        EResultDto Delete(int id);

        bool DeleteRoleAuthorization(int id);
        bool Insert(ERoleAuthorizationDto model);
        bool Update(ERoleAuthorizationDto model);
    }
}