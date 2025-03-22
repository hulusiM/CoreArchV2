using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoreArchV2.Web.Controllers
{
    public class MobileController : AdminController
    {
        private readonly FirmSetting _firmSetting;
        private readonly IUnitOfWork _uow;

        private readonly IWebHostEnvironment _environment;
        private readonly IMobileService _mobileService;
        private readonly IMenuService _menuService;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Device> _deviceRepository;
        private readonly IMailService _mailService;


        public MobileController(IUnitOfWork uow,
            IMobileService mobileService,
            IWebHostEnvironment environment,
            IMenuService menuService,
            IOptions<FirmSetting> firmSetting,
            IMailService mailService)
        {
            _uow = uow;
            _environment = environment;
            _menuService = menuService;
            _mobileService = mobileService;
            _firmSetting = firmSetting.Value;
            _userRepository = uow.GetRepository<User>();
            _deviceRepository = uow.GetRepository<Device>();
            _mailService = mailService;
        }

        public IActionResult SenderMessage() => View();


        public IActionResult App()
        {
            string path = Path.Combine(_environment.WebRootPath, _firmSetting.MobileApkUrl);
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", "MobileApplication.apk");//erpMobile
        }

        public IActionResult MessageGetAll(int? page, EMessageLogDto filterModel)
        {
            var result = _mobileService.GetAllWithPaged(page, filterModel);
            foreach (var item in result)
                item.MessageType = GetMessageType(item.Type);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Mobile/MessageGetAll"));
            return Json(result);
        }

        public string GetMessageType(int type)
        {
            string result = "";
            if (type == (int)MessageLogType.EMail)
                result = "<span class='label bg-primary-300'>Email</span>";
            else if (type == (int)MessageLogType.Sms)
                result = "<span class='label bg-orange-300'>Sms</span>";
            else if (type == (int)MessageLogType.PushNotification)
                result = "<span class='label bg-danger-300'>Push Notification</span>";
            else
                result = "";

            return result;
        }

        public async Task<IActionResult> SendMessageDirectly(EMessageLogDto model)
        {
            var result = new EResultDto();
            model.CreatedBy = _loginUserInfo.Id;
            var startDate = DateTime.Now;
            if (model.Type == (int)MessageLogType.EMail)
            {
                if (model.IsAllUser)//parametre tablosu onayını bekle
                {
                    int sendSuccess = 0, sendFault = 0;
                    var userList = _menuService.GetAuthUserList().Where(w => w.Email != null).ToList();
                    if (userList.Any())
                    {
                        //StartSending("Toplu E-Mail Gönderimi Başladı", model.Body, startDate, DateTime.MinValue, ("Toplam <b>" + userList.Count + "</b> kişiye gönderilmesi planlandı"));
                        //foreach (var item in userList)
                        //{
                        //    model.UserId = item.Id;
                        //    model.Email = item.Email;

                        //    var sender = await _mobileService.SendMailAsync(model);
                        //    if (sender.IsSuccess)
                        //        sendSuccess++;
                        //    else
                        //        sendFault++;
                        //}
                        //result.IsSuccess = true;
                        //string mess = "Toplam <b>" + userList.Count + "</b> kişiye gönderildi<br/> Başarılı sayısı: <b>" + sendSuccess + "</b><br/>Başarısız sayısı: <b>" + sendFault + "</b>";
                        //result.Message = mess;
                        //StartSending("Toplu E-Mail Gönderimi Bitti", model.Body, startDate, DateTime.Now, mess);
                    }
                }
                else
                    result = await _mobileService.SendMailAsync(model);
            }
            else if (model.Type == (int)MessageLogType.PushNotification)
            {
                if (model.IsAllUser)
                {
                    int sendSuccess = 0, sendFault = 0;
                    var userList = _deviceRepository.Where(w => w.Status).ToList();
                    if (userList.Any())
                    {
                        //StartSending("Toplu Push Notification Gönderimi Başladı", model.Body, startDate, DateTime.MinValue, ("Toplam <b>" + userList.Count + "</b> kişiye gönderilmesi planlandı"));
                        //foreach (var item in userList)
                        //{
                        //    model.PushToken = item.PushToken;
                        //    model.UserId = item.Id;
                        //    var sender = await _mobileService.PushNotificationAsync(model);
                        //    if (sender.IsSuccess)
                        //        sendSuccess++;
                        //    else
                        //        sendFault++;
                        //}
                        //result.IsSuccess = true;
                        //string mess = "Toplam <b>" + userList.Count + "</b> kişiye gönderildi<br/> Başarılı sayısı: <b>" + sendSuccess + "</b><br/>Başarısız sayısı: <b>" + sendFault + "</b>";
                        //result.Message = mess;
                        //StartSending("Toplu Push Notification Gönderimi Bitti", model.Body, startDate, DateTime.Now, mess);
                    }
                }
                else
                    result = await _mobileService.PushNotificationAsync(model);
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "Bu hizmet şu anda aktif değildir";
            }
            return Json(result);
        }

        public async Task StartSending(string subject, string body, DateTime startDate, DateTime endDate, string note = "")
        {
            try
            {
                var adminMailList = await _mobileService.GetParameterByKey(ParameterEnum.AdminMailList.ToString());
                var mailList = adminMailList?.ValueP;
                if (!string.IsNullOrEmpty(mailList))
                {
                    string message = "Toplu E-Mail Gönderimi Bilgileri<br/><br/>";
                    message += "<hr/>";
                    message += "Başlama Saati: " + startDate + "<br/>";
                    message += "Bitiş Saati: " + (endDate == DateTime.MinValue ? "Devam ediyor" : endDate.ToString()) + "<br/>";
                    message += "<hr/>";
                    message += "<b>E-Mail İçeriği</b><br/>";
                    message += "<u>Konu</u>: " + subject + "<br/>";
                    message += "<u>İçerik</u>: " + body + "<br/>";
                    message += "<u>Gönderen Kişi</u>: " + _loginUserInfo.NameSurname + "<br/>";
                    message += "<hr/>";
                    message += note;
                    message += "<hr/>";
                    message += "<br/><b>Not:</b> Bu E-Mail sistem tarafından otomatik gönderilmiştir. <br/>Lütfen yanıtlamayınız.";
                    _mailService.SendMail(mailList, subject, message);
                }
            }
            catch (Exception ex) { }
        }
    }
}
