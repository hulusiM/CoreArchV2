using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.TripVehicle;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    public class HangFireRunService : IHangFireRunService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMobileService _mobileService;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehiclePhysicalImage> _vehiclePhysicalImageRepository;
        public HangFireRunService(IUnitOfWork uow,
            IMobileService mobileService)
        {
            _uow = uow;
            _mobileService = mobileService;
            _userRepository = uow.GetRepository<User>();
            _tripRepository = uow.GetRepository<Trip>();
            _unitRepository = uow.GetRepository<Unit>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _messageRepository = uow.GetRepository<Message>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _vehiclePhysicalImageRepository = uow.GetRepository<VehiclePhysicalImage>();
        }

        //Kapatılmamış görevler için PN gönderir
        public async Task TripClosedControlAfterPushNotification()
        {
            if (new ModeDetector().IsDebug)
                return;

            try
            {
                var list = await Task.FromResult(
                    (from t in _tripRepository.GetAll()
                     join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                     join u in _userRepository.GetAll() on t.DriverId equals u.Id
                     where t.Status && t.State == (int)TripState.StartTrip
                     select new ETripDto()
                     {
                         Plate = v.Plate,
                         CreatedDate = t.StartDate,
                         NameSurname = u.Name + " " + u.Surname,
                         DriverId = t.DriverId,
                     }).ToList());

                foreach (var item in list)
                {
                    await _mobileService.PushNotificationAsync(new EMessageLogDto()
                    {
                        Subject = "Görev Kapatma Hatırlatma",
                        Body = "Sayın " + item.NameSurname + ", " + item.Plate + " plakalı araç için " + item.CreatedDate.ToString("dd-MM-yyyy") + " tarihinde açılan görevi kapatmayı unutmayınız.",
                        Type = (int)MessageLogType.PushNotification,
                        UserId = item.DriverId,
                    });
                }
            }
            catch (Exception) { }
        }

        //Mobilden resim göndermeyenlere pn+mail gönderir
        public async Task DontUploadPicturesSendPnMail()
        {
            try
            {
                DateTime today = DateTime.Now;
                DateTime month = new DateTime(today.Year, today.Month, 5);
                DateTime first = month.AddMonths(-1);
                DateTime last = month.AddDays(-1);

                var allVehicle = _vehicleRepository.Where(w => w.Status).ToList();

                var lastMonthList = (from v in _vehicleRepository.GetAll()
                                     join vp in _vehiclePhysicalImageRepository.GetAll() on v.Id equals vp.VehicleId
                                     where vp.CreatedDate >= first && vp.Status && v.Status
                                     select new EVehicleDto()
                                     {
                                         VehicleId = v.Id,
                                         DebitUserId = v.LastUserId,
                                         UnitId = v.LastUnitId,
                                         Plate = v.Plate,
                                         CreatedDate = vp.CreatedDate,
                                     }).ToList();

                var notLoadVehicleId = allVehicle.Select(s => s.Id).Except(lastMonthList.Select(s => s.VehicleId)).ToList();

                var notLoad = allVehicle.Where(w => notLoadVehicleId.Contains(w.Id) && w.LastUserId > 0).ToList();

                if (notLoad.Any())
                {
                    var allUser = _userRepository.Where(w => w.Status && !string.IsNullOrEmpty(w.Email)).ToList();
                    var allUnit = _unitRepository.Where(w => w.Status).ToList();

                    foreach (var item in notLoad)
                    {
                        try
                        {
                            var driveUser = allUser.FirstOrDefault(w => w.Id == item.LastUserId.Value);

                            if (driveUser == null) continue;

                            var subject = "Araç Fotoğraf Yükleme Hatırlatıcı";
                            var body = item.Plate + " plakalı aracın son ay fotoğraflarını yüklemeniz gerekiyor. Bugün son gün lütfen unutmayınız.";

                            #region PN

                            await Task.Run(() => _mobileService.PushNotificationAsync(new EMessageLogDto()
                            {
                                Subject = subject,
                                Body = body,
                                Type = (int)MessageLogType.PushNotification,
                                UserId = item.LastUserId.Value,
                            }));
                            #endregion

                            #region Mail
                            if (!string.IsNullOrEmpty(driveUser.Email))
                            {
                                string mailCc = "";
                                if (driveUser.Flag != (int)Flag.Manager && driveUser.UnitId > 0)
                                {
                                    var driveUnit = allUnit.FirstOrDefault(w => w.Id == driveUser.UnitId.Value); //kullanıcı birim

                                    if (driveUnit.ParentId > 0)
                                    {
                                        var manager = (from u in _userRepository.GetAll()
                                                       join ur in _userRoleRepository.GetAll() on u.Id equals ur.UserId
                                                       where u.Status &&
                                                       u.IsSendMail &&
                                                       u.UnitId == driveUnit.ParentId &&
                                                       u.Flag == (int)Flag.Manager &&
                                                       !string.IsNullOrEmpty(u.Email)
                                                       select new User() { Email = u.Email }).ToList();

                                        if (manager.Any())
                                            mailCc = String.Join(";", manager.Select(s => s.Email).Distinct().ToList());
                                    }
                                }

                                if (string.IsNullOrEmpty(mailCc)) //todo:silinecek
                                    mailCc = "mehmetpehlivan@basaranteknoloji.net";
                                else
                                    mailCc += ";mehmetpehlivan@basaranteknoloji.net";

                                await Task.Run(() => _mobileService.SendMailAsync(new EMessageLogDto()
                                {
                                    Subject = subject,
                                    Body = body,
                                    Type = (int)MessageLogType.EMail,
                                    Email = driveUser.Email,
                                    MailCc = mailCc,
                                }));
                            }
                        }
                        catch (Exception) { }
                        #endregion
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
