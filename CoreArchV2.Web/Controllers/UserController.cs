using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Licence.Dto;
using CoreArchV2.Core.Util.Hash;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies;
using CoreArchV2.Utilies.SessionOperations;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class UserController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;

        private readonly IGenericRepository<User> _userRepository;

        public UserController(IMapper mapper,
            IUnitOfWork uow,
            IUserService userService)
        {
            _mapper = mapper;
            _userRepository = uow.GetRepository<User>();
            _uow = uow;
            _userService = userService;
        }

        public IActionResult Index() => View();
        public IActionResult MyAccount()
        {
            var user = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext").User;
            if (user != null)
            {
                var userEntity = _userRepository.Find(user.Id);
                if (userEntity.IsNewPassword)
                    user.IsChangePassWarning = true;
            }

            return View(user);
        }
        public IActionResult UserGetAll(int? page, EUserDto filterModel)
        {
            var result = _userService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/User/UserGetAll"));
            return Json(result);
        }

        [HttpPost]
        public IActionResult SaveImage(int userId, string base64image)
        {
            try
            {
                var t = base64image.Split(",");
                var newBase64Image = "";
                for (int i = 0; i < t.Length - 1; i++)
                    newBase64Image += t[i + 1].ToString(); // i+1 => remove data:image/png;base64,

                byte[] bytes = Convert.FromBase64String(newBase64Image);

                if (newBase64Image != "")
                {
                    var user = _userRepository.Find(userId);
                    user.Image = bytes;
                    _userRepository.Update(user);
                    _uow.SaveChanges();
                    return Json(true);
                }
                else
                    return Json("Lütfen resim seçiniz");
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }
        public IActionResult DeleteImage(int userId)
        {
            try
            {
                var user = _userRepository.Find(userId);
                if (user.Image != null)
                {
                    user.Image = null;
                    _userRepository.Update(user);
                    _uow.SaveChanges();
                    return Json(true);
                }
                else
                    return Json("Profil resmi bulunamadı");
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = _userService.Find(userId);
            var result = new EResultDto() { IsSuccess = false };
            if (user != null)
            {
                if (user.Password.Length > 10)
                    result.Message = "Şifre max 10 karakter olabilir";
                if (user.Name.ToLower() == newPassword.ToLower()
                    || user.Surname.ToLower() == newPassword.ToLower()
                    || (user.Name + " " + user.Surname).ToLower() == newPassword.ToLower()
                    || (user.Name + user.Surname).ToLower() == newPassword.ToLower())
                    result.Message = "Şifreniz ad/soyadınız olamaz";
                else
                {
                    if (user.Password == OneWayHash.Create(oldPassword))
                    {
                        user.Password = OneWayHash.Create(newPassword);
                        //_userService.Update(user);//servise gidince ve map yapınca password alanı null yapıyor!! Değiştirme 
                        _userRepository.Update(user);
                        _uow.SaveChanges();
                        result.IsSuccess = true;
                        result.Message = "Şifre Başarıyla Değiştirilmiştir.";
                    }
                    else
                        result.Message = "Eski şifreyi hatalı girdiniz, tekrar deneyiniz!";
                }
            }
            else
                result.Message = "Kullanıcı Bulunamadı!";
            return Json(result);
        }


        #region Static Methods

        public IActionResult Insert(EUserDto model)
        {
            model.MobilePhone = model.MobilePhone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");

            if (!model.MobilePhone.StartsWith("0"))
                model.MobilePhone = "0" + model.MobilePhone;

            model.Name = ExternalProcess.SetPascalCaseNameWithSpace(model.Name);
            model.Surname = ExternalProcess.SetPascalCaseNameWithSpace(model.Surname);

            EResultDto result = new EResultDto();
            if (!string.IsNullOrEmpty(model.Password) && model.Password.Length < 4)
            {
                result.IsSuccess = false;
                result.Message = "Şifre min. 4 karakter olmalıdır";
            }
            else
            {
                model.CreatedBy = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext").User.Id;
                if (model.Id > 0)
                    result = _userService.Update(model);
                else
                    result = _userService.Insert(model);
            }
            return Json(result);
        }

        public async Task<IActionResult> GetById(int id) => Json(await _userService.GetByIdAsync(id));
        public IActionResult Delete(int id) => Json(_userService.Delete(id));
        #endregion Static Methods

        #region Lisans kontrolü

        public IActionResult LicenceIndex() => View();

        public IActionResult LicenceGetAll(int? page, EUserDto filterModel)
        {
            var result = _userService.GetAllLicenceWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/User/LicenceGetAll"));
            return Json(result);
        }

        public IActionResult LicenceAddUpdate(LicenceKeyDto entity)
        {
            EResultDto result = new EResultDto();
            entity.UpdatedBy = _loginUserInfo.Id;
            if (entity.Id > 0)
                result = _userService.LicenceUpdate(entity);
            else
                result = _userService.LicenceInsert(entity);

            return Json(result);
        }

        public IActionResult LicenceDeleteUser(int id)
        {
            return Json(_userService.LicenceDeleteUser(id));
        }

        public IActionResult LicenceGetById(int id)
        {
            return Json(_userService.LicenceGetById(id));
        }
        #endregion
    }
}