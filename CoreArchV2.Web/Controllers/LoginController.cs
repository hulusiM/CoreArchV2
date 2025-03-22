using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Util.Hash;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.SignalR;
using CoreArchV2.Utilies;
using CoreArchV2.Utilies.SessionOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using Authorization = CoreArchV2.Core.Entity.Common.Authorization;

namespace CoreArchV2.Web.Controllers
{
    public class LoginController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cacheService;
        private readonly FirmSetting _firmSetting;
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;
        private readonly SessionContext _sessionContext;
        private readonly ILogger<LoginController> _logger;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ActiveUserForSignalR> _activeUserRepository;
        private readonly IGenericRepository<LoginLog> _loginLogRepository;
        private readonly IGenericRepository<Authorization> _authorizationRepository;
        private readonly IMailService _mailService;
        private readonly ILicenceWebService _licenceService;

        public LoginController(IUnitOfWork uow,
            IMapper mapper,
            IUserService userService,
            IMenuService menuService,
            ICacheService cacheService,
            IHubContext<SignalRHub> hubContext,
            IMailService mailService,
            IOptions<FirmSetting> firmSetting,
            ILogger<LoginController> logger,
            ILicenceWebService licenceService)
        {
            _uow = uow;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
            _firmSetting = firmSetting.Value;
            _hubContext = hubContext;
            _userService = userService;
            _menuService = menuService;
            _mailService = mailService;
            _licenceService = licenceService;
            _sessionContext = new SessionContext();
            _loginLogRepository = uow.GetRepository<LoginLog>();
            _userRepository = uow.GetRepository<User>();
            _activeUserRepository = uow.GetRepository<ActiveUserForSignalR>();
            _authorizationRepository = uow.GetRepository<Authorization>();
        }

        public IActionResult Index()
        {
            var session = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext");
            if (session != null)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }


        public Task HangfireTrigger()
        {
            try
            {
                string url = _firmSetting.HangfireUrl;
                // Using WebRequest
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                // Using WebClient
                string result1 = new WebClient().DownloadString(url);
            }
            catch { }

            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> LoginCheck(string mobilePhone, string pass, string redirectUrl)
        {
            var loginInfo = new EUserDto();
            _ = Task.Run(() => HangfireTrigger());

            try
            {
                var licence = await _licenceService.Check();
                if (licence.StatusCode != HttpStatusCode.OK || !licence.Success)//Lisans kontrol
                {
                    loginInfo.LoginMessage = licence.Data;
                    return Json(loginInfo);
                }

                mobilePhone = mobilePhone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                if (!mobilePhone.StartsWith("0"))
                    mobilePhone = "0" + mobilePhone;

                var user = _userService.FindByUsernameAndPass(new EUserDto { MobilePhone = mobilePhone, Password = pass });
                if (user != null && user.ParentUnitId == null && user.UnitId == null)
                    loginInfo.LoginMessage = "Herhangi bir birime/projeye yetkiniz bulunmamaktadır. Yöneticiyle iletişime geçiniz.";
                else
                {
                    var logMessage = "";
                    if (user != null)
                    {
                        var authMenuList = _menuService.AuthMenuList(user.Id);
                        if (authMenuList.Any())
                        {
                            _sessionContext.AuthMenuList = authMenuList;
                            _sessionContext.StringMenuList = CreateMenuHtml(authMenuList);
                            _sessionContext.GetAllAuthorizationList = _menuService.GetAllAuthorization();
                            _sessionContext.User = user;
                            HttpContext.Session.SetComplexData("_sessionContext", _sessionContext);
                            HttpContext.Session.SetString("AuthorizationMenuList", CreateMenuHtml(authMenuList));
                            HttpContext.Session.SetString("UserName", user.FullName);
                            HttpContext.Session.SetString("UserImage", user.Image ?? "");
                            HttpContext.Session.SetString("UnitName", user.UnitName ?? "");
                            HttpContext.Session.SetInt32("UserId", user.Id);
                            HttpContext.Session.SetInt32("ParentUnitId", user.ParentUnitId ?? 0);
                            HttpContext.Session.SetInt32("UnitId", user.UnitId ?? 0);
                            HttpContext.Session.SetComplexData("IsAdmin", user.IsAdmin);

                            await _cacheService.AddCacheAsync(MemoryCache_.ActiveUser.ToString(), user);

                            //var option = new CookieOptions { Expires = DateTime.Now.AddDays(1) }; //SignalR for login
                            //Response.Cookies.Append("UserId", user.Id.ToString(), option);

                            logMessage = $"*** (Başarılı Giriş) - Kullanıcı bilgileri: {user.Id},{DateTime.Now}";
                            _logger.LogInformation(logMessage);
                            LoginLog(user.Id);
                            KillOnline();
                            if (!string.IsNullOrEmpty(redirectUrl))
                                loginInfo.RedirectUrl = redirectUrl;
                            else
                            {
                                var homePageUrl = _menuService.GetUserIdWithHomePage(user.Id);
                                if (OneWayHash.Create(user.Name.ToLower()) == OneWayHash.Create(pass.ToLower()))
                                    loginInfo.RedirectUrl = "/User/MyAccount";//şifre değiştirme sayfası
                                else if (homePageUrl != "")
                                    loginInfo.RedirectUrl = homePageUrl;


                                //else if (authMenuList.Any(a => a.id == (int)AuthorizationId.ManagerHomePage))
                                //    loginInfo.RedirectUrl = "/Manager/Dashboard";//Yönetici anasayfa
                                //else
                                //    loginInfo.RedirectUrl = "/Home/Index";//Normal anasayfa
                            }
                        }
                        else
                        {
                            logMessage = "Herhangi bir menüye yetkiniz bulunmamaktadır. Yöneticiyle iletişime geçiniz.";
                            _logger.LogInformation(string.Format("*** " + logMessage + " Kullanıcı Bilgileri: {0},{1}", user.Id, DateTime.UtcNow));
                            loginInfo.LoginMessage = logMessage;
                        }
                    }
                    else
                    {
                        logMessage = $"*** (Hatalı Giriş) - Kullanıcı bilgileri: {mobilePhone} - {pass} - {DateTime.UtcNow}";
                        _logger.LogInformation(logMessage);
                        loginInfo.LoginMessage = "Kullanıcı Bilgileri Hatalı";
                    }
                }

                return Json(loginInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("*** Kritik Hata! ", ex);
                loginInfo.LoginMessage = "Kritik hata!, adminle iletişime geçiniz.";
                return Json(loginInfo);
            }
        }

        public void KillOnline()
        {
            try
            {
                if (_loginUserInfo != null)
                {
                    var date = DateTime.Now.AddDays(-1);
                    var allUserPast = _activeUserRepository.Where(w => w.Status && w.CreatedDate < date).ToList();//geçmişte açık kalan session

                    var user = _activeUserRepository.Where(w => w.UserId == _loginUserInfo.Id && w.Status).ToList();

                    var result = user.Concat(allUserPast).ToList();

                    result.ForEach(f => f.Status = false);
                    _activeUserRepository.UpdateRange(result);
                    _uow.SaveChanges();
                }
            }
            catch (Exception e)
            {
            }
        }
        public IActionResult GenerateNewPassword(string email)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var user = _userService.FindByMail(email);
                if (user == null)
                    result.Message = "Sistemde kayıtlı E-mail adresi bulunamadı";
                else
                {
                    var pass = Guid.NewGuid().ToString("d").Substring(1, 6);
                    var newPass = OneWayHash.Create(pass);

                    user.IsNewPassword = true;
                    user.NewPassword = newPass;
                    var body = user.Name + " merhaba,<br />" +
                               "Az önce şifre talebinde bulundunuz. <br /> Yeni Şifreniz: <b>" + pass + "</b>" +
                               "<br /><a href='" + _firmSetting.WebSite + "'>" + _firmSetting.WebSiteName + "</a>" +
                               "<br /><br />Not: Bu işlemi siz yapmadıysanız güvenlik için şifrenizi değiştiriniz." +
                               "<br />Şifrenizi değiştirmek için <a href='" + _firmSetting.WebSite + "User/MyAccount'>tıklayınız</a>";

                    if (_mailService.SendMail(user.Email.Replace(" ", ""), "Yeni Şifre Talebi", body))
                    {
                        _userRepository.Update(user);
                        _uow.SaveChanges();
                        result.IsSuccess = true;
                    }
                    else
                        result.Message = "Mail gönderilemedi";
                }
            }
            catch (Exception e) { result.Message = "Hata oluştu, adminle iletişime geçiniz"; }

            return Json(result);
        }
        public void LoginLog(int userId)
        {
            try
            {
                var loginUserInfo = new LoginLog
                {
                    UserId = userId,
                    LoginDate = DateTime.Now
                };
                _loginLogRepository.Insert(loginUserInfo);
                _uow.SaveChanges();
            }
            catch (Exception)
            {
            }
        }
        [HttpGet]
        public IActionResult Logout()
        {
            try
            {
                var userId = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext").User.Id;
                var userInfo = _userService.Find(userId);
                userInfo.IsActive = false;
                userInfo.LogOutDate = DateTime.Now;
                _uow.SaveChanges();
                KillOnline();
            }
            catch (Exception)
            {
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
        public string CreateMenuHtml(List<EAuthorizationDto> menuList)
        {
            var menu = "";
            foreach (var item in menuList.Where(w => w.IsMenu).ToList())
                if (item.children.Count > 0)
                {
                    menu += SubCreateChild(item);
                }
                else
                {
                    menu += "<li>";
                    menu += "<a " + item.Attribute + " href='/" + item.Controller + "/" + item.Action + "'><i class='" + item.icon +
                            "'></i> <span>" + item.Name + "</span></a>";
                    menu += "</li>";
                }

            return menu;
        }
        public string SubCreateChild(EAuthorizationDto subMenu)
        {
            var menu = "<li>";
            if (subMenu.children.Count > 0)
                menu += "<a " + subMenu.Attribute + " href='#'><i class='" + subMenu.icon + "'></i> <span>" + subMenu.Name + "</span></a>";
            menu += "<ul style='display: none; margin-top: -2px;'>";
            foreach (var item in subMenu.children.Where(w => w.IsMenu))
            {
                if (item.children.Count(w => w.IsMenu) > 0)
                    menu += SubCreateChild(item);

                if (item.Controller != "" && item.Action != "")
                    menu += "<li><a " + item.Attribute + " href='/" + item.Controller + "/" + item.Action + "'><i class='" + item.icon +
                            "'></i> <span>" + item.Name + "</span></a>";
            }

            menu += "</ul>";
            menu += "</li>";
            return menu;
        }
        public async Task<IActionResult> UserConnectionInsertSignalR(ActiveUserForSignalR model)
        {
            try
            {
                if (_loginUserInfo != null)
                {
                    KillOnline();
                    var urlName = _authorizationRepository.FirstOrDefault(f => "/" + f.Controller + "/" + f.Action == model.Url);
                    var entity = new ActiveUserForSignalR()
                    {
                        CreatedDate = DateTime.Now,
                        Url = model.Url,
                        Id = 0,
                        Menu = urlName != null ? urlName.Name : (model.Url.Contains("Home") ? "Anasayfa" : ""),
                        ConnectionId = model.ConnectionId,
                        UserId = _loginUserInfo.Id
                    };
                    await _activeUserRepository.InsertAsync(entity);
                    _uow.SaveChanges();

                    await _hubContext.Clients.All.SendAsync("onlineUser");

                    return Json(true);
                }

                return Json(false);
            }
            catch (Exception e)
            {
            }
            return Json(false);
        }

        public async Task<IActionResult> GetOnlineUser()
        {
            var list = await Task.FromResult((from o in _activeUserRepository.GetAll()
                                              join u in _userRepository.GetAll() on o.UserId equals u.Id
                                              where o.Status
                                              select new
                                              {
                                                  Id = o.UserId,
                                                  UserName = u.Name + " " + u.Surname /*+ "-<b style='color:red'>" + o.Menu + "</b>"*/,
                                                  Image = u.Image != null ? Convert.ToBase64String(u.Image) : "",
                                              }).Distinct().ToList());
            return Json(list);
        }
    }

    #region Session operations

    public static class SessionExtensions
    {
        public static T GetComplexData<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null) return default;
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static void SetComplexData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }

    #endregion Session operations
}