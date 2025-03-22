using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;


namespace CoreArchV2.Services.Services
{
    public class OutOfHourService : IOutOfHourService
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehicleCoordinate> _vehicleCoordinateRepository;
        private readonly IGenericRepository<VehicleOperatingReport> _vehicleOperationRepository;
        private readonly IGenericRepository<VehicleOperatingReportParam> _vehicleOperationParamRepository;
        public OutOfHourService(IUnitOfWork uow)
        {
            _uow = uow;
            _userRepository = uow.GetRepository<User>();
            _tripRepository = uow.GetRepository<Trip>();
            _unitRepository = uow.GetRepository<Unit>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _vehicleCoordinateRepository = uow.GetRepository<VehicleCoordinate>();
            _vehicleOperationRepository = uow.GetRepository<VehicleOperatingReport>();
            _vehicleOperationParamRepository = uow.GetRepository<VehicleOperatingReportParam>();
        }

        #region Mesai içi/dışı sayfası
        public PagedList<EOutOfHourDto> GetAllWithPaged(int? page, EOutOfHourDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from vo in _vehicleOperationRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on vo.VehicleId equals v.Id
                        join unit in _unitRepository.GetAll() on vo.LastUnitId equals unit.Id into unitL
                        from unit in unitL.DefaultIfEmpty()
                        join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                        from unit2 in unit2L.DefaultIfEmpty()
                        where vo.Status
                        select new EOutOfHourDto
                        {
                            Id = vo.Id,
                            VehicleId = vo.VehicleId,
                            IlkSonKontakAcildi = "<span class='label bg-primary-300 full-width'>" + vo.IlkKontakAcildi + "<br/>" + vo.SonKontakKapandi + "</span>",
                            MesafeKm = vo.MesafeKm + " Km",
                            TypeName = vo.Type == 0 ? "<span class='label bg-primary-300 full-width'>" + vo.TypeName + "</span>" : "<span class='label bg-info-300 full-width'>" + vo.TypeName + "</span>",
                            Type = vo.Type,
                            UniqueLine = vo.UniqueLine,
                            MaxHiz = vo.MaxHiz + " Km/h",
                            StartDate = vo.StartDate,
                            EndDate = vo.EndDate,
                            Tarih = vo.Tarih,
                            UnitName = unit2.Name + "/" + unit.Name,
                            ArventoStartEndDate = "<span class='label bg-success-300 full-width'>" + (vo.StartDate != null ? vo.StartDate.Value.ToString("dd/MM/yyyy HH:mm") : "-") + "<br/>" + (vo.EndDate != null ? vo.EndDate.Value.ToString("dd/MM/yyyy HH:mm") : "-") + "</span>",
                            TripDescription = "",
                            DuraklamaSuresi = "<span class='label bg-orange-300 full-width'>" + vo.DuraklamaSuresiSaat + ":" + vo.DuraklamaSuresiDakika + ":" + vo.DuraklamaSuresiSaniye + "</span>",
                            RolantiSuresi = "<span class='label bg-slate-300 full-width'>" + vo.RolantiSuresiSaat + ":" + vo.RolantiSuresiDakika + ":" + vo.RolantiSuresiSaniye + "</span>",
                            HareketSuresi = "<span class='label bg-brown-300 full-width'>" + vo.HareketSuresiSaat + ":" + vo.HareketSuresiDakika + ":" + vo.HareketSuresiSaniye + "</span>",
                            KontakAcikKalmaSuresi = "<span class='label bg-grey-300 full-width'>" + vo.KontakAcikKalmaSuresiSaat + ":" + vo.KontakAcikKalmaSuresiDakika + ":" + vo.KontakAcikKalmaSuresiSaniye + "</span>",
                            LastDebitId = vo.LastDebitUserId,
                            LastDebitStatus = vo.LastDebitStatus,
                            UnitId = unit.Id,
                            ParentUnitId = unit2.Id,
                            ArventoDebitPlateNo = vo.ArventoPlaka == vo.ZimmetliPlaka ? ("<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>")
                                                  : (vo.ZimmetliPlaka != "0" + vo.ArventoPlaka ? ("<span class='label bg-danger-300 full-width'>" + vo.ArventoPlaka + "/" + vo.ZimmetliPlaka + "(Plakalar Uyuşmuyor)</span>")
                                                  : ("<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>")),
                            CustomButton = "<li class='text-primary-400'><a data-toggle='modal' onclick='funcGetOutOfHoursMap(" + vo.Id + ");'><i class='icon-map'></i></a></li>",
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1
                        });

            if (!filterModel.IsAdmin)
            {
                var user = _userRepository.Find(filterModel.CreatedBy);
                var unit = _unitRepository.Find(user.UnitId.Value);
                list = unit.ParentId > 0 ? list.Where(w => w.ParentUnitId == unit.ParentId) : list.Where(w => w.ParentUnitId == unit.Id);
            }

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.Type > -1)
                list = list.Where(w => w.Type == filterModel.Type);


            if (filterModel.StartDate != null && filterModel.EndDate != null && filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != DateTime.MinValue)
                list = list.Where(w => w.Tarih >= filterModel.StartDate && w.Tarih <= filterModel.EndDate);

            var result = new PagedList<EOutOfHourDto>(list.OrderByDescending(o => o.EndDate), page, PagedCount.GridKayitSayisi);

            foreach (var item in result)
            {
                if (item.LastDebitId > 0)
                {
                    var debitUser = _userRepository.Find(item.LastDebitId.Value);
                    item.DebitTripUser += "<b>Zimmetli: " + debitUser.Name + " " + debitUser.Surname + "/" + debitUser.MobilePhone + "</b>";
                }
                else
                {
                    item.DebitTripUser += "<b>Zimmetli: ✘";
                    if (item.LastDebitStatus == (int)DebitState.Pool)
                        item.DebitTripUser = "<b style='color:red;'>Zimmetli: Havuzda</b>";
                    else if (item.LastDebitStatus == (int)DebitState.InService)
                        item.DebitTripUser = "<b style='color:red;'>Zimmetli: Serviste</b>";
                    else if (item.LastDebitStatus == (int)DebitState.Deleted)
                        item.DebitTripUser = "<b style='color:red;'>Zimmetli: Silinmiş</b>";
                }

                var tripList = _tripRepository.Where(f => f.Status && f.VehicleId == item.VehicleId && item.StartDate <= f.StartDate && f.StartDate <= item.EndDate).OrderByDescending(o => o.Id).ToList();
                if (tripList.Any())
                {
                    bool isMultipTrip = tripList.Count > 1;
                    int tripCount = 1;
                    foreach (var trip in tripList)
                    {
                        if (trip.EndDate == null)
                            item.TripDescription += $"<b>{trip.StartDate.ToString("dd/MM/yyyy HH:mm")}-Görev Kapatılmayı Bekliyor</b><br/>";
                        else
                        {
                            item.TripDescription += $"<b>{trip.StartDate.ToString("dd/MM/yyyy HH:mm")}-{trip.EndDate.Value.ToString("dd/MM/yyyy HH:mm")}<br/>";
                            item.TripKm = (Convert.ToInt32(item.TripKm) + Convert.ToInt32(trip.EndKm.Value - trip.StartKm)).ToString();
                        }

                        var tripUser = _userRepository.Find(trip.DriverId);
                        if (trip.DriverId != item.LastDebitId)
                            item.DebitTripUser += "<br/><b style='color:red;'>Görev Açan: " + (isMultipTrip ? "-" + (tripCount + " ") : "") + (tripUser.Name + " " + tripUser.Surname + "/" + tripUser.MobilePhone) + "</b>";
                        else
                            item.DebitTripUser += "<br/><b>Görev Açan: " + (isMultipTrip ? "-" + (tripCount + " ") : "") + (tripUser.Name + " " + tripUser.Surname + "/" + tripUser.MobilePhone) + "</b>";

                        tripCount++;
                    }

                    item.TripKm += Convert.ToInt32(item.TripKm) > 0 ? " Km/h" : "";
                }
                else
                {
                    item.TripDescription = "<b style='color:red'>✘</b><br/>";
                    item.DebitTripUser += "<br/><b>Görev Açan: ✘";
                    item.TripKm = "-";
                }
            }

            return result;
        }
        public async Task<List<EOutOfHourDto>> GetAllList(EOutOfHourDto filterModel)
        {
            var list = await Task.FromResult((from vo in _vehicleOperationRepository.GetAll()
                                              join v in _vehicleRepository.GetAll() on vo.VehicleId equals v.Id
                                              join unit in _unitRepository.GetAll() on vo.LastUnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()
                                              where vo.Status
                                              select new EOutOfHourDto
                                              {
                                                  Id = vo.Id,
                                                  VehicleId = vo.VehicleId,
                                                  IlkSonKontakAcildi = vo.IlkKontakAcildi + "-" + vo.SonKontakKapandi,
                                                  MesafeKm = vo.MesafeKm + " Km",
                                                  TypeName = vo.Type == 0 ? vo.TypeName : vo.TypeName,
                                                  Type = vo.Type,
                                                  UniqueLine = vo.UniqueLine,
                                                  MaxHiz = vo.MaxHiz + " Km/h",
                                                  StartDate = vo.StartDate,
                                                  EndDate = vo.EndDate,
                                                  Tarih = vo.Tarih,
                                                  ArventoStartEndDate = (vo.StartDate != null ? vo.StartDate.Value.ToString("dd/MM/yyyy HH:mm") : "-") + "-" + (vo.EndDate != null ? vo.EndDate.Value.ToString("dd/MM/yyyy HH:mm") : "-"),
                                                  TripDescription = "",
                                                  DuraklamaSuresi = vo.DuraklamaSuresiSaat + ":" + vo.DuraklamaSuresiDakika + ":" + vo.DuraklamaSuresiSaniye,
                                                  RolantiSuresi = vo.RolantiSuresiSaat + ":" + vo.RolantiSuresiDakika + ":" + vo.RolantiSuresiSaniye,
                                                  HareketSuresi = vo.HareketSuresiSaat + ":" + vo.HareketSuresiDakika + ":" + vo.HareketSuresiSaniye,
                                                  KontakAcikKalmaSuresi = vo.KontakAcikKalmaSuresiSaat + ":" + vo.KontakAcikKalmaSuresiDakika + ":" + vo.KontakAcikKalmaSuresiSaniye,
                                                  LastDebitId = vo.LastDebitUserId,
                                                  LastDebitStatus = vo.LastDebitStatus,
                                                  UnitId = unit.Id,
                                                  ParentUnitId = unit2.Id,
                                                  ArventoDebitPlateNo = vo.ArventoPlaka == vo.ZimmetliPlaka ? v.Plate
                                                  : ("0" + vo.ZimmetliPlaka != vo.ArventoPlaka ? vo.ArventoPlaka + "/" + vo.ZimmetliPlaka + "(Plakalar Uyuşmuyor)" : vo.ArventoPlaka)
                                              }));

            if (!filterModel.IsAdmin)
            {
                var user = _userRepository.Find(filterModel.CreatedBy);
                var unit = _unitRepository.Find(user.UnitId.Value);
                list = unit.ParentId > 0 ? list.Where(w => w.ParentUnitId == unit.ParentId) : list.Where(w => w.ParentUnitId == unit.Id);
            }

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.Type > -1)
                list = list.Where(w => w.Type == filterModel.Type);

            if (filterModel.StartDate != null && filterModel.EndDate != null && filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != DateTime.MinValue)
                list = list.Where(w => w.Tarih >= filterModel.StartDate && w.Tarih <= filterModel.EndDate);

            return list.OrderByDescending(o => o.Id).ToList();
        }
        public async Task<List<EGeneralReport2Dto>> GetOutOfHourMap(int vecOperationId)
        {
            var vehicleOperationg = await _vehicleOperationRepository.FirstOrDefaultAsync(f => f.Id == vecOperationId);
            var startDate = vehicleOperationg.StartDate;
            var endDate = vehicleOperationg.EndDate;


            var mapList = await Task.FromResult((from c in _vehicleCoordinateRepository.GetAll()
                                                 where c.LocalDate >= startDate && c.LocalDate <= endDate
                                                 && c.VehicleId == vehicleOperationg.VehicleId && c.Id % 2 == 0
                                                 select new EGeneralReport2Dto()
                                                 {
                                                     Enlem = c.Latitude,
                                                     Boylam = c.Longitude,
                                                     Hiz = c.Speed,
                                                     Surucu = c.Driver,
                                                     Tarih = c.LocalDate
                                                 }).Distinct().OrderBy(o => o.Tarih).ToList());

            return mapList;
        }
        #endregion


        #region Parametre sayfası
        public PagedList<EOutOfHourDto> GetAllParamOutOfHourWithPaged(int? page, EOutOfHourDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from vo in _vehicleOperationParamRepository.GetAll()
                        join unit in _unitRepository.GetAll() on vo.UnitId equals unit.Id into unitL
                        from unit in unitL.DefaultIfEmpty()
                        join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                        from unit2 in unit2L.DefaultIfEmpty()
                        where vo.Status
                        select new EOutOfHourDto
                        {
                            Id = vo.Id,
                            UnitId = unit2.Name != null ? unit2.Id : unit.Id,
                            UnitName = unit2.Name != null ? ("<span class='label bg-green'>" + unit2.Name + "/" + unit.Name + "</span>") : "<span class='label bg-green'>" + unit.Name + "</span>",
                            CustomButton = "<li class='text-danger'><a data-toggle='modal' onclick='funcGetOutOfParamDelete(" + vo.Id + ");'><i class='icon-trash'></i></a></li>",
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1
                        }); ;

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            return new PagedList<EOutOfHourDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
        }

        public EResultDto ParamOutOfHourDelete(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _vehicleOperationParamRepository.Find(id);
                if (entity != null)
                {
                    entity.Status = false;
                    _vehicleOperationParamRepository.Update(entity);
                    _uow.SaveChanges();
                    result.Message = "İşlem başarılı";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Kayıt bulunamadı";
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }

        public EResultDto ParamOutOfHourSave(VehicleOperatingReportParam entity)
        {
            var result = new EResultDto();
            try
            {
                var unit = _unitRepository.FirstOrDefault(f => f.Id == entity.UnitId);
                if (unit == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Birim ya da proje seçiniz";
                    return result;
                }

                var parentId = unit.ParentId;
                var isRecord = _vehicleOperationParamRepository.FirstOrDefault(w => w.Status && w.UnitId == entity.UnitId);
                if (isRecord != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu birim zaten bulunmaktadır.";
                    return result;
                }

                if (parentId > 0)//proje ekleniyor, müdürlüğü ekli mi?
                {
                    var mudurlukVarMi = _vehicleOperationParamRepository.FirstOrDefault(f => f.Status && f.UnitId == parentId);
                    if (mudurlukVarMi != null)
                    {
                        result.IsSuccess = false;
                        result.Message = "Bu projenin daha önce <b>müdürlüğü</b> eklenmiş, müdürlüğü silip tekrar deneyiniz. (Not: Müdürlük eklendiğinde altındaki tüm projeler, bildirime kapatılmış olur.)";
                        return result;
                    }
                }
                else if (parentId == null)//Müdürlük ekleniyor, alt projesi ekli mi?
                {
                    var tumKayitlar = (from vo in _vehicleOperationParamRepository.GetAll()
                                       join un in _unitRepository.GetAll() on vo.UnitId equals un.Id into unitL
                                       from un in unitL.DefaultIfEmpty()
                                       join un2 in _unitRepository.GetAll() on un.ParentId equals un2.Id into unit2L
                                       from un2 in unit2L.DefaultIfEmpty()
                                       where vo.Status
                                       select new Unit
                                       {
                                           Id = un.Id,
                                           ParentId = un2.Id,
                                       }).ToList();

                    var projeVarMi = tumKayitlar.Where(f => f.ParentId == unit.Id).ToList();
                    if (projeVarMi.Count > 0)
                    {
                        result.IsSuccess = false;
                        result.Message = "Bu müdürlüğün altında bulunan " + projeVarMi.Count + " adet <b>proje</b> daha önce eklenmiş, bu müdürlüğe bağlı tüm projeleri silip tekrar deneyiniz.";
                        return result;
                    }
                }

                _vehicleOperationParamRepository.Insert(entity);
                _uow.SaveChanges();
                result.Message = "İşlem başarılı";

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }

        public List<EUnitDto> ComboParamOutOfUnitList()
        {
            var activeParam = _vehicleOperationParamRepository.Where(w => w.Status).Select(s => s.UnitId).ToList();
            var allUnit = GetAllUnit();

            //var differentList = allUnit.Select(s => s.Id).ToList().Except(activeParam).ToList();
            //var comboList = allUnit.Where(w => differentList.Contains(w.Id)).ToList();

            var newList = new List<EUnitDto>();
            foreach (var item in allUnit)
            {
                var parent = activeParam.FirstOrDefault(f => f == item.ParentId || f == item.Id);
                if (parent <= 0)
                    newList.Add(item);
            }

            return newList;
        }

        public List<EUnitDto> GetAllUnit()
        {
            var list = (from u in _unitRepository.GetAll()
                        where u.ParentId == null && u.Status
                        select new EUnitDto
                        {
                            Id = u.Id,
                            ParentId = u.ParentId,
                            Name = u.Name,
                        }).ToList();

            var list2 = (from u in _unitRepository.GetAll()
                         join u2 in _unitRepository.GetAll() on u.Id equals u2.ParentId
                         where u.Status && u2.Status
                         select new EUnitDto
                         {
                             Id = u2.Id,
                             ParentId = u2.ParentId,
                             Name = u.Name + " - " + u2.Name,
                             IsTenderVisible = u2.IsTenderVisible
                         }).ToList();

            return list.Concat(list2).Distinct().OrderBy(o => o.Name).ToList();
        }
        #endregion
    }
}
