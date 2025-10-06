using CoreArchV2.Api.Helper;
using CoreArchV2.Core.Entity.Licence.Dto;
using CoreArchV2.Core.Entity.Licence.Entity;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWorkLicence;
using Newtonsoft.Json;
using System.Net;

namespace CoreArchV2.Api.Licence.Services
{
    public class LicenceService : ILicenceService
    {
        private readonly IUowLicence _uow;
        private readonly IGenericRepository<LicenceKey> _licenceKeyRepository;
        private readonly IGenericRepository<RequestLog> _requestLogRepository;
        private readonly IGenericRepository<IpBlackList> _ipBlackListRepository;
        public LicenceService(IUowLicence uow)
        {
            _uow = uow;
            _licenceKeyRepository = uow.GetRepository<LicenceKey>();
            _requestLogRepository = uow.GetRepository<RequestLog>();
            _ipBlackListRepository = uow.GetRepository<IpBlackList>();
        }

        public async Task<ObjectActionResult> LicenceControl(LicenceRequestDto model)
        {
            var response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.BadRequest, data: null);
            var licence = await LicenceCheck(model);
            try
            {
                var dateNow = DateTime.Now;
                if (licence != null && !licence.IsLock)
                {
                    var licenceEndDate = Convert.ToInt32(dateNow.Subtract(licence.EndDate).TotalDays);
                    if (licence.EndDate < dateNow) //Lisansı bitmiş
                    {
                        licence.IsActive = false;
                        licence.IsLock = true;
                        licence.LockDate = dateNow;
                        licence.ErrorMessage = "Lisansı dolmuş";
                        _licenceKeyRepository.Update(licence);
                        await _uow.CommitAsync();
                        response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisansınız dolmuştur, lütfen yöneticiyle iletişime geçiniz.");
                    }
                    else
                        response = new ObjectActionResult(success: true, statusCode: HttpStatusCode.OK, data: "Lisans kontrolü başarılı");
                }
                else
                {
                    if (licence != null && licence.IsLock)
                        response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans kilitli. Lütfen adminle iletişime geçiniz..");
                    else
                        response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans bulunamadı-1");
                }
            }
            catch (Exception)
            {
                response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans sorgulama yaparken hata oluştu. Adminle iletişime geçiniz. Hata Kodu: 12");
            }

            await InsertLog(licence?.Id, "Lisans kontrol", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(new { success = response.Success, statusCode = response.StatusCode, data = response.Data }));
            return response;
        }
        public async Task<LicenceKey> LicenceCheck(LicenceRequestDto model)
        {
            //kara listedeki ip istek atamaz
            var dateNow = DateTime.Now;
            try
            {
                var licence = await _licenceKeyRepository.FirstOrDefaultAsync(f => f.IsActive && f.FirmKey == model.FirmKey && f.FirmName == model.FirmName && f.Ip == model.Ip);
                return licence;
            }
            catch (Exception ex)
            {
                //Mail gönder -- jsonresult hata aldı
                await _requestLogRepository.InsertAsync(new RequestLog()
                {
                    CreatedDate = dateNow,
                    RequestDate = dateNow,
                    TypeName = "Hata: LicenceCheck --> Message: " + ex.Message,
                    RequestJson = JsonConvert.SerializeObject(model),
                    ResponseJson = JsonConvert.SerializeObject("Hata: LicenceCheck --> Message: " + ex.Message)
                });
                await _uow.CommitAsync();
            }

            return null;
        }
        public async Task InsertLog(int? licenceId, string typeName, string request, string response)
        {
            var dateNow = DateTime.Now;
            await _requestLogRepository.InsertAsync(new RequestLog()
            {
                CreatedDate = dateNow,
                LicenceKeyId = licenceId,
                RequestDate = dateNow,
                TypeName = typeName,
                RequestJson = request,
                ResponseJson = response
            });
            await _uow.CommitAsync();
        }
        public async Task<ObjectActionResult> AddUserRole(LicenceRequestDto model)
        {
            var response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: null);
            var licence = await LicenceCheck(model);
            try
            {
                if (licence != null && !licence.IsLock)
                {
                    if (licence.MaxUserCount > (licence.ActiveUserCount ?? 0))
                    {
                        licence.ActiveUserCount += 1;
                        _licenceKeyRepository.Update(licence);
                        await _uow.CommitAsync();
                        response = new ObjectActionResult(success: true, statusCode: HttpStatusCode.OK, data: (licence.MaxUserCount - licence.ActiveUserCount) + " adet kullanıcı rol ekleme hakkınız kaldı.");
                    }
                    else
                        response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans için maksimum kullanıcı-rol sayısına ulaştınız. Lütfen yönetici ile iletişime geçiniz");
                }
                else
                    response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans bulunamadı");
            }
            catch (Exception ex)
            {
                response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kullanıcı ekleme yaparken hata oluştu. Adminle iletişime geçiniz.");
            }

            await InsertLog(licence?.Id, "Rol Ekleme", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(new { success = response.Success, statusCode = response.StatusCode, data = response.Data }));
            return response;
        }
        public async Task<ObjectActionResult> DeleteUserRole(LicenceRequestDto model)
        {
            var response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: null);
            var licence = await LicenceCheck(model);
            try
            {
                if (licence != null && !licence.IsLock)
                {
                    licence.ActiveUserCount -= 1;
                    _licenceKeyRepository.Update(licence);
                    await _uow.CommitAsync();
                    response = new ObjectActionResult(success: true, statusCode: HttpStatusCode.OK, data: (licence.MaxUserCount - licence.ActiveUserCount) + " adet kullanıcı rol ekleme hakkınız kaldı.");
                }
                else if (licence != null && licence.IsLock)
                    response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans kilitli. Lütfen adminle iletişime geçiniz.");
                else
                    response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans bulunamadı");
            }
            catch (Exception ex)
            {
                response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans kullanıcı silme sırasında hata oluştu. Adminle iletişime geçiniz.");
            }

            await InsertLog(licence?.Id, "Rol Silme", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(new { success = response.Success, statusCode = response.StatusCode, data = response.Data }));
            return response;
        }



        public async Task<ObjectActionResult> AddVehicle(LicenceRequestDto model)
        {
            var response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: null);
            var licence = await LicenceCheck(model);
            try
            {
                if (licence != null && !licence.IsLock)
                {
                    if (licence.MaxVehicleCount > (licence.ActiveVehicleCount ?? 0))
                    {
                        licence.ActiveVehicleCount += 1;
                        _licenceKeyRepository.Update(licence);
                        await _uow.CommitAsync();
                        response = new ObjectActionResult(success: true, statusCode: HttpStatusCode.OK, data: (licence.MaxVehicleCount - licence.ActiveVehicleCount) + " adet araç ekleme hakkınız kaldı.");
                    }
                    else
                        response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans için maksimum araç sayısına ulaştınız. Lütfen yönetici ile iletişime geçiniz");
                }
                else
                    response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans bulunamadı");
            }
            catch (Exception ex)
            {
                response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans araç ekleme yaparken hata oluştu. Adminle iletişime geçiniz.");
            }

            await InsertLog(licence?.Id, "Araç Ekleme", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(new { success = response.Success, statusCode = response.StatusCode, data = response.Data }));
            return response;
        }
        public async Task<ObjectActionResult> DeleteVehicle(LicenceRequestDto model)
        {
            var response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: null);
            var licence = await LicenceCheck(model);
            try
            {
                if (licence != null && !licence.IsLock)
                {
                    licence.ActiveVehicleCount -= 1;
                    _licenceKeyRepository.Update(licence);
                    await _uow.CommitAsync();
                    response = new ObjectActionResult(success: true, statusCode: HttpStatusCode.OK, data: (licence.MaxVehicleCount - licence.ActiveVehicleCount) + " adet kullanıcı rol ekleme hakkınız kaldı.");
                }
                else if (licence != null && licence.IsLock)
                    response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans kilitli. Lütfen adminle iletişime geçiniz.");
                else
                    response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Lisans bulunamadı");
            }
            catch (Exception ex)
            {
                response = new ObjectActionResult(success: false, statusCode: HttpStatusCode.BadRequest, data: "Lisans araç silme sırasında hata oluştu. Adminle iletişime geçiniz.");
            }

            await InsertLog(licence?.Id, "Araç Silme", JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(new { success = response.Success, statusCode = response.StatusCode, data = response.Data }));
            return response;
        }
    }
}
