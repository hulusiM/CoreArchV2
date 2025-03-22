using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Licence.Dto;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IUserService
    {
        PagedList<EUserDto> GetAllWithPaged(int? page, EUserDto filterModel);
        Task<IEnumerable<User>> GetAllAsync();
        EUserDto FindByUsernameAndPass(EUserDto model);
        Task<EUserDto> GetByIdAsync(int id);
        EResultDto Insert(EUserDto model);
        EResultDto Update(EUserDto model);
        EResultDto Delete(int id);
        List<EAuthorizationDto> GetWithUserRoleById(int userId);
        User Find(int id);
        User FindByMail(string mail);
        User FindByPhone(string phone);
        Task<EResultDto> InsertDeviceAync(Device entity);

        #region Lisans
        PagedList<LicenceKeyDto> GetAllLicenceWithPaged(int? page, EUserDto filterModel);
        EResultDto LicenceInsert(LicenceKeyDto entity);
        EResultDto LicenceUpdate(LicenceKeyDto entity);
        EResultDto LicenceDeleteUser(int id);
        LicenceKeyDto LicenceGetById(int id);
        #endregion
    }
}