using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IUserRoleService
    {
        PagedList<EUserRoleDto> GetAllWithPaged(int? page, EUserRoleDto filterModel);
        Task<EResultDto> Insert(EUserRoleDto model);
        EResultDto Update(EUserRoleDto model);
        EResultDto Delete(int id, int deletedBy);
    }
}