using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Util.Hash;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.EApiDto;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CoreArchV2.Services.Services
{
    public class UserRoleService : IUserRoleService
    {

        private readonly FirmSetting _firmSetting;
        private readonly IMenuService _menuService;
        private readonly IGenericRepository<Authorization> _authorizationRepository;
        private readonly IGenericRepository<RoleAuthorization> _roleAuthorizationRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userGenericRepository;
        private readonly IGenericRepository<UserRole> _userRoleGenericRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IMailService _mailService;
        private readonly ILicenceWebService _licenceService;

        public UserRoleService(IUnitOfWork uow,
            IMenuService menuService,
            IOptions<FirmSetting> firmSetting,
            IMailService mailService,
            ILicenceWebService licenceService)
        {
            _uow = uow;
            _roleRepository = uow.GetRepository<Role>();
            _userRepository = uow.GetRepository<User>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _authorizationRepository = uow.GetRepository<Authorization>();
            _roleAuthorizationRepository = uow.GetRepository<RoleAuthorization>();
            _menuService = menuService;
            _userRoleGenericRepository = uow.GetRepository<UserRole>();
            _userGenericRepository = uow.GetRepository<User>();
            _unitRepository = uow.GetRepository<Unit>();
            _mailService = mailService;
            _firmSetting = firmSetting.Value;
            _licenceService = licenceService;
        }

        public PagedList<EUserRoleDto> GetAllWithPaged(int? page, EUserRoleDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = from ur in _userRoleRepository.GetAll()
                       join home in _authorizationRepository.GetAll() on ur.HomePageId equals home.Id
                       join u in _userRepository.GetAll() on ur.UserId equals u.Id
                       join r in _roleRepository.GetAll() on ur.RoleId equals r.Id
                       select new EUserRoleDto
                       {
                           Id = ur.Id,
                           UserId = u.Id,
                           RoleId = ur.RoleId,
                           HomePageName = home.Name,
                           RoleName = r.Name,
                           UserNameSurname = u.Name + " " + u.Surname + "/" + u.MobilePhone,
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           CustomButton = "<li title='Satırı Sil' class='text-danger-800'><a onclick='funcDeleteModal(" +
                                          ur.Id +
                                          ")'><i class='icon-trash' style='padding: 3px;width: 20px;height: 20px;'></i></a></li>"
                       };

            if (filterModel.UserId > 0)
                list = list.Where(w => w.UserId == filterModel.UserId);

            if (filterModel.RoleId > 0)
                list = list.Where(w => w.RoleId == filterModel.RoleId);

            var lastResult =
                new PagedList<EUserRoleDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }

        public async Task<EResultDto> Insert(EUserRoleDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            var licence = new AResponseDto(success: false, statusCode: System.Net.HttpStatusCode.BadRequest, data: null);
            try
            {
                var homePageAuth = _menuService.GetRoleWithHomePageList(model.RoleId).FirstOrDefault(w => w.Id == model.HomePageId);
                if (homePageAuth == null)
                    result.Message = "Eklediğiniz anasayfa bu role tanımlı değildir";

                var user = _userGenericRepository.Find(model.UserId);
                if (user != null && user.UnitId == null)
                    result.Message = "Kullanıcıya rol verilebilmesi için birim veya proje atanması gerekiyor";
                else if (string.IsNullOrEmpty(user.MobilePhone))
                    result.Message = "Kullanıcıya rol verilebilmesi için telefon numarası eklenmesi gerekiyor";
                else if (string.IsNullOrEmpty(user.Email))
                    result.Message = "Kullanıcıya rol verilebilmesi için e-mail eklenmesi gerekiyor";
                else
                {
                    var isEarlyInsert = _userRoleGenericRepository.Any(w => w.UserId == model.UserId);
                    if (!isEarlyInsert)
                    {
                        var isSenderMail = false;
                        var pass = Guid.NewGuid().ToString("d").Substring(1, 6);
                        if (user.Password == null)
                        {
                            isSenderMail = true;
                            user.Password = OneWayHash.Create(pass);
                            user.IsNewPassword = true;
                        }

                        var entity = new UserRole
                        {
                            CreatedBy = model.CreatedBy,
                            UserId = model.UserId,
                            RoleId = model.RoleId,
                            HomePageId = model.HomePageId
                        };

                        licence = await _licenceService.AddUserRole(model.UserId, JsonConvert.SerializeObject(entity));//Lisansa rol ekleme
                        if (licence.StatusCode == System.Net.HttpStatusCode.OK && licence.Success)
                        {
                            _userGenericRepository.Update(user);
                            _userRoleGenericRepository.Insert(entity);
                            _uow.SaveChanges();
                            result.IsSuccess = true;
                            result.Message = "İşlem başarılı";

                            if (isSenderMail)
                                result.Message = SendOpenAccountUserMail(entity.UserId, pass);
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = licence.Data;
                        }
                    }
                    else
                        result.Message = "Kullanıcı üzerinde zaten rol var!";
                }
            }
            catch (Exception e)
            {
                result.Message = "Ekleme sırasında hata oluştu!";
                // hata alınca tekrardan lisansa kullanıcıyı silmek lazım
            }

            return result;
        }

        public EResultDto Update(EUserRoleDto model)
        {
            var result = new EResultDto();
            try
            {
                var entity = _userRoleGenericRepository.Find(model.Id);
                entity.UserId = model.UserId;
                entity.RoleId = model.RoleId;
                _userRoleGenericRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Message = "Hata oluştu";
                result.IsSuccess = false;
            }

            return result;
        }

        public EResultDto Delete(int id, int deletedBy)
        {
            var result = new EResultDto();
            try
            {
                var entity = _userRoleGenericRepository.Find(id);
                var user = _userGenericRepository.Find(entity.UserId);

                var licence = _licenceService.DeleteUserRole(deletedBy, JsonConvert.SerializeObject(entity) + "------" + JsonConvert.SerializeObject(user));//Lisans rol silme
                if (licence.Result.StatusCode == System.Net.HttpStatusCode.OK && licence.Result.Success)
                {
                    user.IsAdmin = false;
                    user.Flag = (int)Flag.User;
                    _userRoleGenericRepository.Delete(entity);
                    _userGenericRepository.Update(user);
                    _uow.SaveChanges();
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = licence.Result.Data;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }

            return result;
        }

        public string SendOpenAccountUserMail(int userId, string password)
        {
            var user = _userGenericRepository.Find(userId);
            var unit = _unitRepository.Find(user.UnitId.Value);
            if (!string.IsNullOrEmpty(user.Email))
            {
                var body = user.Name + " merhaba,<br />" +
                           "Portal'da <u>" + unit.Name +
                           "</u> birim yetkisiyle<br />hesabınız aktif edilmiştir.<br /><br />" +
                           "Giriş Bilgileri:<br />" +
                           "Telefon Numarası: <b>" + user.MobilePhone + "</b><br />" +
                           "Şifre:<b> " + password + "</b><br />" +
                           "Web Url: <a href='" + _firmSetting.WebSiteName + "'>" + _firmSetting.WebSiteName + "</a><br /><br />" +
                           "Not: Hesap bilgilerinizi kimseyle paylaşmayınız.";
                if (!_mailService.SendMail(user.Email, "Kullanıcıya Şifre Maili Gönderilemedi!", body))
                    return "Kullanıcıya rol atandı fakat mail adresine şifre gönderilemedi!!<br/><b>Kullanıcı tanımları</b> sayfasında yeni şifre oluşturup iletmeniz gerekiyor<br/><br/> Not: Kullanıcının mail adresini kontrol ediniz";

                return "İşlem Başarılı";
            }
            else
                return "Kullanıcının mail adresi bulunamadı";
        }
    }
}