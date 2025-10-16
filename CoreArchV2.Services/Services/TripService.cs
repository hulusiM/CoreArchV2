using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.TripVehicle;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class TripService : ITripService
    {
        private readonly FirmSetting _firmSetting;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IArventoService _arventoService;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<TripLog> _tripLogRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<VehicleCoordinate> _vehicleCoordinateRepository;
        private readonly IGenericRepository<VehicleDebit> _vehicleDebitRepository;
        private readonly IMailService _mailService;

        public TripService(IUnitOfWork uow,
            IReportService reportService,
            IArventoService arventoService,
            IMailService mailService,
            IOptions<FirmSetting> firmSetting,
            IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _mailService = mailService;
            _firmSetting = firmSetting.Value;
            _reportService = reportService;
            _arventoService = arventoService;
            _tripRepository = uow.GetRepository<Trip>();
            _tripLogRepository = uow.GetRepository<TripLog>();
            _cityRepository = uow.GetRepository<City>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _unitRepository = uow.GetRepository<Unit>();
            _userRepository = uow.GetRepository<User>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _vehicleCoordinateRepository = uow.GetRepository<VehicleCoordinate>();
            _vehicleDebitRepository = uow.GetRepository<VehicleDebit>();
        }

        #region TripAuthorization
        public async Task<PagedList<ETripDto>> GetAllAuthWithPaged(int? page, ETripDto filterModel, bool isAdmin)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = GetAllAuthTrip(page, pageStartCount, filterModel, isAdmin);

            PagedList<ETripDto> result = null;
            if (!isAdmin)//yetkili olduğu birimin araçları listelenmeli
            {
                var user = await _userRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == filterModel.CreatedBy);
                var unit = await _unitRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == user.UnitId.Value);
                var createdByList = unit.ParentId > 0 ? list.Where(w => w.ParentUnitId == unit.ParentId).ToList() : list.Where(w => w.ParentUnitId == unit.Id).ToList();

                var teamDriver = list.Where(w => w.UserUnitId == user.UnitId).ToList().Where(w => createdByList.All(a => a.Id != w.Id)).ToList();

                createdByList.AddRange(teamDriver);
                result = new PagedList<ETripDto>(createdByList, page, PagedCount.GridKayitSayisi);
            }
            else
                result = new PagedList<ETripDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);

            foreach (var item in result)
            {
                var lastDebit = await _vehicleDebitRepository
             .Where(w => w.VehicleId == item.VehicleId && w.State == (int)DebitState.Debit)
             .OrderByDescending(o => o.Id)
             .FirstOrDefaultAsync();
                //var vehicle = _vehicleRepository.Find(item.VehicleId);
                var plateUnit = await _unitRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == lastDebit.UnitId.Value);
                var userUnit = await _userRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == item.CreatedBy);
                var userParent = await _unitRepository.Where(w => w.Id == userUnit.UnitId).FirstOrDefaultAsync();
                if (userParent.ParentId != null)
                    userUnit.UnitId = userParent.ParentId;
                item.Plate = plateUnit.ParentId == userUnit.UnitId
                    ? ("<span class='label bg-primary-300'>" + item.Plate + "</span>")
                    : ("<span class='label bg-warning-300'>" + item.Plate + "-Farklı Birim</span>");

                item.ManagerAllowed = SetTripWithManagerState(item.State, item.IsManagerAllowed);
                item.StateName = SetStateTrip(item.State);
                item.MissionName = item.MissionName + " (" + GetTripType(item.Type) + ")";
            }

            return result;
        }
        public IQueryable<ETripDto> GetAllAuthTrip(int? page, int pageStartCount, ETripDto filterModel, bool isAdmin = false)
        {
            var list = (from t in _tripRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id

                        join user in _userRepository.GetAll() on t.DriverId equals user.Id
                        join userUnit in _unitRepository.GetAll() on user.UnitId equals userUnit.Id

                        join unit in _unitRepository.GetAll() on t.UnitId equals unit.Id
                        join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id

                        join c1 in _cityRepository.GetAll() on t.StartCityId equals c1.Id
                        join c1L in _cityRepository.GetAll() on c1.ParentId equals c1L.Id

                        join c2 in _cityRepository.GetAll() on t.EndCityId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        join c2P in _cityRepository.GetAll() on c2.ParentId equals c2P.Id into c2Pl
                        from c2P in c2Pl.DefaultIfEmpty()
                        where t.Status
                        select new ETripDto
                        {
                            Id = t.Id,
                            Plate = v.Plate + "-(" + user.Name + " " + user.Surname + ")",
                            Plate2 = v.Plate + "-(" + user.Name + " " + user.Surname + ")",
                            CreatedBy = t.CreatedBy,
                            DriverId = t.DriverId,
                            Status = t.Status,
                            State = t.State,
                            StartDate = t.StartDate,
                            Type = t.Type,
                            EndDate = t.EndDate,
                            UnitId = unit.Id,
                            ParentUnitId = unit2.Id,
                            UserUnitId = userUnit.Id,
                            VehicleId = t.VehicleId,
                            IsManagerAllowed = t.IsManagerAllowed,
                            StartCityName = c1L.Name + "-" + c1.Name,
                            StartKm = t.StartKm,
                            EndCityName = c2P.Name != null ? (c2P.Name + "-" + c2.Name) : "Devam ediyor",
                            //StateName = t.IsManagerAllowed == true ? ("<i style='color:blue;font-size: 18px;' class='icon-checkmark-circle2'></i>") : (t.IsManagerAllowed == false ? ("<i style='color:red;font-size: 18px;' class='icon-close2'></i>") : ("<span class='badge bg-orange-800 faa-flash animated faa-slow'>Onay Bekliyor</span>")),
                            MissionName = t.MissionName ?? ("Görev-" + t.Id),
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            CustomButton = "<li title='Onay Ver' class='text-primary-400'><a data-toggle='modal' onclick='funcAllowedTrip(" + t.Id + ");'><i class='icon-pencil5'></i></a></li>"
                        }).Distinct();

            if (filterModel.ManagerAllowedType > 0)
            {
                list = filterModel.ManagerAllowedType switch
                {
                    1 => list.Where(w => w.IsManagerAllowed == true),
                    2 => list.Where(w => w.IsManagerAllowed == false),
                    _ => list.Where(w => w.IsManagerAllowed == null)
                };
            }
            else
                list = list.Where(w => w.IsManagerAllowed == null);//Onay bekleyenleri listele

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.IsFilterMode && filterModel.DriverId > 0)
                list = list.Where(w => w.DriverId == filterModel.DriverId);

            if (filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != null)
                list = list.Where(w =>
                    (w.StartDate >= filterModel.StartDate && w.StartDate < filterModel.EndDate) &&
                    (w.EndDate >= filterModel.StartDate && w.EndDate < filterModel.EndDate));

            return list;
        }
        public string SetTripWithManagerState(int state, bool? isManagerAllowed)
        {
            return isManagerAllowed switch
            {
                null => "<span class='badge bg-orange-300 faa-flash animated faa-slow'>Yönetici Onayı Bekliyor</span>",
                true => "<span class='badge bg-success-300'>Onaylı</span>",
                false => "<span class='badge bg-warning-300'>Onaysız</span>"
            };
        }
        public EResultDto ChangeAllowedStatus(ETripDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var isBeforeCancelled = _tripLogRepository.Any(a => a.Status && a.TripId == model.Id && a.State == (int)TripState.NotAllowedForManager);
                var isBeforeConfirm = _tripLogRepository.Any(a => a.Status && a.TripId == model.Id && a.State == (int)TripState.AllowedForManager);

                if (isBeforeCancelled)
                    result.Message = "Görev daha önce iptal edilmiş, işleme kapalıdır";
                else if (isBeforeConfirm)
                    result.Message = "Göreve daha önce onay verilmiş, işleme kapalıdır";
                else
                {
                    var entity = _tripRepository.Find(model.Id);
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //trip update
                        entity.IsManagerAllowed = model.IsManagerAllowed;
                        entity.State = model.IsManagerAllowed == false ? (entity.State == (int)TripState.EndTrip ? entity.State : (int)TripState.CloseTrip) : entity.State;
                        _tripRepository.Update(entity);

                        //TripLog table
                        var tripLog = new TripLog()
                        {
                            CreatedBy = model.CreatedBy,
                            TripId = entity.Id,
                            TransactionDate = DateTime.Now,
                            Description = model.Description,
                            CityId = entity.StartCityId,
                            State = model.IsManagerAllowed == true ? (int)TripState.AllowedForManager : (int)TripState.NotAllowedForManager
                        };
                        _tripLogRepository.Insert(tripLog);

                        _uow.SaveChanges();
                        scope.Complete();
                        result.IsSuccess = true;
                        result.Message = "İşlem tamamlandı";
                    }

                    var user = _userRepository.Find(entity.CreatedBy);
                    if (model.IsManagerAllowed != null && user.Email != null)
                    {
                        var vehicle = _vehicleRepository.Find(entity.VehicleId);
                        var body = "Merhabalar,<br/>" + entity.CreatedDate + " tarihinde açmış olduğunuz görev, yöneticiniz tarafından " + (model.IsManagerAllowed == true ? "<b style='color:blue;'>onaylanmıştır. <i style='color:blue;font-size: 18px;' class='icon-checkmark-circle2'></i></b>" : "<b style='color:red;'>onaylanmamıştır. <i style='color:blue;font-size: 18px;' class='icon-close2'></i></b>");
                        var head = "Yönetici Onayı Hk.";
                        if (!model.IsManagerAllowed.Value)
                            head = "Yönetici İptali Hk.";

                        body += "<br /><br /><b><u>Görev İçeriği:</u></b>" +
                                  "<br />Plaka: " + vehicle.Plate +
                                  "<br />Görev Adı/Adresi: " + model.MissionName +
                                  "<br />Çıkış Yeri: " + GetCityName(entity.StartCityId) +
                                  "<br />İşlem Tarihi: " + DateTime.Now +
                                  "<br /><b>Yönetici Açıklama:</b> " + model.Description +
                                  "<br /><br />Görev detayına gitmek için için <a href='" + _firmSetting.WebSite + "Trip/Index?tripId=" + entity.Id + "'>Tıklayınız</a>";

                        Task.Run(() => _mailService.SendMail(user.Email, head, body));
                    }
                }
            }
            catch (Exception) { result.Message = "Hata oluştu-2"; }
            return result;
        }
        public EResultDto SetTripPassiveForManager(Trip entity)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var lastStartTripLog = GetLastTrip(entity.VehicleId);
                if (entity.Id == lastStartTripLog.Id)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //tripLog passive
                        var tripLog = _tripLogRepository.Where(w => w.TripId == entity.Id).ToList();
                        tripLog.ForEach(f => f.Status = false);
                        _tripLogRepository.UpdateRange(tripLog);
                        _uow.SaveChanges();

                        //Last km update
                        lastStartTripLog = GetLastTrip(entity.VehicleId);//son kayıtlar pasif edildikten sonraki ilk aktif kayıt geliyor
                        var vehicle = _vehicleRepository.Find(entity.VehicleId);
                        var startKm = lastStartTripLog?.StartKm ?? 0;
                        vehicle.LastKm = startKm;
                        _vehicleRepository.Update(vehicle);

                        var tripLogAgain = _tripLogRepository.Where(w => w.TripId == entity.Id).ToList();
                        tripLogAgain.ForEach(f => f.Status = true);
                        _tripLogRepository.UpdateRange(tripLogAgain);

                        //trip passive
                        if (entity.State != (int)TripState.EndTrip)
                        {
                            entity.IsManagerAllowed = false;
                            entity.State = (int)TripState.NotAllowedForManager;
                            _tripRepository.Update(entity);
                        }

                        _uow.SaveChanges();
                        scope.Complete();
                        result.Message = "İşlem başarılı";
                        result.IsSuccess = true;
                    }
                }
                else
                    result.Message = "Bu görev ilgili aracın son görevi <b>olmadığı</b> için bu işlem yapılamaz.";

            }
            catch (Exception e) { result.Message = "Silme sırasında hata oluştu"; }
            return result;
        }
        #endregion

        #region Trip
        public PagedList<ETripDto> GetAllWithPaged(int? page, ETripDto filterModel, bool isAdmin)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = GetAllTrip(page, pageStartCount, filterModel, isAdmin);
            var result = new PagedList<ETripDto>(list.OrderByDescending(o => o.Id).ThenBy(t => t.State), page, PagedCount.GridKayitSayisi);
            foreach (var item in result)
            {
                item.ManagerAllowed = SetTripWithManagerState(item.State, item.IsManagerAllowed);
                item.MissionName = item.MissionName + " ( " + GetTripType(item.Type) + ")";

                if (item.State == (int)TripState.NotAllowedForManager)
                    item.DeleteButtonActive = false;

                item.StateName = SetStateTrip(item.State);
                if ((item.State == (int)TripState.StartTrip || item.State == (int)TripState.AllowedForManager) && (item.EndDate == null))
                    item.CustomButton += "<li title='Adres Ekle' class='text-success-400'><a data-toggle='modal' onclick='funcAddCityForTrip(" + item.Id + ");'><i class='icon-city'></i></a></li>";

                item.InsertDeviceType = item.InsertType == (int)TripInsertType.Mobile ? "<i title='Bu görev mobil uygulamadan açılmıştır' style:'color:red;' class='icon-mobile'></i>" : "<i style:'color:blue;' title='Bu görev web uygulamadan açılmıştır' class='icon-screen3'></i>";
            }
            return result;
        }

        public IQueryable<ETripDto> GetAllTrip(int? page, int pageStartCount, ETripDto filterModel, bool isAdmin = false)
        {
            var list = (from t in _tripRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                        join un in _unitRepository.GetAll() on t.UnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join c1 in _cityRepository.GetAll() on t.StartCityId equals c1.Id
                        join c1L in _cityRepository.GetAll() on c1.ParentId equals c1L.Id
                        join c2 in _cityRepository.GetAll() on t.EndCityId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        join c2P in _cityRepository.GetAll() on c2.ParentId equals c2P.Id into c2Pl
                        from c2P in c2Pl.DefaultIfEmpty()
                        where t.Status
                        select new ETripDto
                        {
                            Id = t.Id,
                            Plate = v.Plate,
                            Plate2 = v.Plate,//"<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>",
                            CreatedBy = t.CreatedBy,
                            DriverId = t.DriverId,
                            Status = t.Status,
                            ParentUnitId = un1.Id,
                            SubUnitId = un.Id,
                            State = t.State,
                            UnitId = t.UnitId,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            Type = t.Type,
                            VehicleId = t.VehicleId,
                            UnitName = un1.Name + "/" + un.Name,
                            IsManagerAllowed = t.IsManagerAllowed,
                            StartCityName = c1L.Name + "-" + c1.Name,
                            EndCityName = c2P.Name != null ? (c2P.Name + "-" + c2.Name) : "Devam ediyor",
                            MissionName = t.MissionName ?? ("Görev-" + t.Id),
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            DeleteButtonActive = true,
                            InsertType = t.InsertType,
                            CustomButton = "<li title='Düzenle' class='text-primary-400'><a data-toggle='modal' onclick='funcEditTrip(" + t.Id + ");'><i class='icon-pencil5'></i></a></li>"
                        }).Distinct();

            if (filterModel.State > 0)
            {
                if (filterModel.State == (int)TripState.StartTrip)
                    list = list.Where(w => w.State == (int)TripState.StartTrip);
                else if (filterModel.State == (int)TripState.EndTrip)
                    list = list.Where(w => w.State == (int)TripState.EndTrip);
            }

            if (filterModel.ManagerAllowedType > 0)
            {
                list = filterModel.ManagerAllowedType switch
                {
                    1 => list.Where(w => w.IsManagerAllowed == true),
                    2 => list.Where(w => w.IsManagerAllowed == false),
                    3 => list.Where(w => w.IsManagerAllowed == null),
                    _ => list.Where(w => w.IsManagerAllowed == null)
                };
            }

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            if (!isAdmin)
            {
                var user = _userRepository.Find(filterModel.CreatedBy);
                if (user.Flag == (int)Flag.Manager)//Müdürse alttaki çalışanları görebilsin
                {
                    var unit = _unitRepository.Find(user.UnitId.Value);
                    if (unit.ParentId > 0)//Proje bazlı müdür
                        list = list.Where(w => w.SubUnitId == user.UnitId.Value || w.DriverId == user.Id);
                    else//Müdürlük bazlı müdür
                        list = list.Where(w => w.ParentUnitId == user.UnitId.Value || w.DriverId == user.Id);
                }
                else
                    list = list.Where(w => w.DriverId == filterModel.CreatedBy);
            }

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.IsFilterMode && filterModel.DriverId > 0)
                list = list.Where(w => w.DriverId == filterModel.DriverId);

            if (filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != null)
                list = list.Where(w =>
                    (w.StartDate >= filterModel.StartDate && w.StartDate < filterModel.EndDate) &&
                    (w.EndDate >= filterModel.StartDate && w.EndDate < filterModel.EndDate));

            return list;
        }

        public List<ETripDto> GetAllTrip(ETripDto filterModel)
        {
            //Trip unit çalıştırma, manuel
            //var alltrip = _tripRepository.Where(w => w.Status && w.UnitId == null).ToList();
            //var allDebit = _vehicleDebitRepository.Where(w => w.Status).ToList();

            //var date = DateTime.Now;
            //foreach (var item in alltrip)
            //{
            //    var vehicleDebit = allDebit.FirstOrDefault(f => f.Status && f.VehicleId == item.VehicleId && f.StartDate <= item.StartDate && item.StartDate < (f.EndDate == null ? date : f.EndDate));
            //    if (vehicleDebit != null)
            //    {
            //        item.UnitId = vehicleDebit.UnitId;
            //    }
            //}

            //_tripRepository.UpdateRange(alltrip);
            //_uow.SaveChanges();




            var list = (from t in _tripRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                        join un in _unitRepository.GetAll() on t.UnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join c1 in _cityRepository.GetAll() on t.StartCityId equals c1.Id
                        join c1L in _cityRepository.GetAll() on c1.ParentId equals c1L.Id
                        join c2 in _cityRepository.GetAll() on t.EndCityId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        join c2P in _cityRepository.GetAll() on c2.ParentId equals c2P.Id into c2Pl
                        from c2P in c2Pl.DefaultIfEmpty()

                        join u in _userRepository.GetAll() on t.DriverId equals u.Id
                        where t.Status
                        select new ETripDto
                        {
                            Id = t.Id,
                            Plate = v.Plate,
                            CreatedBy = t.CreatedBy,
                            NameSurname = u.Name + " " + u.Surname + "/" + u.MobilePhone,
                            DriverId = t.DriverId,
                            Status = t.Status,
                            ParentUnitId = un1.Id,
                            SubUnitId = un.Id,
                            State = t.State,
                            UnitId = t.UnitId,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            StartKm = t.StartKm,
                            EndKm = t.EndKm,
                            Type = t.Type,
                            VehicleId = t.VehicleId,
                            IsManagerAllowed = t.IsManagerAllowed,
                            StartCityName = c1L.Name + "-" + c1.Name,
                            EndCityName = c2P.Name != null ? (c2P.Name + "-" + c2.Name) : "Devam ediyor",
                            MissionName = t.MissionName ?? ("Görev-" + t.Id),
                            InsertType = t.InsertType,
                        }).Distinct();

            if (filterModel.State > 0)
            {
                if (filterModel.State == (int)TripState.StartTrip)
                    list = list.Where(w => w.State == (int)TripState.StartTrip);
                else if (filterModel.State == (int)TripState.EndTrip)
                    list = list.Where(w => w.State == (int)TripState.EndTrip);
            }

            if (filterModel.ManagerAllowedType > 0)
            {
                list = filterModel.ManagerAllowedType switch
                {
                    1 => list.Where(w => w.IsManagerAllowed == true),
                    2 => list.Where(w => w.IsManagerAllowed == false),
                    3 => list.Where(w => w.IsManagerAllowed == null),
                    _ => list.Where(w => w.IsManagerAllowed == null)
                };
            }

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != null)
                list = list.Where(w =>
                (w.StartDate >= filterModel.StartDate && w.StartDate < filterModel.EndDate) &&
                (w.EndDate >= filterModel.StartDate && w.EndDate < filterModel.EndDate));

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            if (filterModel.CreatedBy > 0)
                list = list.Where(w => w.CreatedBy == filterModel.CreatedBy);

            return list.OrderByDescending(o => o.Id).ToList();
        }
        public string SetStateTrip(int state)
        {
            var result = state switch
            {
                (int)TripState.StartTrip => "Görev Başladı",
                (int)TripState.EndTrip => "Görev Bitti",
                (int)TripState.AllowedForManager => "Yönetici Onay Verdi",
                (int)TripState.NotAllowedForManager => "Yönetici İptal Etti",
                (int)TripState.CloseTrip => "İşleme Kapalı",
                _ => "Tip bulunamadı",
            };
            //var result = state switch
            //{
            //    (int)TripState.StartTrip => "<span class='label bg-primary-300 full-width faa-flash animated faa-slow'>Görev Başladı</span>",
            //    (int)TripState.EndTrip => "<span class='label bg-success-300 full-width'>Görev Bitti</span>",
            //    (int)TripState.AllowedForManager => "<span class='label bg-success-300 full-width'>Yönetici Onay Verdi</span>",
            //    (int)TripState.NotAllowedForManager => "<span class='label bg-warning-300 full-width'>Yönetici İptal Etti</span>",
            //    (int)TripState.CloseTrip => "<span class='label bg-warning-300 full-width'>İşleme Kapalı</span>",
            //    _ => "<span class='label bg-success-300 full-width'>??</span>",
            //};
            return result;
        }
        public EResultDto UpdateVehicleKm(ETripDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                if (model.IsAdmin)
                {
                    var vehicleEnt = _vehicleRepository.FindForInsertUpdateDelete(model.VehicleId);
                    if (vehicleEnt.LastKm == null)
                    {
                        result.Message = "Bu araç daha önce göreve çıkmadığı için km değişimi yapılamaz";
                        return result;
                    }
                    if (vehicleEnt.LastKm != null && vehicleEnt.LastKm == model.StartKm)
                    {
                        result.Message = "Aynı km değeri girilemez";
                        return result;
                    }
                    //var getLastTrip = _tripRepository.Where(f => f.Status && f.VehicleId == model.VehicleId && f.IsManagerAllowed != false).LastOrDefault();
                    var getLastTrip = _tripRepository.Where(f => f.Status && f.VehicleId == model.VehicleId && f.IsManagerAllowed != false && f.EndDate != null).LastOrDefault();
                    if (getLastTrip is { Status: true, State: (int)TripState.StartTrip })
                    {
                        result.Message = "Görevdeki aracın km değeri değiştirilemez";
                        return result;
                    }

                    if (getLastTrip != null && model.StartKm < vehicleEnt.LastKm && getLastTrip.StartKm >= model.StartKm)
                    {
                        var driver = _userRepository.Find(getLastTrip.DriverId);
                        string mess = "Bu aracın son görev başlangıç km değeri:" + getLastTrip.StartKm + "<br/> Bu değerin altında olamaz. Son görevi silip öyle deneyiniz<br/> Son görev kullanıcısı: " + driver.Name + " " + driver.Surname;
                        result.Message = mess;
                        return result;
                    }

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        getLastTrip.EndKm = model.StartKm;
                        _tripRepository.Update(getLastTrip);

                        //TripLog table
                        if (getLastTrip != null)
                        {
                            var tripLog = new TripLog()
                            {
                                CreatedBy = model.CreatedBy,
                                TripId = getLastTrip.Id,
                                Km = model.StartKm - vehicleEnt.LastKm,
                                TransactionDate = DateTime.Now,
                                Description = model.Description,
                                State = (int)TripState.ChangeKm
                            };
                            _tripLogRepository.Insert(tripLog);
                        }

                        //vehicle table
                        vehicleEnt.LastKm = model.StartKm > vehicleEnt.LastKm ? (vehicleEnt.LastKm + (model.StartKm - vehicleEnt.LastKm)) : (vehicleEnt.LastKm - (vehicleEnt.LastKm - model.StartKm));
                        _vehicleRepository.Update(vehicleEnt);

                        _uow.SaveChanges();
                        scope.Complete();
                        result.Message = "Km güncellenmiştir";
                        result.IsSuccess = true;
                    }
                }
                else
                    result.Message = "Bu işleme yetkiniz bulunmamaktadır";
            }
            catch (Exception e) { result.Message = "Kayıt sırasında hata oluştu"; }
            return result;
        }
        public async Task<EVehicleDto> GetVehicleLastKm(int vehicleId)
        {
            var vehicle = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Status && f.Id == vehicleId);
            var result = new EVehicleDto();
            if (vehicle is { LastKm: { } })
                result = await GetLastTripInfo(vehicleId);
            else
            {
                var lastTrip = await _tripRepository.Where(w => w.Status && w.VehicleId == vehicleId).OrderByDescending(o => o.Id).Take(1).FirstOrDefaultAsync();
                if (lastTrip != null)
                {
                    bool isChange = false;
                    if (lastTrip.State == (int)TripState.StartTrip)
                    {
                        vehicle.LastKm = lastTrip.StartKm;
                        isChange = true;
                    }
                    else if (lastTrip.State == (int)TripState.EndTrip)
                    {
                        vehicle.LastKm = lastTrip.EndKm;
                        isChange = true;
                    }

                    if (isChange)
                    {
                        _vehicleRepository.Update(vehicle);
                        _uow.SaveChanges();
                        result = await GetLastTripInfo(vehicleId);
                    }
                }
            }
            return result;
        }

        public async Task<EVehicleDto> GetLastTripInfo(int vehicleId)
        {
            var result = new EVehicleDto();
            var vehicle = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Status && f.Id == vehicleId);
            var lastTrip = GetLastTripFinishedMission(vehicleId);
            if (lastTrip != null)
            {
                var user = _userRepository.Find(lastTrip.DriverId);
                result.NameSurname = user.Name + " " + user.Surname + "/" + user.MobilePhone;
                if (vehicle.LastCityId > 0)
                    result.LastCityName = GetCityName(vehicle.LastCityId.Value);
            }
            result.LastKm = vehicle.LastKm;
            result.LastCityId = vehicle.LastCityId;

            return result;
        }

        public async Task<ETripDto> ActiveMissionControl(int driveId)
        {
            var activeTrip = await _tripRepository.FirstOrDefaultNoTrackingAsync(f =>
                f.Status && f.IsManagerAllowed != false && (f.State == (int)TripState.StartTrip || f.State == (int)TripState.AllowedForManager) && f.DriverId == driveId);
            if (activeTrip != null)
            {
                var map = _mapper.Map<ETripDto>(activeTrip);
                map.StartCityName = GetCityName(map.StartCityId);
                var vehicle = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == map.VehicleId);
                if (vehicle != null)
                    map.Plate = vehicle.Plate;
                return map;
            }
            else
                return new ETripDto();
        }
        public async Task<List<ETripDto>> GetByTripIdHistory(int tripId)
        {
            var list = await Task.FromResult((from tl in _tripLogRepository.GetAll()
                                              join t in _tripRepository.GetAll() on tl.TripId equals t.Id
                                              join u in _userRepository.GetAll() on t.DriverId equals u.Id
                                              join uc in _userRepository.GetAll() on tl.CreatedBy equals uc.Id
                                              join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id

                                              join unit in _unitRepository.GetAll() on u.UnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()

                                              join unit3 in _unitRepository.GetAll() on t.UnitId equals unit3.Id into unit3L
                                              from unit3 in unit3L.DefaultIfEmpty()
                                              join unit4 in _unitRepository.GetAll() on unit3.ParentId equals unit4.Id into unit4L
                                              from unit4 in unit4L.DefaultIfEmpty()

                                              join c1 in _cityRepository.GetAll() on tl.CityId equals c1.Id into c1L
                                              from c1 in c1L.DefaultIfEmpty()
                                              join c2 in _cityRepository.GetAll() on c1.ParentId equals c2.Id into c2L
                                              from c2 in c2L.DefaultIfEmpty()
                                              where tl.Status && tl.TripId == tripId
                                              select new ETripDto()
                                              {
                                                  Id = tl.Id,
                                                  StartCityName = c2.Name + "-" + c1.Name,
                                                  StartKm = tl.Km,
                                                  CreatedNameSurname = uc.Name + " " + uc.Surname,
                                                  StartDate = t.StartDate,
                                                  TransactionDate2 = tl.TransactionDate,
                                                  UnitName = unit2.Name != null ? (unit2.Name + "/" + unit.Name) : unit.Name,
                                                  Plate = v.Plate + " (" + unit4.Code + "/" + unit3.Code + ")",
                                                  State = tl.State,
                                                  Type = t.Type,
                                                  VehicleLastKm = v.LastKm,
                                                  LogType = tl.Type,
                                                  IsManagerAllowed = t.IsManagerAllowed,
                                                  Description = tl.Description,
                                                  NameSurname = u.Name + " " + u.Surname + " (" + u.MobilePhone + ")"
                                              }).OrderBy(o => o.Id).ToList());

            foreach (var item in list)
            {
                item.StateName = SetStateTrip(item.State);
                item.TypeName = GetTripType(item.Type);
                if (item.LogType != null)
                    item.LogTypeName = GetTripType(item.LogType.Value);
            }

            //Total km 
            var end = list.FirstOrDefault(f => f.State == (int)TripState.EndTrip);
            if (end != null)
            {
                var start = list.FirstOrDefault(f => f.State == (int)TripState.StartTrip);
                var changeKm = list.Where(f => f.State == (int)TripState.ChangeKm).Sum(s => s.StartKm);
                var totalKm = end.StartKm - start.StartKm.Value + changeKm;
                list.Add(new ETripDto()
                {
                    State = 0,
                    Plate = start.Plate,
                    TotalKm = totalKm.Value,
                    StartDate = start.StartDate,
                    EndDate = end.TransactionDate2,
                    VehicleLastKm = list[0].VehicleLastKm,
                    StartCityName = String.Join(" ➢➢➢ ", list.Where(w => w.StartCityName != "-").ToList().Select(s => s.StartCityName).ToArray()),
                    EndCityName = end.StartCityName
                });
            }

            return list;
        }

        public async Task<List<EGeneralReport2Dto>> GetByTripIdHistoryMap(int tripId)
        {
            var trip = _tripRepository.Find(tripId);
            var vehicle = _vehicleRepository.Find(trip.VehicleId);
            var startDate = trip.StartDate;
            var endDate = trip.EndDate;
            if (trip.EndDate == null)
                endDate = DateTime.Now;

            //endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            var mapList = new List<EGeneralReport2Dto>();
            var dbList = (from c in _vehicleCoordinateRepository.GetAll()
                          join v in _vehicleRepository.GetAll() on c.VehicleId equals v.Id
                          where v.Id == vehicle.Id &&
                          (c.LocalDate >= startDate && c.LocalDate <= endDate) && c.Id % 2 == 0
                          select new EGeneralReport2Dto()
                          {
                              Enlem = c.Latitude,
                              Boylam = c.Longitude,
                              Hiz = c.Speed,
                              Surucu = c.Driver,
                              Tarih = c.LocalDate
                          }).OrderBy(o => o.Tarih).ToList();

            if (dbList.Any())//db'de kayıt varsa onu al yoksa arvento'dan çek
                mapList = dbList;
            else
                mapList = await _arventoService.GeneralReport(startDate, endDate.Value, vehicle.ArventoNo);


            //var tempTripEndDate = trip.EndDate == null ? DateTime.Now : trip.EndDate;
            //foreach (var item in mapList)
            //{
            //    if (item.Tarih >= trip.StartDate && item.Tarih <= tempTripEndDate)
            //        item.IconUrl = "/leaflet/images/marker-icon-green.png";
            //}
            return mapList;
        }
        public async Task<EResultDto> CloseTrip(ETripDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var trip = await _tripRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == model.Id);
                var tripDayKm = Convert.ToInt32((DateTime.Now - trip.StartDate).TotalDays);
                tripDayKm = tripDayKm == 0 ? 1 : tripDayKm;
                tripDayKm *= 2000; //Günlük max 2000km

                if (tripDayKm < (model.EndKm.Value - trip.StartKm))
                {
                    result.IsSuccess = false;
                    result.Message = "Günlük max 2000 km yapılabilir";
                    return result;
                }


                if (!model.IsAdmin && trip.DriverId != model.CreatedBy)
                    result.Message = "Bu işleme yetkiniz bulunmamaktadır.";
                else if (trip.Status && trip.State == (int)TripState.EndTrip)
                    result.Message = "Görev zaten kapatılmıştır.";
                else if (trip.StartKm >= model.EndKm)
                    result.Message = "Bitiş km,başlangıç km'den büyük olmalıdır";
                else if (trip.State == (int)TripState.NotAllowedForManager)
                    result.Message = "Yönetici tarafından izin verilmeyen görev üzerinde değişiklik yapılamaz";
                else if (trip.StartDate > DateTime.Now)
                    result.Message = "Görev açılış tarihi: " + trip.StartDate.ToString("dd/MM/yyyy") + "<br/>Bu tarihten sonra kapatabilirsiniz";
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Trip table
                        trip.State = (int)TripState.EndTrip;
                        trip.EndKm = model.EndKm;
                        trip.EndDate = DateTime.Now;
                        trip.EndCityId = model.EndCityId;
                        _tripRepository.Update(trip);

                        //vehicle table
                        var vehicleEnt = _vehicleRepository.FindForInsertUpdateDelete(trip.VehicleId);
                        vehicleEnt.LastKm = model.EndKm;
                        vehicleEnt.LastCityId = model.EndCityId;
                        _vehicleRepository.Update(vehicleEnt);

                        //TripLog table
                        var tripLog = new TripLog()
                        {
                            CreatedBy = model.CreatedBy,
                            TripId = trip.Id,
                            Km = model.EndKm.Value,
                            TransactionDate = DateTime.Now,
                            CityId = model.EndCityId.Value,
                            Description = model.Description,
                            State = (int)TripState.EndTrip
                        };
                        await _tripLogRepository.InsertAsync(tripLog);
                        await _uow.SaveChangesAsync();
                        scope.Complete();
                        result.Message = "Görev kapatılmıştır";
                        result.IsSuccess = true;
                    }
                }
            }
            catch (Exception e) { result.Message = "Kayıt sırasında hata oluştu"; }
            return result;
        }
        public async Task<EResultDto> TripAddCity(ETripDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var trip = _tripRepository.Find(model.Id);
                if (!model.IsAdmin && trip.DriverId != model.CreatedBy)
                    result.Message = "Görevi açan kullanıcı değişiklik yapabilir.";
                else if (trip.Status && trip.State == (int)TripState.EndTrip)
                    result.Message = "Kapalı göreve adres eklenemez";
                else if (model.Type == (int)TripType.OffDuty && model.StartKm <= 0)
                    result.Message = "Görev dışı kullanımda <b>km</b> girilmesi gerekiyor";
                else
                {
                    var tripLog = new TripLog()
                    {
                        CreatedBy = model.CreatedBy,
                        TripId = trip.Id,
                        TransactionDate = DateTime.Now,
                        Type = model.Type,
                        Km = model.Type == (int)TripType.OffDuty ? model.StartKm : null,
                        CityId = model.StartCityId,
                        Description = model.Description,
                        State = (int)TripState.AddAddress
                    };
                    await _tripLogRepository.InsertAsync(tripLog);
                    await _uow.SaveChangesAsync();
                    result.Message = "Adres eklendi";
                    result.IsSuccess = true;
                }
            }
            catch (Exception e) { result.Message = "Kayıt sırasında hata oluştu"; }
            return result;
        }
        public async Task<EResultDto> TripInsert(ETripDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                //if (model.StartDate < DateTime.Now.Date)
                //{
                //    result.Message = "Geçmiş tarihe görev girilemez";
                //    return result;
                //}
                if (model.StartDate > DateTime.Now.AddDays(3).Date)
                {
                    result.Message = "En fazla, bugünden +3 gün sonrası için görev açılabilir";
                    return result;
                }

                var userActiveTrip = await _tripRepository.FirstOrDefaultNoTrackingAsync(a =>
                    a.Status && a.EndDate == null && (a.State == (int)TripState.StartTrip || a.State == (int)TripState.AllowedForManager) && a.DriverId == model.CreatedBy);
                var plateActiveTrip = await _tripRepository.FirstOrDefaultNoTrackingAsync(f =>
                    f.Status && f.State == (int)TripState.StartTrip && f.VehicleId == model.VehicleId);
                var trip = _mapper.Map<Trip>(model);

                if (userActiveTrip != null)
                {
                    var plate = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == userActiveTrip.VehicleId);
                    result.Message = "Üzerinizde " + plate.Plate + " plakalı araç " + userActiveTrip.StartDate.ToString("g") + " tarihinde açılmış görev bulunmaktadır";
                }
                else if (plateActiveTrip != null)
                {
                    var userTrip = await _userRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == plateActiveTrip.DriverId);
                    result.Message = "Bu plakalı araç " + userTrip.Name + " " + userTrip.Surname + " adlı kulanıcı tarafından görevdedir";
                }
                else
                {
                    //var getLastTrip = _tripRepository.Where(f => f.Status && f.VehicleId == model.VehicleId && f.IsManagerAllowed != false && f.EndDate != null).LastOrDefault();
                    var getLastTrip = _tripRepository.Where(f => f.Status && f.VehicleId == model.VehicleId && f.IsManagerAllowed != false && f.EndDate != null).OrderByDescending(f => f.EndDate).FirstOrDefault();
                    if (getLastTrip is { State: (int)TripState.StartTrip })//aracın ilk kaydı değil
                    {
                        result.Message = "Bu plaka zaten görevde";
                        return result;
                    }
                    else if (getLastTrip != null && getLastTrip.EndDate.Value.Date > model.StartDate)
                    {
                        result.Message = "Bu aracın son görevi " + getLastTrip.EndDate + " tarihinde sonlandığı için,bu tarihten sonra açılabilir";
                        return result;
                    }

                    var vehicleEnt = _vehicleRepository.FindForInsertUpdateDelete(model.VehicleId);
                    //if (vehicleEnt.LastKm != null && vehicleEnt.LastKm.Value != 0 && vehicleEnt.LastKm.Value != model.StartKm)
                    //{
                    //    result.Message = "Aracın son km değeri: <b>" + $"{vehicleEnt.LastKm.Value:0,0.00}" + "</b> <br/> Bu değerden az/fazla girilemez";
                    //    return result;
                    //}
                    //else if (vehicleEnt.LastCityId != null && vehicleEnt.LastCityId != trip.StartCityId)
                    //{
                    //    result.Message = "Aracın son görev şehri: <b>" + GetCityName(vehicleEnt.LastCityId.Value) + "</b> <br/>. Farklı il girilemez";
                    //    return result;
                    //}

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var dateNow = DateTime.Now;
                        var startDateAttach = model.StartDate.Date == dateNow.Date ? dateNow : model.StartDate;//bugünse saat ekle
                        //Trip table
                        trip.State = (int)TripState.StartTrip;
                        trip.Type = model.Type;
                        trip.DriverId = model.CreatedBy;
                        trip.CreatedBy = model.CreatedBy;
                        if (getLastTrip != null && getLastTrip?.EndKm > 0)
                            trip.StartKm = getLastTrip.EndKm.Value;
                        trip.StartDate = startDateAttach;
                        trip.UnitId = vehicleEnt.LastUnitId;
                        _tripRepository.Insert(trip);
                        _uow.SaveChanges();

                        //vehicle table
                        vehicleEnt.LastCityId = model.StartCityId;
                        _vehicleRepository.Update(vehicleEnt);

                        //TripLog table
                        var tripLog = new TripLog()
                        {
                            CreatedBy = model.CreatedBy,
                            TripId = trip.Id,
                            TransactionDate = startDateAttach,
                            Km = model.StartKm,
                            CityId = trip.StartCityId,
                            Description = model.Description,
                            State = (int)TripState.StartTrip
                        };
                        await _tripLogRepository.InsertAsync(tripLog);
                        result.Message = !SendMailInsertTrip(getLastTrip, trip, vehicleEnt) ? "Hata oluştu,yöneticiye onay maili gönderilemedi" : "İşlem Başarılı";
                        _uow.SaveChanges();
                        scope.Complete();
                        result.IsSuccess = true;
                    }
                }
            }
            catch (Exception e) { result.Message = "Kayıt sırasında hata oluştu"; }
            return result;
        }
        public async Task<EResultDto> TripUpdate(ETripDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var entity = await _tripRepository.FindAsync(model.Id);
                //if (model.StartDate < entity.CreatedDate)
                //{
                //    result.Message = "Bu görev " + entity.CreatedDate + " tarihinde açılmıştır.<br/>Bu yüzden önceki tarihe görev girilemez";
                //    return result;
                //}
                if (model.StartDate > entity.CreatedDate.AddDays(3).Date)
                {
                    result.Message = "Bu görev " + entity.CreatedDate + " tarihinde açılmıştır. Bu tarihten +3 gün sonrası için görev açılabilir";
                    return result;
                }

                if (entity.Type == model.Type && entity.MissionName == model.MissionName &&
                    entity.StartCityId == model.StartCityId && entity.StartKm == model.StartKm && entity.StartDate == model.StartDate)
                {
                    result.Message = "Herhangi bir değişiklik yapılmadı";
                    result.IsSuccess = true;
                }
                else
                {
                    var firstVehicle = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == entity.VehicleId);
                    var vehicleEnt = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == model.VehicleId);
                    var addressLog = await _tripLogRepository.AnyAsync(a =>
                        a.Status && a.TripId == model.Id && a.State == (int)TripState.AddAddress);
                    if (firstVehicle.Plate != vehicleEnt.Plate)
                    {
                        result.Message = "Plaka güncellenemez,görevi silip yeni görev açabilirsiniz";
                        return result;
                    }
                    else if (addressLog)
                    {
                        result.Message = "Göreve adres eklendiği için revize edilemez";
                        return result;
                    }
                    else if (entity.State == (int)TripState.EndTrip)
                    {
                        result.Message = "Kapalı görev revize edilemez";
                        return result;
                    }
                    else if (entity.IsManagerAllowed == false)
                    {
                        result.Message = "Yönetici tarafından iptal edilen görev revize edilemez";
                        return result;
                    }

                    var user = await _userRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == entity.CreatedBy);
                    var firstCity = await _cityRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == entity.StartCityId);
                    var lastCity = await _cityRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == model.StartCityId);

                    var body = "<b><u>Önceki Bilgiler:</u></b>" +
                               "<br />Adı Soyadı: " + user.Name + " " + user.Surname + " ➤➤➤ <a href='tel:" + user.MobilePhone + "'>Ara ☎</a>" +
                               "<br />Plaka: " + firstVehicle.Plate +
                               "<br />Görev Tipi: " + GetTripType(entity.Type) +
                               "<br />Görev Adı/Adresi: " + entity.MissionName +
                               "<br />Çıkış Yeri: " + GetCityName(entity.StartCityId) +
                               "<br />Çıkış Tarihi: " + entity.StartDate.ToString("dd/MM/yyyy") +
                               "<br />İşlem Tarihi: " + entity.CreatedDate +
                               "<br />Araç Km: " + entity.StartKm + "<br /><br />";
                    body += "<b><u>Güncel Bilgiler:</u></b>" +
                            "<br />Görev Tipi: " + (entity.Type == model.Type ? GetTripType(entity.Type) : ("<b style='color:blue'>" + GetTripType(model.Type) + "</b>")) +
                            "<br />Görev Adı/Adresi: " + (entity.MissionName == model.MissionName ? model.MissionName : ("<b style='color:blue'>" + model.MissionName + "</b>")) +
                            "<br />Çıkış Yeri: " + (lastCity.Name == firstCity.Name ? GetCityName(model.StartCityId) : ("<b style='color:blue'>" + GetCityName(model.StartCityId) + "</b>")) +
                            "<br />Çıkış Tarihi: " + (entity.StartDate == model.StartDate ? model.StartDate.ToString("dd/MM/yyyy") : ("<b style='color:blue'>" + model.StartDate.ToString("dd/MM/yyyy") + "</b>")) +
                            "<br />İşlem Tarihi: " + DateTime.Now +
                            "<br />Araç Km: " + (model.StartKm.Value == entity.StartKm ? model.StartKm.ToString() : ("<b style='color:blue'>" + model.StartKm.ToString() + "</b>")) +
                            "<br /><br /> Görevi onaylamak için <a href='" + _firmSetting.WebSite + "TripAuthorization?tripId=" + entity.Id + "'>Tıklayınız</a>";

                    var renew = _userRepository.Find(model.CreatedBy);
                    body += "<br />Güncelleme yapan kullanıcı: " + renew.Name + " " + renew.Surname + " ➤➤➤ <a href='tel:" + renew.MobilePhone + "'>Ara ☎</a>";
                    if (model.IsAdmin || entity.DriverId == model.CreatedBy)
                    {
                        var last10Trip = await GetLastTripTop10(model.VehicleId);
                        var lastTrip = last10Trip.Where(w => w.Id != entity.Id && w.State == (int)TripState.EndTrip).OrderByDescending(o => o.TripLogId).Take(1).FirstOrDefault();
                        if (entity.State == (int)TripState.EndTrip)
                            result.Message = "Görev tamamlanmıştır, düzenleme yapılamaz";
                        else if (entity.State == (int)TripState.NotAllowedForManager)
                            result.Message = "Yönetici tarafından izin verilmeyen görev üzerinde değişiklik yapılamaz";
                        else if (lastTrip != null && lastTrip.StartKm.Value > model.StartKm)
                            result.Message = "Aracın son görev km değeri: <b>" + $"{lastTrip.StartKm.Value:0,0.00}" + "</b> <br/> Bu değerden az/fazla girilemez";
                        //else if (lastTrip != null && lastTrip.EndDate >= model.StartDate)
                        //    result.Message = "Bu aracın son görevi tarihinde sonlandığı için,bu tarihten sonra görev açılabilir";
                        else
                        {
                            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //Trip table
                                var newEnt = _tripRepository.FindForInsertUpdateDelete(model.Id);
                                var driverId = newEnt.DriverId;
                                var trip = _mapper.Map(model, newEnt);
                                trip.DriverId = driverId;
                                trip.IsManagerAllowed = null;
                                trip.State = (int)TripState.StartTrip;
                                trip.StartDate = model.StartDate;
                                trip.UnitId = vehicleEnt.LastUnitId;
                                _tripRepository.Update(trip);

                                //TripLog table
                                var listD = await _tripLogRepository.Where(f => f.TripId == trip.Id).ToListAsync();
                                _tripLogRepository.DeleteRange(listD);
                                var tripLog = new TripLog()
                                {
                                    CreatedBy = model.CreatedBy,
                                    TripId = trip.Id,
                                    TransactionDate = DateTime.Now,
                                    Km = trip.StartKm,
                                    CityId = trip.StartCityId,
                                    Description = model.Description,
                                    State = (int)TripState.StartTrip
                                };
                                await _tripLogRepository.InsertAsync(tripLog);

                                result.Message = !SendMailForTrip(vehicleEnt, "Görev Revize Edildi", body) ? "Hata oluştu, yöneticiye onay maili gönderilemedi" : "İşlem Başarılı";
                                _uow.SaveChanges();
                                scope.Complete();
                                result.Message = "Kayıt başarıyla güncellenmiştir";
                                result.IsSuccess = true;
                            }
                        }
                    }
                    else
                        result.Message = "Görevi açan kullanıcı değişiklik yapabilir.";
                }
            }
            catch (Exception e) { result.Message = "Güncelleme sırasında hata oluştu"; }
            return result;
        }
        public bool SendMailInsertTrip(Trip getLastTrip, Trip trip, Vehicle vehicle)
        {
            var user = _userRepository.Find(trip.CreatedBy);
            var kmInfo = getLastTrip != null ? (trip.StartKm + " <b style='color:orange'>(" + (trip.StartKm - getLastTrip.EndKm) + " km fazla girildi!)</b>") : "";
            var body = "<b><u>Görev İçeriği:</u></b>" +
                       "<br />Adı Soyadı: " + user.Name + " " + user.Surname + " ➤➤➤ <a href='tel:" +
                       user.MobilePhone + "'>Ara ☎</a>" +
                       "<br />Plaka: " + vehicle.Plate +
                       "<br />Görev Tipi: " + GetTripType(trip.Type) +
                       "<br />Görev Adı/Adresi: " + trip.MissionName +
                       "<br />Çıkış Yeri: " + GetCityName(trip.StartCityId) +
                       "<br />Çıkış Tarihi: " + trip.StartDate.ToString("dd/MM/yyyy") +
                       "<br />İşlem Tarihi: " + DateTime.Now +
                       "<br />Araç Km: " + (getLastTrip != null ? ((trip.StartKm == getLastTrip.EndKm ? trip.StartKm.ToString() : kmInfo)) : trip.StartKm.ToString()) +
                       "<br /><br /> Görevi onaylamak için <a href='" + _firmSetting.WebSite + "Trip/TripAuthorization?tripId=" + trip.Id + "'>Tıklayınız</a>";

            return SendMailForTrip(vehicle, "Yeni Görev Talebi", body); ;
        }
        public string GetTripType(int type)
        {
            string result = type switch
            {
                (int)TripType.Mission => "Görev",
                (int)TripType.OffDuty => "Görev Dışı",
                _ => ""
            };
            return result;
        }
        public bool SendMailForTrip(Vehicle vehicle, string head, string body)
        {
            var result = true;
            try
            {
                if (vehicle.LastUnitId != null)
                {
                    var mailList = FindUnitManagerList(vehicle.LastUnitId.Value).Where(w => w.Email != null && w.IsSendMail).ToList();
                    if (!mailList.Any())//Eğer proje müdürü yoksa parentId değerindeki müdüre istek gönder
                    {
                        var unit = _unitRepository.Find(vehicle.LastUnitId.Value);
                        if (unit.ParentId > 0)
                            mailList = FindUnitManagerList(unit.ParentId.Value).Where(w => w.Email != null && w.IsSendMail).ToList();
                    }

                    if (mailList.Any()) //Birden fazla yönetici olabilir
                    {
                        var mailTos = String.Join(";", mailList.Select(s => s.Email).Distinct().ToList());
                        Task.Run(() => _mailService.SendMail(mailTos, head, body));
                    }
                }
            }
            catch (Exception e) { result = false; }
            return result;
        }
        public List<User> FindUnitManagerList(int unitId)
        {
            var list = (from u in _userRepository.GetAll()
                        join ur in _userRoleRepository.GetAll() on u.Id equals ur.UserId
                        where u.Status && u.UnitId == unitId && u.Flag == (int)Flag.Manager
                        select u).ToList();
            return list;
        }
        public async Task<ETripDto> GetById(int id)
        {
            var result = await (from t in _tripRepository.GetAll()
                                where t.Id == id
                                select new ETripDto()
                                {
                                    Id = t.Id,
                                    VehicleId = t.VehicleId,
                                    Type = t.Type,
                                    MissionName = t.MissionName,
                                    StartCityId = t.StartCityId,
                                    StartKm = t.StartKm,
                                    StartDate = t.StartDate
                                }).FirstOrDefaultAsync();

            if (result != null)
            {
                var last10Trip = await GetLastTripTop10(result.VehicleId);
                var lastTrip = last10Trip.Where(w => w.State == (int)TripState.EndTrip)
                    .OrderByDescending(o => o.TripLogId).Take(1).FirstOrDefault();
                result.IsPastRecord = lastTrip != null;
                result.StartCityName = GetCityName(result.StartCityId);
            }

            return result;
        }
        public async Task<EResultDto> Delete(int id, bool isAdmin, int createdBy)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var entity = _tripRepository.Find(id);
                if (isAdmin || entity.DriverId == createdBy)
                    result = SetTripPassive(entity, isAdmin);
                else
                    result.Message = "Görevi açan kullanıcı değişiklik yapabilir.";
            }
            catch (Exception) { result.Message = "Silme sırasında hata oluştu-2"; }
            return result;
        }
        public EResultDto SetTripPassive(Trip entity, bool isAdmin)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var lastStartTripLog = GetLastTrip(entity.VehicleId);
                if (entity.Id != lastStartTripLog.Id)
                    result.Message = "Bu görev, ilgili aracın son görevi olmadığı için <b>silinemez.</b>";
                else if (!isAdmin && entity.IsManagerAllowed != null)
                    result.Message = "Yönetici tarafından onay/ret verilen görev silinemez";
                else if (!isAdmin && entity.State == (int)TripState.EndTrip)
                    result.Message = "Kapalı görev silinemez";
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //tripLog passive
                        var tripLog = _tripLogRepository.Where(w => w.TripId == entity.Id).ToList();
                        tripLog.ForEach(f => f.Status = false);
                        _tripLogRepository.UpdateRange(tripLog);
                        _uow.SaveChanges();

                        //trip passive
                        entity.Status = false;
                        _tripRepository.Update(entity);
                        _uow.SaveChanges();

                        //Vehicle 
                        var lastTripFinished = GetLastTripFinishedMission(entity.VehicleId);
                        decimal kmEdit = (decimal)0;
                        if (lastTripFinished != null)
                            kmEdit = (decimal)_tripLogRepository.Where(w => w.Status && w.TripId == lastTripFinished.Id && w.State == (int)TripState.ChangeKm).Sum(s => s.Km);
                        var vehicle = _vehicleRepository.FindForInsertUpdateDelete(entity.VehicleId);
                        vehicle.LastKm = lastTripFinished != null ? (lastTripFinished.StartKm + kmEdit) : 0;
                        vehicle.LastCityId = lastTripFinished != null ? (lastTripFinished?.EndCityId) : null;
                        _uow.SaveChanges();

                        scope.Complete();
                        result.Message = "İşlem başarılı";
                        result.IsSuccess = true;
                    }
                }
            }
            catch (Exception e) { result.Message = "Silme sırasında hata oluştu"; }
            return result;
        }

        //Başla-Bitir Tipindekileri listeler
        public ETripDto GetLastTrip(int vehicleId)
        {
            var lastStartTripLog = (from t in _tripRepository.GetAll()
                                    join tl in _tripLogRepository.GetAll() on t.Id equals tl.TripId
                                    where t.Status && tl.Status && t.VehicleId == vehicleId
                                    && (tl.State == (int)TripState.StartTrip || tl.State == (int)TripState.EndTrip)
                                    select new ETripDto()
                                    {
                                        Id = t.Id,
                                        IsManagerAllowed = t.IsManagerAllowed,
                                        EndDate = t.EndDate,
                                        TripLogId = tl.Id,
                                        DriverId = t.DriverId,
                                        StartKm = tl.Km
                                    }).OrderByDescending(o => o.TripLogId).Take(1).FirstOrDefault();

            return lastStartTripLog;
        }

        //Bitmiş görevlerin sonuncusunu listeler (onaylı veya red)
        public ETripDto GetLastTripFinishedMission(int vehicleId)
        {
            var lastStartTripLog = (from t in _tripRepository.GetAll()
                                    join tl in _tripLogRepository.GetAll() on t.Id equals tl.TripId
                                    where t.Status && tl.Status && t.VehicleId == vehicleId
                                    && (tl.State == (int)TripState.StartTrip || tl.State == (int)TripState.EndTrip)
                                    //&& (t.IsManagerAllowed != false && t.EndDate != null)//yönetici tarafında ret edilen görevi getirme
                                    select new ETripDto()
                                    {
                                        Id = t.Id,
                                        IsManagerAllowed = t.IsManagerAllowed,
                                        EndDate = t.EndDate,
                                        TripLogId = tl.Id,
                                        StartCityId = t.StartCityId,
                                        EndCityId = t.EndCityId,
                                        DriverId = t.DriverId,
                                        StartKm = tl.Km
                                    }).OrderByDescending(o => o.TripLogId).Take(1).FirstOrDefault();

            return lastStartTripLog;
        }
        public async Task<List<ETripDto>> GetLastTripTop10(int vehicleId)
        {
            var list = (from t in _tripRepository.GetAll()
                        join tl in _tripLogRepository.GetAll() on t.Id equals tl.TripId
                        where t.Status && tl.Status && t.VehicleId == vehicleId && (tl.State == (int)TripState.StartTrip || tl.State == (int)TripState.EndTrip)
                        select new ETripDto()
                        {
                            Id = t.Id,
                            TripLogId = tl.Id,
                            State = tl.State,
                            StartKm = tl.Km
                        });

            return await list.OrderByDescending(o => o.TripLogId).Take(10).ToListAsync();
        }
        public async Task<List<ETripDto>> GetReport(ETripDto filterModel)
        {
            try
            {
                var tripList = await Task.FromResult((from t in _tripRepository.GetAll()
                                                      join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                                                      join c1 in _cityRepository.GetAll() on t.StartCityId equals c1.Id
                                                      join c1L in _cityRepository.GetAll() on c1.ParentId equals c1L.Id

                                                      join c2 in _cityRepository.GetAll() on t.EndCityId equals c2.Id
                                                      join c2P in _cityRepository.GetAll() on c2.ParentId equals c2P.Id

                                                      join user in _userRepository.GetAll() on t.CreatedBy equals user.Id

                                                      join unit in _unitRepository.GetAll() on t.UnitId equals unit.Id into unitL
                                                      from unit in unitL.DefaultIfEmpty()
                                                      join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                                      from unit2 in unit2L.DefaultIfEmpty()

                                                      where t.Status && t.State == (int)TripState.EndTrip
                                                      select new ETripDto()
                                                      {
                                                          Id = t.Id,
                                                          Plate = v.Plate,
                                                          CreatedBy = t.CreatedBy,
                                                          Status = t.Status,
                                                          State = t.State,
                                                          UnitName = unit2.Name,
                                                          UnitId = unit.Id,
                                                          ParentUnitId = unit2.Id,
                                                          StartDate = t.StartDate,
                                                          EndDate = t.EndDate,
                                                          VehicleId = t.VehicleId,
                                                          StartCityName = c1L.Name + "-" + c1.Name,
                                                          EndCityName = (c2P.Name + "-" + c2.Name),
                                                          MissionName = t.MissionName ?? ("Görev-" + t.Id),
                                                          CustomButton = "<a onclick='funcEditTrip(" + t.Id + ");'><i class='icon-search'></i></a>"
                                                      }).Where(w => (w.StartDate >= filterModel.StartDate && w.StartDate < filterModel.EndDate) &&
                                                                     (w.EndDate >= filterModel.StartDate && w.EndDate < filterModel.EndDate)).ToList());

                var unitList = await _reportService.ManagementVehicleCount(new RFilterModelDto() { });//Birimlerdeki araç sayıları

                var list = new List<ETripDto>();
                List<string> tripPlates = tripList.GroupBy(g => g.Plate).Select(s => s.Key).ToList();
                foreach (var item in unitList)
                {
                    var vehicleList = item.VehicleIds;
                    var plateList = tripList.Where(c => c.ParentUnitId == item.ParentUnitId).ToList();
                    var notOpenMissionList = item.Plates.Where(i => !tripPlates.Contains(i)).ToList();
                    var openMissionList = plateList.GroupBy(g => g.Plate).Select(s => s.Key).ToList();
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    foreach (var btn in notOpenMissionList)
                        sb.Append("<button onclick=btnChangebgColor(this) style='margin: 2px;' class='btn border-orange text-orange btn-flat btn-icon '>" + btn.Trim() + "</button>");
                    foreach (var btn in openMissionList)
                        sb2.Append("<button onclick=btnChangebgColor(this) style='margin: 2px;' class='btn border-orange text-orange btn-flat btn-icon '>" + btn.Trim() + "</button>");

                    list.Add(new ETripDto()
                    {
                        UnitName = item.UnitName,
                        DebitPlateCount = item.Count,
                        //OpenMissionCount = tripList.Where(w => w.ParentUnitId == item.ParentUnitId).ToList().GroupBy(g => g.VehicleId).Count(), //İlgili birimde açılan görev sayısı
                        PlateList = plateList,
                        NotOpenMissionList = notOpenMissionList,//görev açmayan liste
                        NotOpenPlateSplit = sb.ToString(),
                        OpenPlateSplit = sb2.ToString(),
                        OpenMissionList = openMissionList//görev açanlar
                    });
                }

                return list.OrderByDescending(o => o.OpenMissionList.Count).ToList();
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion


        public string GetCityName(int cityId)
        {
            var result = (from c1 in _cityRepository.GetAll()
                          join c2 in _cityRepository.GetAll() on c1.ParentId equals c2.Id
                          where c1.Id == cityId
                          select new ETripDto()
                          {
                              StartCityName = c2.Name + "-" + c1.Name
                          }).FirstOrDefault();

            if (result != null)
                return result.StartCityName;
            return "";
        }

    }
}
