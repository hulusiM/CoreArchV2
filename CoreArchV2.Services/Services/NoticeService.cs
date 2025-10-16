using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.NoticeVehicle.Notice;
using CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.NoticeVehicle.Notice;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeUnitDto_;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Transactions;
using System.Web;

namespace CoreArchV2.Services.Services
{
    public class NoticeService : INoticeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<Notice> _noticeRepository;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<NoticeUnit> _noticeUnitRepository;
        private readonly IGenericRepository<NoticeUnitHistory> _noticeUnitHistoryRepository;
        private readonly IGenericRepository<NoticeSendUnit> _noticeSendUnitRepository;
        private readonly IGenericRepository<NoticePunishment> _noticePunishmentRepository;
        private readonly IGenericRepository<NoticeUnitFile> _noticeUnitFileRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IWebHostEnvironment _env;


        public NoticeService(IUnitOfWork uow,
            IWebHostEnvironment env,
            IMapper mapper)
        {
            _mapper = mapper;
            _uow = uow;
            _env = env;
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _noticeRepository = uow.GetRepository<Notice>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _cityRepository = uow.GetRepository<City>();
            _userRepository = uow.GetRepository<User>();
            _tripRepository = uow.GetRepository<Trip>();
            _noticeUnitRepository = uow.GetRepository<NoticeUnit>();
            _noticeUnitHistoryRepository = uow.GetRepository<NoticeUnitHistory>();
            _noticeSendUnitRepository = uow.GetRepository<NoticeSendUnit>();
            _noticePunishmentRepository = uow.GetRepository<NoticePunishment>();
            _noticeUnitFileRepository = uow.GetRepository<NoticeUnitFile>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _unitRepository = uow.GetRepository<Unit>();
        }
        #region Notice
        public PagedList<ENoticeDto> GetAllWithPaged(int? page, ENoticeDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from n in _noticeRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        where n.Status
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            CreatedDate = n.CreatedDate,
                            Plate = "<b>" + v.Plate + "</b>",
                            FirstRunEngineDate = n.FirstRunEngineDate,
                            TransactionDate = n.TransactionDate,
                            LastRunEngineDate = n.LastRunEngineDate,
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            Description = n.Description,
                            DeleteButtonActive = true,
                            State = n.State,
                            NoticeType = n.NoticeType,
                            CustomButton = "<li title='İhbar düzenle' class='text-primary'><a onclick='funcEditNotice(" + n.Id + ");'><i class='icon-pencil5'></i></a></li>",
                        });

            if (filterModel.UserId > 0)
                list = list.Where(w => w.UserId == filterModel.UserId);
            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);
            if (filterModel.TransactionDate != null)
                list = list.Where(w => w.TransactionDate == filterModel.TransactionDate);

            if (filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != DateTime.MinValue)
                list = list.Where(w => w.TransactionDate > filterModel.StartDate && w.TransactionDate < filterModel.EndDate);

            var result = new PagedList<ENoticeDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return (PagedList<ENoticeDto>)SetNoticeState(result);
        }

        public PagedList<ENoticeDto> GetAllSpeedWithPaged(int? page, ENoticeDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from n in _noticeRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        join c in _cityRepository.GetAll() on n.CityId equals c.Id
                        join unit in _unitRepository.GetAll() on n.LastUnitId equals unit.Id into unitL
                        from unit in unitL.DefaultIfEmpty()
                        join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                        from unit2 in unit2L.DefaultIfEmpty()
                        where n.Status && n.NoticeType == (int)NoticeType.Speed
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            CreatedDate = n.CreatedDate,
                            Speed = n.Speed,
                            UnitId = unit.Id,
                            ParentUnitId = unit2.Id,
                            LastDebitUserId = n.LastDebitUserId,
                            UnitName = unit2.Name != null ? unit2.Name + "<br>" + unit.Name : "",
                            VehicleId = n.VehicleId,
                            LastDebitKm = n.LastDebitKm,
                            LastDebitStatus = n.LastDebitStatus,
                            Speed2 = "<span class='label bg-success-300 full-width'><i class='icon-meter-slow'></i> " + n.Speed + " Km/h</span>",
                            CityName = "<span class='label bg-orange full-width'>" + c.Name + "</span>",
                            Plate = "<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>",
                            TransactionDate = n.TransactionDate,
                            Address = n.Address,
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                        });

            if (!filterModel.IsAdmin)
            {
                var user = _userRepository.Find(filterModel.CreatedBy);
                var unit = _unitRepository.Find(user.UnitId.Value);
                list = unit.ParentId > 0 ? list.Where(w => w.ParentUnitId == unit.ParentId) : list.Where(w => w.ParentUnitId == unit.Id);
            }

            if (filterModel.LastDebitUserId > 0)
                list = list.Where(w => w.LastDebitUserId == filterModel.LastDebitUserId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.TransactionDate != null)
                list = list.Where(w => w.TransactionDate == filterModel.TransactionDate);

            if (filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != DateTime.MinValue)
                list = list.Where(w => w.TransactionDate > filterModel.StartDate && w.TransactionDate < filterModel.EndDate);

            if (filterModel.MinSpeed > 0)
                list = list.Where(w => w.Speed >= filterModel.MinSpeed);

            if (filterModel.MaxSpeed > 0)
                list = list.Where(w => w.Speed <= filterModel.MaxSpeed);

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);


            var result = new PagedList<ENoticeDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);

            foreach (var item in result)
            {
                if (item.LastDebitUserId > 0)
                {
                    var debitUser = _userRepository.Find(item.LastDebitUserId.Value);
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
                        if (trip.DriverId != item.LastDebitUserId)
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


            return (PagedList<ENoticeDto>)SetNoticeState(result);
        }

        public async Task<List<ENoticeDto>> GetAllSpeed(ENoticeDto filterModel)
        {
            var list = await Task.FromResult((from n in _noticeRepository.GetAll()
                                              join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                                              join c in _cityRepository.GetAll() on n.CityId equals c.Id
                                              join unit in _unitRepository.GetAll() on n.LastUnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()
                                              where n.Status && n.NoticeType == (int)NoticeType.Speed
                                              select new ENoticeDto()
                                              {
                                                  Id = n.Id,
                                                  CreatedDate = n.CreatedDate,
                                                  Speed = n.Speed,
                                                  UnitId = unit.Id,
                                                  ParentUnitId = unit2.Id,
                                                  LastDebitUserId = n.LastDebitUserId,
                                                  UnitName = unit2.Name != null ? unit2.Name + "/" + unit.Name : "",
                                                  VehicleId = n.VehicleId,
                                                  LastDebitKm = n.LastDebitKm,
                                                  LastDebitStatus = n.LastDebitStatus,
                                                  Speed2 = n.Speed.ToString(),
                                                  CityName = c.Name,
                                                  Plate = v.Plate,
                                                  TransactionDate = n.TransactionDate,
                                                  Address = n.Address,
                                              }));

            if (!filterModel.IsAdmin)
            {
                var user = _userRepository.Find(filterModel.CreatedBy);
                var unit = _unitRepository.Find(user.UnitId.Value);
                list = unit.ParentId > 0 ? list.Where(w => w.ParentUnitId == unit.ParentId) : list.Where(w => w.ParentUnitId == unit.Id);
            }

            if (filterModel.LastDebitUserId > 0)
                list = list.Where(w => w.LastDebitUserId == filterModel.LastDebitUserId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.TransactionDate != null)
                list = list.Where(w => w.TransactionDate == filterModel.TransactionDate);

            if (filterModel.StartDate != DateTime.MinValue && filterModel.EndDate != DateTime.MinValue)
                list = list.Where(w => w.TransactionDate > filterModel.StartDate && w.TransactionDate < filterModel.EndDate);

            if (filterModel.MinSpeed > 0)
                list = list.Where(w => w.Speed >= filterModel.MinSpeed);

            if (filterModel.MaxSpeed > 0)
                list = list.Where(w => w.Speed <= filterModel.MaxSpeed);

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            return list.OrderByDescending(o => o.Id).ToList();
        }

        public object SetNoticeState(List<ENoticeDto> list)
        {
            foreach (var item in list)
            {
                var isSendPunish = _noticePunishmentRepository.Any(w => w.NoticeUnitId == item.Id);//i.k'ya yönlendirilmişse
                var isSendUnit = _noticeSendUnitRepository.Any(w => w.NoticeUnitId == item.Id);
                if (item.State == (int)NoticeState.SendUnit && item.Status && !isSendPunish && isSendUnit)
                {
                    var noticeSendUnit = _noticeSendUnitRepository.Where(w => w.NoticeUnitId == item.Id).ToList();
                    if (noticeSendUnit.Any(w => w.State != null))
                        item.StateName = "<span class='label bg-orange faa-flash animated faa-slow full-width'>Cevap geldi </span>";
                    else
                        item.StateName = "<span class='label bg-orange full-width'>Birime Gönderildi-Cevap Bekleniyor</span>";
                }
                else
                {
                    item.StateName = item.State switch
                    {
                        (int)NoticeState.Draft =>
                            "<span class='label bg-warning faa-flash animated faa-slow full-width'>Birime Gönderilmeyi Bekliyor</span>",
                        (int)NoticeState.SendUnit =>
                            "<span class='label bg-orange full-width'>Birime Gönderildi</span>",
                        (int)NoticeState.SendCancelled =>
                            "<span class='label bg-warning full-width'>Gönderim İptal Edildi</span>",
                        (int)NoticeState.RedirectToCancelled =>
                            "<span class='label bg-warning full-width'>Yönlendirmede İptal Edildi</span>",
                        (int)NoticeState.Closed =>
                            "<span class='label bg-orange full-width'>Kapatıldı</span>",
                        _ => "<span class='label bg-warning'>Tip bulunamadı</span>"
                    };
                }

                item.NoticeTypeName = item.NoticeType switch
                {
                    (int)NoticeType.Speed => "<span class='label bg-orange'>Hız İhlali</span>",
                    (int)NoticeType.OutOfHours => "<span class='label bg-slate'>Mesai Dışı Kullanım</span>",
                    (int)NoticeType.Duty => "<span class='label bg-primary'>Görev Analizi</span>",
                    (int)NoticeType.Request => "<span class='label bg-warning'>İstek/Talep</span>",
                    (int)NoticeType.Complaint => "<span class='label bg-warning'>Şikayet</span>",
                    (int)NoticeType.MaintananceUserFault => "<span class='label bg-warning'>Bakım/Onarım Kullanıcı Hatası</span>",
                    _ => "<span class='label bg-warning'>Tip bulunamadı</span>"
                };
            }
            return list;
        }
        public async Task<EResultDto> InsertBulkAsync(ENoticeReadExcelDto model)
        {
            var result = new EResultDto();
            try
            {
                if (model.NoticeList.Count > 0 && model.NoticeType > 0)
                {
                    //plakalar db'de var mı kontrol ediliyor
                    var entities = new List<Notice>();
                    foreach (var item in model.NoticeList)
                    {
                        var entity = _vehicleRepository.Find(item.VehicleId);
                        if (entity != null)
                        {
                            entities.Add(new Notice()
                            {
                                CreatedBy = model.CreatedBy,
                                VehicleId = entity.Id,
                                NoticeType = model.NoticeType,
                                ArventoNo = item.ArventoNo,
                                Driver = item.Driver,
                                TransactionDate = item.TransactionDate,
                                Speed = item.Speed,
                                FirstRunEngineDate = item.FirstRunEngineDate,
                                LastRunEngineDate = item.LastRunEngineDate,
                                TotalKm = item.TotalKm,
                                Address = item.Address,
                                ImportType = model.ImportType,
                                State = (int)NoticeState.Draft
                            });
                        }
                    }

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await _noticeRepository.InsertRangeAsync(entities);
                        _uow.SaveChanges();
                        scope.Complete();
                        result.Message = "Tüm kayıtlar başarıyla eklendi";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Excel okunamadı";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }
        public EResultDto InsertNotice(ENoticeDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var model = _mapper.Map<Notice>(tempModel);
                if (model.NoticeType == (int)NoticeType.Speed)
                    model.TransactionDate = tempModel.TransactionDateSpeed;
                else if (model.NoticeType == (int)NoticeType.Duty)
                    model.TransactionDate = tempModel.TransactionDateMission;

                model.ImportType = (int)ImportType.User;
                model.CreatedBy = tempModel.CreatedBy;
                model.State = (int)NoticeState.Draft;
                _noticeRepository.Insert(model);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
            }
            return result;
        }
        public EResultDto UpdateNotice(ENoticeDto tempModel)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var oldEntity = _noticeRepository.Find(tempModel.Id);
                if (oldEntity.State == (int)NoticeState.Draft)
                {
                    var entity = _mapper.Map<ENoticeDto, Notice>(tempModel, oldEntity);
                    entity.State = oldEntity.State;
                    entity.NoticeUnitId = oldEntity.NoticeUnitId;
                    _noticeRepository.Update(entity);
                    _uow.SaveChanges();
                    result.IsSuccess = true;
                }
                else
                    result.Message = "Kayıt işleme alındığından düzenleme yapılamaz";

            }
            catch (Exception) { result.Message = "Düzenleme sırasında hata oluştu"; }

            return result;
        }
        public async Task<ENoticeDto> GetByIdNoticeAsync(int id)
        {
            var result = await Task.FromResult((from n in _noticeRepository.GetAll()
                                                join c in _cityRepository.GetAll() on n.CityId equals c.Id into cL
                                                from c in cL.DefaultIfEmpty()
                                                where n.Id == id && n.Status
                                                select new ENoticeDto
                                                {
                                                    Id = n.Id,
                                                    VehicleId = n.VehicleId,
                                                    NoticeType = n.NoticeType,
                                                    ArventoNo = n.ArventoNo,
                                                    Driver = n.Driver,
                                                    MissionName = n.MissionName,
                                                    CityId = c.Id,
                                                    TransactionDate = n.TransactionDate,
                                                    Speed = n.Speed,
                                                    FirstRunEngineDate = n.FirstRunEngineDate,
                                                    LastRunEngineDate = n.LastRunEngineDate,
                                                    TotalKm = n.TotalKm,
                                                    Address = n.Address,
                                                    ImportType = n.ImportType,
                                                    Description = n.Description,
                                                }).FirstOrDefault());
            return result;
        }
        public EResultDto DeleteNotice(int id)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var entity = _noticeRepository.Find(id);
                if (entity.State == (int)NoticeState.Draft)
                {
                    entity.Status = false; ;
                    _noticeRepository.Update(entity);
                    _uow.SaveChanges();
                    result.IsSuccess = true;
                }
                else
                    result.Message = "Birime gönderildiğinden işlem yapılamaz";
            }
            catch (Exception) { result.Message = "Silme sırasında hata oluştu"; }
            return result;
        }
        public NoticeUnit Find(int id) => _noticeUnitRepository.Find(id);
        #endregion

        #region NoticeUnit
        public PagedList<ENoticeDto> GetAllUnitWithPaged(int? page, ENoticeDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from nu in _noticeUnitRepository.GetAll()
                        select new ENoticeDto()
                        {
                            Id = nu.Id,
                            Status = nu.Status,
                            StatusName = nu.Status ? "<span class='label bg-success'>Aktif</span>" : "<span class='label bg-warning'>Pasif</span>",
                            CreatedDate = nu.CreatedDate,
                            CreatedBy = nu.CreatedBy,
                            OpenDate = nu.OpenDate,
                            ClosedDate = nu.ClosedDate,
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            State = nu.State,
                            NoticeType = nu.NoticeType,
                            CustomButton = "<li title='Talep düzenle' class='text-primary'><a onclick='funcEditNoticeUnit(" + nu.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                                           "<li title='Talep yönlendir' class='text-orange'><a onclick='funcRedirectNoticeUnit(" + nu.Id + ");'><i class='icon-redo'></i></a></li>" +
                                           "<li title='Talebi sil' class='text-danger'><a onclick='funcDeleteModalOpen(" + nu.Id + ");'><i class='icon-trash'></i></a></li>"
                        });

            //Admin değilse kendi oluşturduğu ticket'ları görebilir
            if (filterModel.LoginUserId > 0)
                list = list.Where(w => w.CreatedBy == filterModel.LoginUserId);

            var result = new PagedList<ENoticeDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return (PagedList<ENoticeDto>)SetNoticeState(result);
        }
        public List<ENoticeDto> GetUnitIdStartEndDateVehicleList(ENoticeUnitDto filterModel)
        {
            var list = (from n in _noticeRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        join un in _unitRepository.GetAll() on v.LastUnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join c in _cityRepository.GetAll() on n.CityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        where v.Status && n.Status && n.NoticeType == filterModel.NoticeType
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            VehicleId = n.VehicleId,
                            NoticeType = n.NoticeType,
                            ArventoNo = n.ArventoNo,
                            Driver = n.Driver,
                            TransactionDate = n.TransactionDate,
                            Speed = n.Speed,
                            FirstRunEngineDate = n.FirstRunEngineDate,
                            LastRunEngineDate = n.LastRunEngineDate,
                            MissionName = n.MissionName,
                            CityId = n.CityId,
                            TotalKm = n.TotalKm,
                            Address = n.Address,
                            ImportType = n.ImportType,
                            Description = n.Description,
                            Plate = v.Plate,
                            State = n.State,
                            NoticeUnitId = n.NoticeUnitId,
                            UnitId = un.Id,//child unit Id
                            ToUnitName = un1.Name + "-" + un.Name,
                            CityName = c2.Name + "/" + c.Name,
                        });

            if (filterModel.ToUnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.ToUnitId);

            if (filterModel.NoticeType == (int)NoticeType.Speed || filterModel.NoticeType == (int)NoticeType.Duty)
                list = list.Where(w => (w.TransactionDate >= filterModel.StartDate && w.TransactionDate < filterModel.EndDate));
            else if (filterModel.NoticeType == (int)NoticeType.OutOfHours)
                list = list.Where(w => (w.FirstRunEngineDate >= filterModel.StartDate && w.FirstRunEngineDate < filterModel.EndDate) && (w.FirstRunEngineDate >= filterModel.StartDate && w.LastRunEngineDate < filterModel.EndDate));

            if (filterModel.Id > 0)//Edit mode
            {
                var result = list.Where(w => w.NoticeUnitId == filterModel.Id).ToList();
                var sendUnitList = _noticeSendUnitRepository.Where(w => w.NoticeUnitId == filterModel.Id).ToList();
                foreach (var item in result)
                {
                    item.EditMode = true;
                    var sendUnitRow = sendUnitList.FirstOrDefault(w => w.NoticeId == item.Id);
                    if (sendUnitRow != null)
                    {
                        item.IsAnswer = sendUnitRow.State != null;
                        item.IsSend = true;
                        item.NoticeSendUnitState = sendUnitRow.State;
                        item.NoticeSendUnitState2 = GetNoticeSendUnitState(sendUnitRow.State);
                        item.NoticeSendUnitDescription = sendUnitRow.Description;
                    }
                    else
                        item.NoticeSendUnitState2 = "<span class='label bg-warning'>İptal</span>";
                }
                return result;
            }
            else //new insert
                list = list.Where(w => w.State == (int)NoticeState.Draft);

            return list.ToList();
        }
        public string GetNoticeSendUnitState(int? state)
        {
            return state switch
            {
                (int)NoticeState.Other => "<span class='label bg-success-300'>Diğer</span>",
                (int)NoticeState.Mission => "<span class='label bg-success-300'>Görevde</span>",
                (int)NoticeState.SpecialPermission => "<span class='label bg-success-300'>Özel İzin</span>",
                (int)NoticeState.OffMission => "<span class='label bg-warning-300'>Görev Dışı</span>",
                _ => "<span class='label bg-orange-300 faa-flash animated faa-slow'>Bekliyor</span>"
            };
        }
        public List<Notice> GetNoticeList(List<ENoticeDto> list)
        {
            var result = new List<Notice>();
            foreach (var item in list)
                result.Add(_noticeRepository.Find(item.Id));

            return result;
        }
        public bool PlateWithProcess(int type)
        {
            if (type == (int)NoticeType.Speed ||
                type == (int)NoticeType.Duty ||
                type == (int)NoticeType.OutOfHours ||
                type == (int)NoticeType.MaintananceUserFault)
                return true;
            else
                return false;
        }
        public bool NoticeCompareOldNew(List<ENoticeDto> oldEntity, List<ENoticeDto> newEntity)
        {
            foreach (var item in newEntity)
                if (oldEntity.Any(a => !a.Status && a.State != (int)NoticeState.Draft && a.Id != item.Id))
                    return false;
            return true;
        }

        public EResultDto InsertNoticeUnit(IList<IFormFile> files, ENoticeUnitDto tempModel)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var user = _userRepository.Find(tempModel.CreatedBy);
                if (user.UnitId > 0)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //NoticeUnit insert
                        var noticeUnitEnt = new NoticeUnit()
                        {
                            CreatedBy = tempModel.CreatedBy,
                            StartDate = tempModel.StartDate,
                            EndDate = tempModel.EndDate,
                            NoticeType = tempModel.NoticeType,
                            Description = HttpUtility.HtmlEncode(tempModel.Description),
                            OpenDate = DateTime.Now,
                            State = (int)NoticeState.SendUnit
                        };
                        _noticeUnitRepository.Insert(noticeUnitEnt);
                        _uow.SaveChanges();

                        //NoticeUnitHistory insert
                        var noticeUnitHistoryEnt = new NoticeUnitHistory()
                        {
                            CreatedBy = tempModel.CreatedBy,
                            NoticeUnitId = noticeUnitEnt.Id,
                            Description = HttpUtility.HtmlEncode(tempModel.Description),
                            ToUnitId = null,
                            State = (int)NoticeState.OpenNotice
                        };
                        _noticeUnitHistoryRepository.Insert(noticeUnitHistoryEnt);

                        if (PlateWithProcess(tempModel.NoticeType))//plakaya bağlı talep
                        {
                            //Notice update
                            var oldNoticeList = GetUnitIdStartEndDateVehicleList(tempModel);
                            var sendNoticeList = tempModel.NoticeList.Where(w => w.IsSend).ToList();//birime gönderilecekler
                            var notSendNoticeList = tempModel.NoticeList.Where(w => !w.IsSend).ToList();
                            if (NoticeCompareOldNew(oldNoticeList, sendNoticeList) && NoticeCompareOldNew(oldNoticeList, notSendNoticeList))//Kontrol
                            {
                                var sendNoticeEntity = sendNoticeList.Select(item => _noticeRepository.Find(item.Id)).ToList();// Birime gönderilecekler state değişiyor
                                sendNoticeEntity.ForEach(f =>
                                {
                                    f.State = (int)NoticeState.SendUnit;
                                    f.NoticeUnitId = noticeUnitEnt.Id;
                                });

                                var notSendNoticeEntity = notSendNoticeList.Select(item => _noticeRepository.Find(item.Id)).ToList();
                                notSendNoticeEntity.ForEach(f =>
                                {
                                    f.State = (int)NoticeState.SendCancelled;
                                    f.NoticeUnitId = noticeUnitEnt.Id;
                                });

                                //NoticeSendUnit insert
                                foreach (var sub in sendNoticeEntity)
                                {
                                    var unitId = oldNoticeList.FirstOrDefault(w => w.Id == sub.Id).UnitId;
                                    var noticeSendUnit = new NoticeSendUnit()
                                    {
                                        CreatedBy = tempModel.CreatedBy,
                                        NoticeId = sub.Id,
                                        NoticeUnitId = noticeUnitEnt.Id,
                                        ToUnitId = unitId.Value
                                    };
                                    _noticeSendUnitRepository.Insert(noticeSendUnit);
                                }

                                _noticeRepository.UpdateRange(sendNoticeEntity);
                                _noticeRepository.UpdateRange(notSendNoticeEntity);
                                _uow.SaveChanges();
                            }
                            else
                            {
                                result.Message = "Sayfayı yenileyip tekrar deneyiniz";
                                return result;
                            }
                        }

                        if (files.Count > 0)
                        {
                            result = FileInsert(files, noticeUnitEnt.Id);
                            if (!result.IsSuccess)
                                return new EResultDto { IsSuccess = false };
                        }

                        _uow.SaveChanges();
                        scope.Complete();
                        result.Message = "İşlem Başarılı";
                        result.Id = noticeUnitEnt.Id;
                        result.IsSuccess = true;
                    }
                }
                else
                    result.Message = "Biriminiz bulunmadığı için talep açamazsınız";
            }
            catch (Exception ex) { result.Message = "Kayıt sırasında hata oluştu"; }
            return result;
        }

        public EResultDto UpdateNoticeUnit(IList<IFormFile> files, ENoticeUnitDto tempModel)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var oldEntity = _noticeUnitRepository.Find(tempModel.Id);
                var user = _userRepository.Find(tempModel.CreatedBy);
                if (!oldEntity.Status)
                    result.Message = "Pasif ihbar üzerinden işlem yapılamaz";
                else if (oldEntity.State == (int)NoticeState.SendUnit)
                    result.Message = "Birime gönderilen talep üzerinden işlem yapılamaz";
                else if (oldEntity.State == (int)NoticeState.Closed)
                    result.Message = "Kapalı ihbar üzerinden işlem yapılamaz";
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (oldEntity.State == tempModel.State)
                        {
                            //toplu notice tablosu da update olacak ilgili sütunlar
                            var entity = _mapper.Map<ENoticeUnitDto, NoticeUnit>(tempModel, oldEntity);
                            _noticeUnitRepository.Update(entity);
                            _uow.SaveChanges();

                            if (files.Count > 0)
                            {
                                result = FileInsert(files, entity.Id);
                                if (!result.IsSuccess)
                                    return new EResultDto { IsSuccess = false };
                            }

                            scope.Complete();
                            result.IsSuccess = true;
                            result.Id = entity.Id;

                        }
                        else
                            result.Message = "Düzenleme modunda talep türü değişemez";
                    }
                }
            }
            catch (Exception) { result.Message = "Düzenleme sırasında hata oluştu"; }
            return result;
        }
        public async Task<ENoticeUnitDto> GetByIdNoticeUnitAsync(int id)
        {
            var result = await Task.FromResult((from nu in _noticeUnitRepository.GetAll()
                                                    //join u in _unitRepository.GetAll() on nu.ToUnitId equals u.Id
                                                where nu.Id == id
                                                select new ENoticeUnitDto()
                                                {
                                                    Id = nu.Id,
                                                    //ToUnitId = nu.ToUnitId,
                                                    StartDate = nu.StartDate,
                                                    EndDate = nu.EndDate,
                                                    NoticeType = nu.NoticeType,
                                                    Description = HttpUtility.HtmlDecode(nu.Description),
                                                    State = nu.State
                                                }).FirstOrDefault());

            var files = (from n in _noticeUnitRepository.GetAll()
                         join nu in _noticeUnitFileRepository.GetAll() on n.Id equals nu.NoticeUnitId
                         join fu in _fileUploadRepository.GetAll() on nu.FileUploadId equals fu.Id
                         where n.Id == id
                         select new EFileUploadDto
                         {
                             Id = fu.Id,
                             NoticeUnitId = n.Id,
                             Name = fu.Name,
                             Extention = fu.Extention,
                             FileSize = fu.FileSize,
                             NoticeUnitFileId = nu.Id,
                         }).ToList();

            result.files = files;
            return result;
        }

        public EResultDto FileInsert(IList<IFormFile> files, int noticeUnitId)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                if (noticeUnitId > 0)
                {
                    result = fs.FileUploadInsertNoticeUnit(files);
                    if (result.IsSuccess)
                    {
                        foreach (var item in result.Ids)
                        {
                            var entity = _noticeUnitFileRepository.Insert(new NoticeUnitFile()
                            {
                                FileUploadId = item,
                                NoticeUnitId = noticeUnitId
                            });
                        }
                        _uow.SaveChanges();
                        result.Id = noticeUnitId;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Dosya yüklemede hata oluştu";
                    }
                }
                else
                    result.IsSuccess = false;
            }
            catch (Exception)
            {
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/Notice/NoticeUnit/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }
        public EResultDto DeleteNoticeUnit(ENoticeDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var noticeUnit = _noticeUnitRepository.Find(model.NoticeUnitId.Value);
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var noticeList = _noticeRepository.Where(w => w.NoticeUnitId == model.NoticeUnitId).ToList();
                    if (model.IsReleaseSubNotice)//alttaki notice'leri sonraki kullanım için serbest bırak
                    {
                        noticeList.ForEach(f =>
                        {
                            f.State = (int)NoticeState.Draft;
                            f.NoticeUnitId = null;
                        });
                        _noticeRepository.UpdateRange(noticeList);
                    }

                    noticeUnit.Status = false;
                    noticeUnit.DeletedBy = model.LoginUserId;
                    noticeUnit.DeleteDescription = model.Description;
                    _noticeUnitRepository.Update(noticeUnit);

                    _uow.SaveChanges();
                    scope.Complete();
                    if (model.IsReleaseSubNotice)
                        result.Message = "Bu talebe bağlı <b>" + noticeList.Count + "</b> adet plaka serbest bırakılmıştır.";
                    else
                        result.Message = "Talep silinmiştir";
                    result.IsSuccess = true;
                }
            }
            catch (Exception) { result.Message = "Kayıt sırasında hata oluştu"; }

            return result;
        }
        #endregion

        #region NoticeUnitAnswer
        public PagedList<ENoticeDto> GetAllUnitAnswerWithPaged(int? page, ENoticeDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var unitId = _userRepository.Find(filterModel.LoginUserId).UnitId.Value;
            var parentUnitId = _unitRepository.Find(unitId).ParentId;
            parentUnitId = parentUnitId ?? 0;

            var noticeUnitAnswerList = (from nu in _noticeUnitRepository.GetAll()
                                        join h in _noticeSendUnitRepository.GetAll() on nu.Id equals h.NoticeUnitId
                                        join un in _unitRepository.GetAll() on h.ToUnitId equals un.Id into unL
                                        from un in unL.DefaultIfEmpty()// child
                                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                                        from un1 in un1L.DefaultIfEmpty()//parentId
                                        where nu.Status && (un.ParentId == unitId || un1.Id == parentUnitId)
                                        select new ENoticeDto()
                                        {
                                            Id = nu.Id,
                                            UnitId = h.ToUnitId,
                                            CreatedDate = nu.CreatedDate,
                                            OpenDate = nu.OpenDate,
                                            ClosedDate = nu.ClosedDate,
                                            State = nu.State,
                                            NoticeType = nu.NoticeType,
                                            NoticeUnitTypeName = "Analiz",
                                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                                            CustomButton = "<li title='Talep Detay' class='text-primary'><a onclick='previewNotice(" + nu.Id + "," + NoticeRedirectType.ContentEdit + ");'><i class='icon-list'></i></a></li>"
                                        }).Distinct();
            var result1 = new PagedList<ENoticeDto>(noticeUnitAnswerList.OrderBy(o => o.State).ThenByDescending(t => t.CreatedDate), page, PagedCount.GridKayitSayisi);

            var noticeUnitRedirectList = (from nu in _noticeUnitRepository.GetAll()
                                          join np in _noticePunishmentRepository.GetAll() on nu.Id equals np.NoticeUnitId
                                          //join h in _noticeSendUnitRepository.GetAll() on nu.Id equals h.NoticeUnitId
                                          join un in _unitRepository.GetAll() on np.ToUnitId equals un.Id into unL
                                          from un in unL.DefaultIfEmpty()//child
                                          join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                                          from un1 in un1L.DefaultIfEmpty()//parentId
                                          where nu.Status && (np.ToUnitId == unitId || np.ToUnitId == parentUnitId)
                                          select new ENoticeDto()
                                          {
                                              Id = nu.Id,
                                              CreatedDate = nu.CreatedDate,
                                              OpenDate = nu.OpenDate,
                                              ClosedDate = nu.ClosedDate,
                                              State = nu.State,
                                              NoticeType = nu.NoticeType,
                                              NoticeUnitTypeName = "Sonuç",
                                              PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                                              CustomButton = "<li title='Talep Detay' class='text-primary'><a onclick='previewNotice(" + nu.Id + "," + NoticeRedirectType.Analysis + ");'><i class='icon-list'></i></a></li>"
                                          }).Distinct();
            var result2 = new PagedList<ENoticeDto>(noticeUnitRedirectList.OrderBy(o => o.State).ThenByDescending(t => t.CreatedDate), page, PagedCount.GridKayitSayisi);

            result1.AddRange(result2);
            return (PagedList<ENoticeDto>)SetNoticeState(result1);
        }
        public EResultDto InsertNoticeUnitAnswer(ENoticeUnitDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var unitId = _userRepository.Find(model.LoginUserId).UnitId.Value;
                var parentUnitId = _unitRepository.Find(unitId).ParentId;
                var noticeUnitId = model.Id;
                var noticeList = model.NoticeList;

                var isAnswerRecord = (from nu in _noticeSendUnitRepository.GetAll()
                                      join un in _unitRepository.GetAll() on nu.ToUnitId equals un.Id into unL
                                      from un in unL.DefaultIfEmpty() // child
                                      join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                                      from un1 in un1L.DefaultIfEmpty() //parentId
                                      where nu.NoticeUnitId == noticeUnitId &&
                                            (un.Id == unitId || un.ParentId == unitId || un.Id == parentUnitId || un.ParentId == parentUnitId) &&
                                            nu.State == null
                                      select new ENoticeUnitDto() { Id = nu.Id }).Any();
                if (isAnswerRecord)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var noticeUnitHistory = new NoticeUnitHistory()
                        {
                            CreatedBy = model.LoginUserId,
                            NoticeUnitId = noticeUnitId,
                            //ToUnitId = noticeUnit,
                            Description = HttpUtility.HtmlEncode(model.Description),
                            State = (int)NoticeState.AnswerUnit
                        };
                        _noticeUnitHistoryRepository.Insert(noticeUnitHistory);

                        foreach (var item in noticeList)
                        {
                            var noticeSendUnit = _noticeSendUnitRepository.FirstOrDefault(f => f.NoticeId == item.Id);
                            if (noticeSendUnit != null)
                            {
                                noticeSendUnit.State = item.State;
                                noticeSendUnit.Description = item.Description;
                                _noticeSendUnitRepository.Update(noticeSendUnit);
                            }
                            else
                            {
                                result.Message = "Hata oluştu";
                                return result;
                            }
                        }

                        _uow.SaveChanges();
                        scope.Complete();
                        result.IsSuccess = true;
                    }
                }
                else
                    result.Message = "Bu talebe daha önce cevap verilmiş";
            }
            catch (Exception e) { result.Message = "Hata Oluştu"; }

            return result;
        }
        public List<ENoticeDto> GetNoticeUnitAnswerList(int noticeUnitId, int userId)//Birimin cevaplayacağı ekran listesi
        {
            var unitId = _userRepository.Find(userId).UnitId.Value;
            var parentUnitId = _unitRepository.Find(unitId).ParentId;

            var list = (from su in _noticeSendUnitRepository.GetAll()
                        join nu in _noticeUnitRepository.GetAll() on su.NoticeUnitId equals nu.Id
                        join p in _noticePunishmentRepository.GetAll() on nu.Id equals p.NoticeUnitId into pL
                        from p in pL.DefaultIfEmpty()
                        join n in _noticeRepository.GetAll() on su.NoticeId equals n.Id
                        join un in _unitRepository.GetAll() on su.ToUnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        join c in _cityRepository.GetAll() on n.CityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        where nu.Status && nu.Id == noticeUnitId && (un.ParentId == unitId || un1.Id == parentUnitId)
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            VehicleId = n.VehicleId,
                            NoticeType = n.NoticeType,
                            ArventoNo = n.ArventoNo,
                            Driver = n.Driver,
                            TransactionDate = n.TransactionDate,
                            Speed = n.Speed,
                            FirstRunEngineDate = n.FirstRunEngineDate,
                            LastRunEngineDate = n.LastRunEngineDate,
                            MissionName = n.MissionName,
                            CityId = n.CityId,
                            ToUnitName = un1.Name + "-" + un.Name,
                            TotalKm = n.TotalKm,
                            Address = n.Address,
                            ImportType = n.ImportType,
                            Description = n.Description,
                            Plate = v.Plate,
                            State = n.State,
                            CityName = c2.Name + "/" + c.Name,
                            NoticeUnitId = n.NoticeUnitId,
                            UnitId = v.LastUnitId,

                            NoticeSendUnitState = su.State,
                            NoticeSendUnitDescription = su.Description,
                        }).Distinct();

            return list.ToList();
        }
        public List<ENoticeDto> GetNoticeUnitAnswerRedirectList(int noticeUnitId, int userId)//insan kaynakları cevaplayacağı ekran listesi
        {
            var user = _userRepository.Find(userId);
            var unitId = _userRepository.Find(userId).UnitId.Value;
            var parentUnitId = _unitRepository.Find(unitId).ParentId;
            parentUnitId = parentUnitId ?? 0;

            var list = (from p in _noticePunishmentRepository.GetAll()
                        join driver in _userRepository.GetAll() on p.DriverId equals driver.Id into driverL
                        from driver in driverL.DefaultIfEmpty()
                        join su in _noticeSendUnitRepository.GetAll() on p.NoticeSendUnitId equals su.Id
                        join nu in _noticeUnitRepository.GetAll() on su.NoticeUnitId equals nu.Id
                        join n in _noticeRepository.GetAll() on su.NoticeId equals n.Id
                        join un in _unitRepository.GetAll() on su.ToUnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        join c in _cityRepository.GetAll() on n.CityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        where nu.Status && p.NoticeUnitId == noticeUnitId && n.State == (int)NoticeState.SendUnit
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            VehicleId = n.VehicleId,
                            NoticeType = n.NoticeType,
                            ArventoNo = n.ArventoNo,
                            Driver = n.Driver,
                            TransactionDate = n.TransactionDate,
                            Speed = n.Speed,
                            FirstRunEngineDate = n.FirstRunEngineDate,
                            LastRunEngineDate = n.LastRunEngineDate,
                            MissionName = n.MissionName,
                            CityId = n.CityId,
                            ToUnitName = un1.Name + "-" + un.Name,
                            TotalKm = n.TotalKm,
                            Address = n.Address,
                            ImportType = n.ImportType,
                            Description = n.Description,
                            Plate = v.Plate,
                            State = n.State,
                            NoticeUnitId = n.NoticeUnitId,
                            UnitId = v.LastUnitId,
                            CityName = c2.Name + "/" + c.Name,
                            ToUnitId = p.ToUnitId,

                            NoticeSendUnitState = su.State,
                            NoticeSendUnitDescription = su.Description,

                            NoticePunishmentId = p.Id,
                            Amount = p.Amount,
                            DriverId = p.DriverId,
                            NoticeSendRedirectUnitState = p.State,
                            NoticeSendRedirectUnitDescription = p.Description,
                            NameSurname = driver.Name + " " + driver.Surname + "/" + driver.MobilePhone
                        }).Distinct();

            if (!user.IsAdmin)
                list = list.Where(w => w.ToUnitId == unitId || w.ToUnitId == parentUnitId);

            //if (user.Flag != (int)Flag.Admin)
            //    list = list.Where(w => w.ToUnitId == unitId || w.ToUnitId == parentUnitId);

            return list.ToList();
        }


        public bool IsAutForNoticeUnit(int noticeUnitId, int userId)
        {
            var unitId = _userRepository.Find(userId).UnitId.Value;
            var parentUnitId = _unitRepository.Find(unitId).ParentId;
            parentUnitId = parentUnitId ?? 0;

            var sendUnit = (from nu in _noticeSendUnitRepository.GetAll()
                            join un in _unitRepository.GetAll() on nu.ToUnitId equals un.Id into unL
                            from un in unL.DefaultIfEmpty() // child
                            join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                            from un1 in un1L.DefaultIfEmpty() //parentId
                            where nu.NoticeUnitId == noticeUnitId &&
                                  (un.Id == unitId || un.ParentId == unitId || un.Id == parentUnitId || un.ParentId == parentUnitId)
                            select new ENoticeUnitDto() { Id = nu.Id }).Any();

            var history = _noticeUnitHistoryRepository.Any(a => a.CreatedBy == userId);
            var noticePunishment =
                _noticePunishmentRepository.Any(a => a.ToUnitId == unitId || a.ToUnitId == parentUnitId);

            if (sendUnit || history || noticePunishment)
                return true;
            return false;
        }
        #endregion

        #region NoticeUnit Redirect
        public EResultDto IsRedirectNoticeUnit(int noticeUnitId)
        {
            var result = new EResultDto();
            var noticeUnit = _noticeUnitRepository.Find(noticeUnitId);
            if (!noticeUnit.Status)
            {
                result.Message = "Pasif talep yönlendirilemez";
                result.IsSuccess = false;
                return result;
            }

            var noticePunishment = (from np in _noticePunishmentRepository.GetAll()
                                    join u in _unitRepository.GetAll() on np.ToUnitId equals u.Id
                                    where np.NoticeUnitId == noticeUnitId
                                    select new { UnitName = u.Name }).FirstOrDefault();
            if (noticePunishment != null)
            {
                result.Message = "Bu talep <b>" + noticePunishment.UnitName + "</b> birimine yönlendirilmiştir";
                result.IsSuccess = false;
                return result;
            }

            var noticeSendUnit = (from nu in _noticeSendUnitRepository.GetAll()
                                  join u in _unitRepository.GetAll() on nu.ToUnitId equals u.Id
                                  where nu.NoticeUnitId == noticeUnitId && nu.State == null
                                  select new { UnitName = u.Name }).Distinct().ToList();
            if (noticeSendUnit.Count > 0)
            {
                result.Message = "Birimlerden cevap beklendiği için yönlendirilemez.<br/>Cevap beklenen birim(ler)<br/>";
                foreach (var sb in noticeSendUnit)
                    result.Message += "<span class='label bg-orange-300'>" + sb.UnitName + "</span><br/>";
                result.IsSuccess = false;
                return result;
            }
            return result;
        }
        public List<ENoticeDto> RedirectVehicleList(int noticeUnitId)
        {
            var list = (from su in _noticeSendUnitRepository.GetAll()
                        join p in _noticePunishmentRepository.GetAll() on su.NoticeUnitId equals p.NoticeUnitId into pL
                        from p in pL.DefaultIfEmpty()
                        join n in _noticeRepository.GetAll() on su.NoticeId equals n.Id
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        join un in _unitRepository.GetAll() on v.LastUnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join c in _cityRepository.GetAll() on n.CityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        where su.NoticeUnitId == noticeUnitId
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            VehicleId = n.VehicleId,
                            NoticeType = n.NoticeType,
                            ArventoNo = n.ArventoNo,
                            Driver = n.Driver,
                            TransactionDate = n.TransactionDate,
                            Speed = n.Speed,
                            FirstRunEngineDate = n.FirstRunEngineDate,
                            LastRunEngineDate = n.LastRunEngineDate,
                            MissionName = n.MissionName,
                            CityId = n.CityId,
                            TotalKm = n.TotalKm,
                            Address = n.Address,
                            ImportType = n.ImportType,
                            Description = n.Description,
                            Plate = v.Plate,
                            State = n.State,
                            NoticeUnitId = n.NoticeUnitId,
                            UnitId = un1.Id,
                            ToUnitName = un1.Name + "-" + un.Name,
                            CityName = c2.Name + "/" + c.Name,
                            NoticeSendUnitState = su.State,
                            NoticeSendUnitDescription = su.Description,

                            DriverId = p.DriverId,
                            NoticeSendRedirectUnitState = p.State,
                            NoticeSendRedirectUnitDescription = p.Description
                        }).Distinct().ToList();

            foreach (var item in list)
                item.NoticeSendUnitState2 = GetNoticeSendUnitState(item.NoticeSendUnitState);

            return list;
        }
        public EResultDto InsertRedirectNotice(ENoticeUnitDto model)//Birim müdürü cevap
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                result = IsRedirectNoticeUnit(model.Id);
                if (result.IsSuccess)
                {
                    var sendNoticeList = model.NoticeList.Where(w => w.IsSend).ToList();
                    var notSendNoticeList = model.NoticeList.Where(w => !w.IsSend).ToList();
                    if (!sendNoticeList.Any())
                    {
                        result.IsSuccess = false;
                        result.Message = "Gönderilecek en az 1 plaka seçilmelidir";
                        return result;
                    }

                    var noticeList = _noticeRepository.Where(w => w.NoticeUnitId == model.Id && w.State == (int)NoticeState.SendUnit).Select(s => new ENoticeDto()
                    {
                        Id = s.Id,
                        Status = s.Status,
                        State = s.State
                    }).ToList();

                    if (NoticeCompareOldNew(noticeList, sendNoticeList) && NoticeCompareOldNew(noticeList, notSendNoticeList)) //Kontrol
                    {
                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var notSendNoticeEntity = notSendNoticeList.Select(item => _noticeRepository.Find(item.Id)).ToList();
                            notSendNoticeEntity.ForEach(f => { f.State = (int)NoticeState.RedirectToCancelled; });
                            _noticeRepository.UpdateRange(notSendNoticeEntity);
                            foreach (var item in sendNoticeList)
                            {
                                var noticeSendUnit = _noticeSendUnitRepository.FirstOrDefault(f => f.NoticeId == item.Id);
                                _noticePunishmentRepository.Insert(new NoticePunishment()
                                {
                                    CreatedBy = model.LoginUserId,
                                    NoticeSendUnitId = (int)UnitId.InsanKaynaklari,// noticeSendUnit.Id,//şimdilik ik'ya gidecek ama diğer birimlere de gidebilir altyapısı uygun
                                    NoticeId = item.Id,
                                    Amount = item.Amount,
                                    DriverId = item.DriverId,
                                    ToUnitId = model.ToUnitId,
                                    NoticeUnitId = model.Id,
                                });
                            }

                            //History insert
                            var noticeUnitHistory = new NoticeUnitHistory()
                            {
                                CreatedBy = model.LoginUserId,
                                NoticeUnitId = model.Id,
                                Description = HttpUtility.HtmlEncode(model.Description),
                                State = (int)NoticeState.SendUnit
                            };
                            _noticeUnitHistoryRepository.Insert(noticeUnitHistory);

                            //noticeUnit update
                            var noticeUnit = _noticeUnitRepository.Find(model.Id);
                            noticeUnit.State = (int)NoticeState.SendUnit;
                            _noticeUnitRepository.Update(noticeUnit);

                            _uow.SaveChanges();
                            scope.Complete();
                            result.IsSuccess = true;
                        }
                    }
                    else
                        result.Message = "Sayfayı yenileyip tekrar deneyiniz.";
                }
            }
            catch (Exception e) { result.Message = "Hata oluştu"; }

            return result;
        }
        public EResultDto InsertNoticeUnitRedirectAnswer(ENoticeUnitDto model)//i.k cevap
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var sendNoticeList = model.NoticeList;
                var noticeUnitId = model.Id;
                foreach (var item in sendNoticeList)//updat edilecek NoticePunisment kontrol ediliyor
                {
                    var punishment = _noticePunishmentRepository.FirstOrDefault(w => w.NoticeUnitId == noticeUnitId && w.NoticeId == item.Id);
                    if (punishment == null)
                    {
                        result.Message = "Sayfayı yenileyip tekrar deneyiniz";
                        return result;
                    }
                    else if (punishment.ConfirmUserId > 0)
                    {
                        var confirmUser = _userRepository.Find(punishment.ConfirmUserId.Value);
                        result.Message = "Bu talep <b>" + confirmUser.Name + " " + confirmUser.Surname +
                                         "</b> tarafından <b>" + punishment.ConfirmDate.Value.ToString("dd/MM/yyyy") +
                                         "</b> tarihinde cevaplanıp kapatılmıştır";
                        return result;
                    }

                    if (item.State == (int)NoticePunishmentState.PayCut && item.Amount == 0)
                    {
                        result.Message = "Ceza tutarı alanı boş geçilemez";
                        return result;
                    }

                    if (item.State != (int)NoticePunishmentState.PayCut && item.Amount > 0)
                    {
                        result.Message = "Ceza tutarı alanı sadece <b>ücret kesintisi</b> tipinde girilmelidir";
                        return result;
                    }
                }

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var item in sendNoticeList)
                    {
                        var noticePunishmentEntity = _noticePunishmentRepository.FirstOrDefault(w => w.NoticeUnitId == noticeUnitId && w.NoticeId == item.Id);
                        if (noticePunishmentEntity == null)
                        {
                            result.Message = "Sayfayı yenileyip tekrar deneyiniz";
                            return result;
                        }

                        //noticePunishmentEntity.Amount = item.State == (int)NoticePunishmentState.PayCut ? item.Amount : null;
                        //noticePunishmentEntity.DriverId = item.DriverId;
                        noticePunishmentEntity.ConfirmUserId = model.LoginUserId;
                        noticePunishmentEntity.ConfirmDate = DateTime.Now;
                        noticePunishmentEntity.State = item.State;
                        noticePunishmentEntity.Description = item.Description;
                        _noticePunishmentRepository.Update(noticePunishmentEntity);
                    }

                    //History insert
                    var noticeUnitHistory = new NoticeUnitHistory()
                    {
                        CreatedBy = model.LoginUserId,
                        NoticeUnitId = model.Id,
                        Description = HttpUtility.HtmlEncode(model.Description),
                        State = (int)NoticeState.Closed
                    };
                    _noticeUnitHistoryRepository.Insert(noticeUnitHistory);

                    //noticeUnit update
                    var noticeUnit = _noticeUnitRepository.Find(model.Id);
                    noticeUnit.State = (int)NoticeState.Closed;
                    noticeUnit.ClosedDate = DateTime.Now;
                    _noticeUnitRepository.Update(noticeUnit);

                    _uow.SaveChanges();
                    scope.Complete();
                    result.IsSuccess = true;
                }
            }
            catch (Exception e) { result.Message = "Hata oluştu"; }

            return result;
        }
        #endregion

        #region NoticeUnitHistory
        public List<ENoticeUnitDto> GetNoticeUnitHistory(int noticeUnitId, int userId, bool isAdmin)
        {
            var noticeUnit = _noticeUnitRepository.Find(noticeUnitId);
            var list = (from h in _noticeUnitHistoryRepository.GetAll()
                        join u in _userRepository.GetAll() on h.CreatedBy equals u.Id
                        where h.NoticeUnitId == noticeUnitId && h.Status
                        select new ENoticeUnitDto()
                        {
                            Id = noticeUnitId,
                            NameSurname = u.Name + " " + u.Surname,
                            CreatedDate = h.CreatedDate,
                            CreatedBy = h.CreatedBy,
                            CreatedUnitId = noticeUnit.CreatedBy,
                            //ToUnitName = un1.Name + "-" + un.Name,
                            State = h.State,
                            Image = u.Image != null ? ("data:image/gif;base64," + Convert.ToBase64String(u.Image)) : "/admin/images/userEmpty.jpg",
                            Description = HttpUtility.HtmlDecode(h.Description),
                        }).OrderBy(o => o.CreatedDate).ToList();

            //geçmiş listesinde sadece kendi yönlendirmesini görebilir veya ceza veren kullanıcı
            var user = _userRepository.Find(userId);
            if (!_noticePunishmentRepository.Any(a => a.NoticeUnitId == noticeUnitId && a.ToUnitId == user.UnitId))
                if (!isAdmin)
                    if (noticeUnit.CreatedBy != userId)
                    {
                        var redirectUnit = list.Where(w => w.State != (int)NoticeState.AnswerUnit).ToList();
                        list = list.Where(w => w.State == (int)NoticeState.AnswerUnit && w.CreatedBy == userId).ToList();
                        list.AddRange(redirectUnit);
                    }

            foreach (var item in list)
            {
                var userUnit = (from u in _userRepository.GetAll()
                                join unit in _unitRepository.GetAll() on u.UnitId equals unit.Id
                                where u.Id == item.CreatedBy
                                select new { Id = unit.Id, UnitName = unit.Name }).FirstOrDefault();
                item.FromUnitName = userUnit.UnitName;
                switch (item.State)
                {
                    case (int)NoticeState.OpenNotice:
                        {

                            item.StateName = "<b>#" + item.Id + "</b> numaralı talep kaydı açıldı.";
                            if (noticeUnit.CreatedBy == userId || isAdmin)
                            {
                                var arr = (from su in _noticeSendUnitRepository.GetAll()
                                           join un in _unitRepository.GetAll() on su.ToUnitId equals un.Id
                                           join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id
                                           where su.NoticeUnitId == noticeUnitId
                                           select new { UnitId = su.ToUnitId, UnitName = un1.Name + "-" + un.Name }).ToList().GroupBy(g => g.UnitId).Select(s => new { UnitName = s.First().UnitName }).ToList();
                                if (arr.Any())//hangi birimlere kayıt açıldı
                                {
                                    item.StateName += "<br/>Talep açılan birimler:<br/>";
                                    foreach (var sb in arr)
                                        item.StateName += "<span class='label bg-primary-300'>" + sb.UnitName + "</span><br/>";
                                }
                            }
                            break;
                        }
                    case (int)NoticeState.AnswerUnit:
                        {
                            item.StateName = "Biriminden cevap geldi";
                            break;
                        }
                    case (int)NoticeState.SendUnit:
                        {
                            var sendToUnit = (from u in _noticePunishmentRepository.GetAll()
                                              join unit in _unitRepository.GetAll() on u.ToUnitId equals unit.Id
                                              where u.NoticeUnitId == item.Id
                                              select new { Id = unit.Id, UnitName = unit.Name }).FirstOrDefault();
                            item.FromUnitName = sendToUnit != null ? sendToUnit.UnitName : "";
                            item.StateName = "Birimine yönlendirildi";
                            break;
                        }
                    default:
                        {
                            item.StateName = "";
                            break;
                        }
                }
            }

            return list.OrderBy(o => o.CreatedDate).ToList();
        }
        public List<ENoticeDto> GetNoticeHistoryResultList(int noticeUnitId, int userId)
        {
            var user = _userRepository.Find(userId);
            var unitId = _userRepository.Find(userId).UnitId.Value;
            var parentUnitId = _unitRepository.Find(unitId).ParentId;
            parentUnitId = parentUnitId ?? 0;

            var list = (from p in _noticePunishmentRepository.GetAll()
                        join driver in _userRepository.GetAll() on p.DriverId equals driver.Id into driverL
                        from driver in driverL.DefaultIfEmpty()
                        join su in _noticeSendUnitRepository.GetAll() on p.NoticeSendUnitId equals su.Id
                        join nu in _noticeUnitRepository.GetAll() on su.NoticeUnitId equals nu.Id
                        join n in _noticeRepository.GetAll() on su.NoticeId equals n.Id
                        join un in _unitRepository.GetAll() on su.ToUnitId equals un.Id into unL
                        from un in unL.DefaultIfEmpty()
                        join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                        from un1 in un1L.DefaultIfEmpty()
                        join v in _vehicleRepository.GetAll() on n.VehicleId equals v.Id
                        join c in _cityRepository.GetAll() on n.CityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        where nu.Status && p.NoticeUnitId == noticeUnitId && n.State == (int)NoticeState.SendUnit
                        select new ENoticeDto()
                        {
                            Id = n.Id,
                            CreatedBy = nu.CreatedBy,
                            VehicleId = n.VehicleId,
                            NoticeType = n.NoticeType,
                            ArventoNo = n.ArventoNo,
                            Driver = n.Driver,
                            TransactionDate = n.TransactionDate,
                            Speed = n.Speed,
                            FirstRunEngineDate = n.FirstRunEngineDate,
                            LastRunEngineDate = n.LastRunEngineDate,
                            MissionName = n.MissionName,
                            CityId = n.CityId,
                            ToUnitName = un1.Name + "-" + un.Name,
                            TotalKm = n.TotalKm,
                            Address = n.Address,
                            ImportType = n.ImportType,
                            Description = n.Description,
                            Plate = v.Plate,
                            State = n.State,
                            NoticeUnitId = n.NoticeUnitId,
                            UnitId = v.LastUnitId,
                            CityName = c2.Name + "/" + c.Name,
                            ToUnitId = su.ToUnitId,

                            NoticeSendUnitState = su.State,
                            NoticeSendUnitDescription = su.Description,

                            NoticePunishmentId = p.Id,
                            Amount = p.Amount,
                            DriverId = p.DriverId,
                            NoticeSendRedirectUnitState = p.State,
                            NoticeSendRedirectUnitDescription = p.Description,
                            NameSurname = driver.Name + " " + driver.Surname + "/" + driver.MobilePhone
                        }).Distinct();

            if (!user.IsAdmin)
                if (list.ToList().Any(a => a.CreatedBy != userId))
                    list = list.Where(w => w.ToUnitId == unitId || w.ToUnitId == parentUnitId);

            return list.ToList();
        }
        #endregion
    }
}
