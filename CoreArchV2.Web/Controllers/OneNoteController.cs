using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Note;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.Note;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ENoteDto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class OneNoteController : AdminController
    {
        private readonly IGenericRepository<OneNote> _oneNoteRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IMobileService _mobileService;
        private readonly IUnitOfWork _uow;
        private readonly IMailService _mailService;

        public OneNoteController(IUnitOfWork uow, IMobileService mobileService, IMailService mailService)
        {
            _uow = uow;
            _mobileService = mobileService;
            _oneNoteRepository = uow.GetRepository<OneNote>();
            _userRepository = uow.GetRepository<User>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _mailService = mailService;
        }
        public IActionResult Index() => View();
        public IActionResult ArventoRedirect()
        {
            var user = _userRepository.Find(_loginUserInfo.Id);
            if (user != null && !string.IsNullOrEmpty(user.ArventoUserName) && !string.IsNullOrEmpty(user.ArventoPassword))
            {
                var url = $"https://web.arvento.com/login.aspx?txtUserName={user.ArventoUserName.Replace(" ", "")}&txtPassword1={user.ArventoPassword.Replace(" ", "")}&l=tr-TR";
                return Redirect(url);
            }
            else
                return View();
        }
        public IActionResult GetAllOneNote()
        {
            var list = (from o in _oneNoteRepository.GetAll()
                        join u in _userRepository.GetAll() on o.CreatedBy equals u.Id
                        join p in _vehicleRepository.GetAll() on o.VehicleId equals p.Id
                        where o.Status && p.Status
                        select new EOneNoteDto()
                        {
                            Id = o.Id,
                            CreatedBy = o.CreatedBy,
                            Plate = p.Plate,
                            VehicleId = p.Id,
                            Description = o.Description,
                            ImportanceLevel = o.ImportanceLevel == 0 ? "Önemli" : "Normal",
                            Type = o.Type,
                            NameSurname = u.Name + " " + u.Surname.Substring(0, 1) + "."
                        }).ToList();


            var result = new EOneNoteListDto()
            {
                ToDo = list.Where(w => w.Type == (int)OneNoteType.ToDo).ToList(),
                Process = list.Where(w => w.Type == (int)OneNoteType.Process).ToList(),
                Finished = list.Where(w => w.Type == (int)OneNoteType.Finished).ToList()
            };
            return Json(result);
        }
        public IActionResult Delete(int id)
        {
            var result = new EResultDto { IsSuccess = false };
            try
            {
                if (id > 0)
                {
                    var entity = _oneNoteRepository.Find(id);
                    if (entity.CreatedBy == _loginUserInfo.Id)
                    {
                        entity.Status = false;
                        _oneNoteRepository.Update(entity);
                        _uow.SaveChanges();
                        result.IsSuccess = true;
                    }
                    else
                        result.Message = "Sadece <b>kart sahibi</b> silebilir";
                }
                else
                    result.Message = "Silme sırasında hata oluştu-1";
            }
            catch (Exception e) { result.Message = "Silme sırasında hata oluştu"; }

            return Json(result);
        }
        public IActionResult InsertUpdate(OneNote model)
        {
            var result = new EResultDto();
            try
            {
                if (model.Id > 0)
                {
                    var oldEnt = _oneNoteRepository.Find(model.Id);
                    if (oldEnt.CreatedBy == _loginUserInfo.Id)
                    {
                        oldEnt.Type = model.Type;
                        oldEnt.VehicleId = model.VehicleId;
                        oldEnt.Description = model.Description;
                        oldEnt.ImportanceLevel = model.ImportanceLevel;
                        _oneNoteRepository.Update(oldEnt);
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "<b>Kart sahibi</b> değişiklik yapabilir";
                    }
                }
                else
                {
                    var plate = _oneNoteRepository
                        .Where(w => w.Status && w.VehicleId == model.VehicleId && w.Type == model.Type)
                        .FirstOrDefault();
                    if (plate == null)
                    {
                        model.CreatedBy = _loginUserInfo.Id;
                        _oneNoteRepository.Insert(model);
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Araç üstünde zaten not bulunuyor. Üzerine ekleyebilirsiniz";
                    }
                }

                _uow.SaveChanges();
                result.Id = model.Id;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return Json(result);
        }
        public IActionResult GetVehicleNote(int vehicleId)
        {
            var notes = (from o in _oneNoteRepository.GetAll()
                         join v in _vehicleRepository.GetAll() on o.VehicleId equals v.Id
                         join u in _userRepository.GetAll() on o.CreatedBy equals u.Id
                         where o.VehicleId == vehicleId && o.Status
                         select new EOneNoteDto()
                         {
                             Id = o.Id,
                             Plate = v.Plate,
                             Description = o.Description,
                             CreatedDate = o.CreatedDate,
                             Type = o.Type,
                             NameSurname = u.Name + " " + u.Surname
                         }).OrderBy(o => o.CreatedDate).ToList();
            return Json(notes);
        }
        public async Task<IActionResult> SendSuggestionMessage(string message)
        {
            var sendUser = _userRepository.Find(_loginUserInfo.Id);
            var adminMailList = await _mobileService.GetParameterByKey(ParameterEnum.AdminMailList.ToString());

            var mailList = adminMailList?.ValueP;
            if (!string.IsNullOrEmpty(mailList))
            {
                if (sendUser != null)
                    message += "<br/><br/><b>Gönderen Kullanıcı:<br/></b> " + sendUser.Name + " " + sendUser.Surname + "<br/>Telefon:<a href='tel:" + sendUser.MobilePhone + "'>" + sendUser.MobilePhone + "</a>" +
                        "<br/>Kullanıcıyı aramak için <a href='tel:" + sendUser.MobilePhone + "'>tıklayınız</a>";

                message += "<br/><br/>Not: Bu E-Mail sistem tarafından otomatik gönderilmiştir.<br/>Lütfen <u>cevaplamayınız.</u>";
                var result = _mailService.SendMail(mailList, "Soru/Öneri Formu Hk.", message);
                return Json(result);
            }
            else
                return Json(false);

        }
    }
}
