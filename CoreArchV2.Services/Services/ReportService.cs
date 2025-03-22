using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Note;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.TripVehicle;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CoreArchV2.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<FuelLog> _fuelLogRepository;
        private readonly ILogger<ReportService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleBrandModel> _vehicleBrandModelRepository;
        private readonly IGenericRepository<VehicleCity> _vehicleCityRepository;
        private readonly IGenericRepository<VehicleDebit> _vehicleDebitRepository;
        private readonly IGenericRepository<VehicleExaminationDate> _vehicleExaminationDateRepository;
        private readonly IGenericRepository<VehicleRent> _vehicleRentRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehicleTransferLog> _vehicleTransferLogRepository;
        private readonly IGenericRepository<Maintenance> _maintenanceRepository;
        private readonly IGenericRepository<MaintenanceType> _maintenanceTypeRepository;
        private readonly IGenericRepository<VehicleContract> _vehicleContractRepository;
        private readonly IGenericRepository<VehicleAmount> _vehicleAmountRepository;
        private readonly IGenericRepository<OneNote> _oneNoteRepository;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<TripLog> _tripLogRepository;

        public ReportService(IMemoryCache memoryCache,
            IUnitOfWork uow,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<ReportService> logger)
        {
            _memoryCache = memoryCache;
            _cacheService = cacheService;
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _vehicleRentRepository = uow.GetRepository<VehicleRent>();
            _vehicleDebitRepository = uow.GetRepository<VehicleDebit>();
            _vehicleExaminationDateRepository = uow.GetRepository<VehicleExaminationDate>();
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _cityRepository = uow.GetRepository<City>();
            _unitRepository = uow.GetRepository<Unit>();
            _vehicleCityRepository = uow.GetRepository<VehicleCity>();
            _vehicleTransferLogRepository = uow.GetRepository<VehicleTransferLog>();
            _vehicleBrandModelRepository = uow.GetRepository<VehicleBrandModel>();
            _fuelLogRepository = uow.GetRepository<FuelLog>();
            _maintenanceRepository = uow.GetRepository<Maintenance>();
            _maintenanceTypeRepository = uow.GetRepository<MaintenanceType>();
            _vehicleContractRepository = uow.GetRepository<VehicleContract>();
            _vehicleAmountRepository = uow.GetRepository<VehicleAmount>();
            _oneNoteRepository = uow.GetRepository<OneNote>();
            _tripRepository = uow.GetRepository<Trip>();
            _tripLogRepository = uow.GetRepository<TripLog>();
        }

        #region Dashboard
        public async Task<RDashboardDto> ActiveVehicleCount(RFilterModelDto filterModel)
        {
            var entity = new List<EVehicleDto>();
            var result = new RDashboardDto();
            if (filterModel.IsHistoryForDebitList && filterModel.Search != null)//araç ve zimmetli geçmişinde arama yapar
                entity = await DebitInSearch(filterModel);
            else//Araç tablosunda arama yapar
                entity = await GetActiveVehicleList(filterModel);

            result = await Task.FromResult(new RDashboardDto
            {
                AllVehicle = entity.Count(),
                InPoolVehicle = await SetOneNoteVehiclePlate(entity.Where(w => w.LastStatus == (int)DebitState.Pool).ToList()),
                RentVehicle = await SetOneNoteVehiclePlate(entity.Where(w => w.FixtureTypeId == (int)FixtureType.ForRent).ToList()),
                FixVehicle = await SetOneNoteVehiclePlate(entity.Where(w => w.FixtureTypeId == (int)FixtureType.Ownership).ToList()),
                ServiceInVehicle = await SetOneNoteVehiclePlate(entity.Where(w => w.LastStatus == (int)DebitState.InService).ToList()),
            });
            result.EmptyVehicle = new List<EVehicleDto>();
            result.EmptyVehicle.AddRange(result.ServiceInVehicle);
            result.EmptyVehicle.AddRange(result.InPoolVehicle);


            return result;
        }

        public async Task<List<EVehicleDto>> SetOneNoteVehiclePlate(List<EVehicleDto> list)
        {
            foreach (var item in list)
            {
                if (!item.Plate.Contains("getVehicleNote"))
                    if (await _oneNoteRepository.AnyAsync(w => w.VehicleId == item.VehicleId && w.Status && w.Type != 3))
                        item.Plate += "<br/><i title='Bu araçta not bulundu' onclick='getVehicleNote(" + item.VehicleId + ")' style='color:red;cursor: pointer;' class='icon-bell3 faa-ring animated faa-slow'></i>";
            }
            return list.OrderByDescending(o => o.Plate.Length).ToList();
        }
        public async Task<List<EVehicleDto>> DebitInSearch(RFilterModelDto filterModel)
        {
            var listt = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                              join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                                              from vr in vrL.DefaultIfEmpty()
                                              join lFirm in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lFirm.Id into lFirmL
                                              from lFirm in lFirmL.DefaultIfEmpty()
                                              join vd in _vehicleDebitRepository.GetAll() on v.Id equals vd.VehicleId
                                              join u2 in _userRepository.GetAll() on vd.DebitUserId equals u2.Id into uL
                                              from u2 in uL.DefaultIfEmpty()
                                              join unit in _unitRepository.GetAll() on vd.UnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()
                                              where vd.Status && v.Status
                                              select new EVehicleDto()
                                              {
                                                  VehicleId = v.Id,
                                                  RentFirmName = v.FixtureTypeId == (int)FixtureType.ForRent ? lFirm.Name : "Mülkiyet",
                                                  //ParentUnitId = unit2.Id,
                                                  UnitId = unit.Id,
                                                  ParentUnitId = unit2.Id,
                                                  DebitUserId = v.LastUserId,
                                                  FixtureTypeId = v.FixtureTypeId,
                                                  //UnitName = unit2.Code + "/" + unit.Code,
                                                  //Code = unit2.Code,
                                                  DebitState2 = v.LastStatus,
                                                  FixtureName = v.FixtureTypeId == (int)FixtureType.ForRent
                                                     ? "<span class='label bg-pink-300'>Kiralık Araç</span>"
                                                     : "<span class='label bg-brown-300'>Mülkiyet</span>",
                                                  //DebitNameSurname = v.LastUserId != null
                                                  //   ?
                                                  //   "<a data-toggle='modal' href='#PersonModal' onclick='funcGetPerson(" + u2.Id +
                                                  //   ")' class='text-danger'>" + u2.Name + " " + u2.Surname + "/" + u2.MobilePhone + "</a>"
                                                  //   : v.LastStatus == (int)DebitState.Pool
                                                  //       ? "<span class='label bg-primary-300 full-width'>Havuzda</span>"
                                                  //       : "<span class='label bg-danger-300 full-width'>Kullanılmayan Araç</span>",
                                                  Plate = "<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>",
                                                  Search = unit2.Name + " " + unit.Name + " " + u2.Name + " " + u2.Surname + " " + u2.MobilePhone + " " + v.Plate + " " + v.ArventoNo
                                              });

            //Proje bazında filtre
            if (filterModel.UnitId > 0)
                listt = listt.Where(w => w.UnitId == filterModel.UnitId);

            //Müdürlük bazında filtre
            if (filterModel.ParentUnitId > 0)
                listt = listt.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            var result = listt.Where(c => c.Search.ToLower().Contains(filterModel.Search.ToLower())).ToList();
            //son zimmet bilgileri ekleniyor
            foreach (var item in result)
            {
                if (item.DebitState2 == (int)DebitState.Debit && item.DebitUserId > 0)
                {
                    var lastDebitUser = _userRepository.Find(item.DebitUserId.Value);
                    var lastDebitUnit = (from unit in _unitRepository.GetAll()
                                         join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id
                                         where unit.Id == item.UnitId
                                         select new EUnitDto()
                                         {
                                             Name = unit2.Code + "/" + unit.Code
                                         }).FirstOrDefault();

                    item.DebitNameSurname = "<a data-toggle='modal' href='#PersonModal' onclick='funcGetPerson(" + lastDebitUser.Id + ")' class='text-danger'>" + lastDebitUser.Name + " " + lastDebitUser.Surname + "/" +
                          lastDebitUser.MobilePhone + "</a>";
                    item.UnitName = lastDebitUnit.Name;
                }
                else if (item.DebitState2 == (int)DebitState.Pool)
                    item.DebitNameSurname = "<span class='label bg-primary-300 full-width'>Havuzda</span>";
                else if (item.DebitState2 == (int)DebitState.InService)
                    item.DebitNameSurname = "<span class='label bg-danger-300 full-width'>Serviste</span>";
                else
                    item.DebitNameSurname = "<span class='label bg-danger-300 full-width'>Kullanılmayan Araç</span>";
            }

            return result;
        }
        public async Task<List<EVehicleDto>> GetActiveVehicleList(RFilterModelDto filterModel)
        {
            try
            {
                var listEnt = await GetActiveListWithMemoryCache();

                if (filterModel.VehicleId > 0)
                    listEnt = listEnt.Where(w => w.VehicleId == filterModel.VehicleId).ToList();

                //Proje bazında filtre
                if (filterModel.UnitId > 0)
                    listEnt = listEnt.Where(w => w.UnitId == filterModel.UnitId).ToList();

                //Müdürlük bazında filtre
                if (filterModel.ParentUnitId > 0)
                    listEnt = listEnt.Where(w => w.ParentUnitId == filterModel.ParentUnitId).ToList();

                foreach (var item in listEnt)
                {
                    if (item.LastStatus == (int)DebitState.Pool)
                        item.DebitNameSurname = "<span class='label bg-primary-300 full-width'>Havuzda</span>";
                    else if (item.LastStatus == (int)DebitState.InService)
                        item.DebitNameSurname = "<span class='label bg-danger-300 full-width'>Serviste</span>";

                    if (item.LastCityId > 0)
                    {
                        var lastTrip = _tripRepository.Where(w => w.Status && w.VehicleId == item.VehicleId).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();

                        if (lastTrip != null)
                        {
                            if (lastTrip.State == (int)TripState.StartTrip)
                                item.Plate = item.Plate.Replace(item.Plate2, (item.Plate2 + "<br><div style='color:red;font-size: 11px;'>" + item.LastCityName + "/" + lastTrip.StartDate.ToString("dd.MM.yyyy") + "</div>"));
                            else if (lastTrip.State == (int)TripState.EndTrip)
                                item.Plate = item.Plate.Replace(item.Plate2, (item.Plate2 + "<br><div style='color:green;font-size: 11px;'>" + item.LastCityName + "/" + lastTrip.EndDate.Value.ToString("dd.MM.yyyy") + "</div>"));
                        }
                    }
                }

                return listEnt;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<EVehicleDto>> GetActiveListWithMemoryCache()
        {
            var activeList = new List<EVehicleDto>();
            string theKey = MemoryCache_.ActiveVehicleList.ToString();
            var listEnt = await _cacheService.GetOrSetAsync(theKey, async () =>
            {
                activeList = await Task.FromResult((from v in _vehicleRepository.GetAll()

                                                    join c in _cityRepository.GetAll() on v.LastCityId equals c.Id into cL
                                                    from c in cL.DefaultIfEmpty()
                                                    join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                                                    from c2 in c2L.DefaultIfEmpty()

                                                    join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                                                    from vr in vrL.DefaultIfEmpty()
                                                    join u2 in _userRepository.GetAll() on v.LastUserId equals u2.Id into uL
                                                    from u2 in uL.DefaultIfEmpty()
                                                    join lFirm in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lFirm.Id into lFirmL
                                                    from lFirm in lFirmL.DefaultIfEmpty()
                                                    join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                                                    from unit in unitL.DefaultIfEmpty()
                                                    join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                                    from unit2 in unit2L.DefaultIfEmpty()
                                                    where v.Status //&& v.LastUnitId != null
                                                    select new EVehicleDto
                                                    {
                                                        VehicleId = v.Id,
                                                        RentFirmName = v.FixtureTypeId == (int)FixtureType.ForRent ? lFirm.Name : "Mülkiyet",
                                                        ParentUnitId = unit2.Id,
                                                        UnitId = unit.Id,
                                                        UnitParentId = unit2.Id,
                                                        ArventoNo = v.ArventoNo,
                                                        MaxSpeed = v.MaxSpeed,
                                                        LastCityId = v.LastCityId,
                                                        LastCityName = c2.Name + "-" + c.Name,
                                                        DebitUserId = v.LastUserId,
                                                        FixtureTypeId = v.FixtureTypeId,
                                                        UnitName = unit2.Code == null ? "-" : (unit2.Code + "/" + unit.Code),
                                                        Code = unit2.Code,
                                                        LastStatus = v.LastStatus,
                                                        FixtureName = v.FixtureTypeId == (int)FixtureType.ForRent
                                                            ? "<span class='label bg-pink-300'>Kiralık Araç</span>"
                                                            : "<span class='label bg-brown-300'>Mülkiyet</span>",
                                                        DebitNameSurname = "<a data-toggle='modal' href='#PersonModal' onclick='funcGetPerson(" + u2.Id +
                                                            ")' class='text-black'>" + u2.Name + " " + u2.Surname + "/" + u2.MobilePhone + "</a>",
                                                        DebitNameSurname2 = u2.Name + " " + u2.Surname + "/" + u2.MobilePhone,
                                                        Plate = "<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>",
                                                        Plate2 = v.Plate
                                                    }).ToList());

                return activeList;
            });

            return listEnt;
        }
        public async Task<EUserDto> GetCachedMemoryActiveUser()
        {
            var activeUser = new EUserDto();
            string theKey = MemoryCache_.ActiveUser.ToString();
            var result = await _cacheService.GetOrSetAsync(theKey, async () =>
            {
                activeUser = null;
                return activeUser;
            });

            return result;
        }
        public List<EVehicleDto> GetLastDebitUserHistory(int vehicleId)
        {
            try
            {
                var result = (from vd in _vehicleDebitRepository.GetAll()
                              join v in _vehicleRepository.GetAll() on vd.VehicleId equals v.Id
                              join u in _userRepository.GetAll() on vd.DebitUserId equals u.Id
                              join u2 in _userRepository.GetAll() on vd.CreatedBy equals u2.Id
                              join du in _userRepository.GetAll() on vd.DeliveryUserId equals du.Id into duL
                              from du in duL.DefaultIfEmpty()
                              join c in _cityRepository.GetAll() on vd.CityId equals c.Id into cL
                              from c in cL.DefaultIfEmpty()
                              join un in _unitRepository.GetAll() on vd.UnitId equals un.Id
                              join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id
                              where vd.Status && vd.VehicleId == vehicleId
                              select new EVehicleDto
                              {
                                  VehicleDebitId = vd.Id,
                                  UnitName = un1.Name + "-" + un.Name,
                                  DeliveryUserName = du.Name != null ? du.Name + " " + du.Surname : "",
                                  CreatedUserName = u2.Name + " " + u2.Surname,
                                  CreatedDate = vd.CreatedDate,
                                  DebitNameSurname = u.Name + " " + u.Surname + "<br/>" + u.MobilePhone,
                                  DebitCreatedDate = vd.CreatedDate,
                                  DebitStartDate = vd.StartDate,
                                  DebitEndDate = vd.EndDate,
                                  UnitId = vd.UnitId,
                                  Plate = v.Plate,
                                  DebitState = vd.State == (int)DebitState.Debit
                                      ? u.Name + " " + u.Surname + "<br/>" + u.MobilePhone
                                      : "<span style='width: 100%;' class='label bg-orange-800'>Havuz</span>",
                                  DebitState2 = vd.State,
                                  CityName = c.Name ?? "",
                                  DebitUserId = vd.DebitUserId
                              }).OrderByDescending(o => o.VehicleDebitId).Take(10).ToList();
                return result;
            }
            catch (Exception)
            {
                return new List<EVehicleDto>();
            }
        }
        public async Task<List<EVehicleDto>> GetLastVehicleDebit(RFilterModelDto filterModel)
        {
            var list = from vd in _vehicleDebitRepository.GetAll()
                       join v in _vehicleRepository.GetAll() on vd.VehicleId equals v.Id
                       join u in _userRepository.GetAll() on vd.DebitUserId equals u.Id
                       join u2 in _userRepository.GetAll() on vd.CreatedBy equals u2.Id
                       join du in _userRepository.GetAll() on vd.DeliveryUserId equals du.Id into duL
                       from du in duL.DefaultIfEmpty()
                       join unit in _unitRepository.GetAll() on vd.UnitId equals unit.Id into unitL
                       from unit in unitL.DefaultIfEmpty()
                       join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                       from unit2 in unit2L.DefaultIfEmpty()
                       where vd.Status && v.Status
                       select new EVehicleDto
                       {
                           Plate = v.Plate,
                           DebitState2 = vd.State,
                           CreatedDate = vd.CreatedDate,
                           DebitEndDate = vd.EndDate,
                           DebitNameSurname = u.Name + " " + u.Surname + "/" + u.MobilePhone,
                           UnitName = unit.Name + "-" + unit.Name,
                           VehicleDebitId = vd.Id,
                           DeliveryUserName = du.Name != null ? du.Name + " " + du.Surname : "",
                           CreatedUserName = u2.Name + " " + u2.Surname,
                           DebitCreatedDate = vd.CreatedDate,
                           DebitStartDate = vd.StartDate,
                           UnitId = vd.UnitId,
                           ParentUnitId = unit2.Id,
                           DebitUserId = vd.DebitUserId
                       };

            //Proje bazında filtre
            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            //Müdürlük bazında filtre
            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            return await Task.FromResult(list.OrderByDescending(o => o.VehicleDebitId).Take(10).ToList());
        }

        #endregion

        #region VehicleReport Page
        public async Task<List<RVehicleDto>> RentVehicleFirmCount(RFilterModelDto model)
        {
            var list = await GetActiveVehicleList(model);
            var rentFirm = (from d in list.Where(w => w.FixtureTypeId == (int)FixtureType.ForRent).GroupBy(g => g.RentFirmName)
                            select new RVehicleDto()
                            {
                                Name = d.First().RentFirmName,
                                Count = d.GroupBy(g => g.VehicleId).Count()
                            }).OrderByDescending(o => o.Count).ToList();

            return rentFirm;
        }
        public async Task<List<RVehicleDto>> ManagementVehicleCount(RFilterModelDto model)
        {
            var list = await GetActiveVehicleList(model);
            var management = list.GroupBy(g => g.Code)
                .Select(s => new RVehicleDto()
                {
                    UnitName = s.First().Code ?? "Havuz",
                    Name = s.First().UnitName,
                    UnitId = s.First().UnitId,
                    ParentUnitId = s.First().ParentUnitId,
                    Plates = s.GroupBy(g => g.VehicleId).Select(s => s.First().Plate2).ToList(),
                    Count = s.GroupBy(g => g.VehicleId).Count()
                }).Where(w => w.Count > 0).ToList();

            return management;
        }
        public async Task<List<RVehicleDto>> UsageTypeVehicleCount(RFilterModelDto model)
        {
            var vehicleList = await GetActiveVehicleList(model);

            var list = new List<RVehicleDto>();
            foreach (var item in vehicleList)
            {
                var lastDebit = (from vd in _vehicleDebitRepository.GetAll()
                                 join l in _lookUpListRepository.GetAll() on vd.UsageTypeId equals l.Id
                                 where vd.Status && vd.VehicleId == item.VehicleId
                                 select new RVehicleDto()
                                 {
                                     Id = vd.Id,
                                     UsageTypeId = vd.UsageTypeId,
                                     Name = l.Name,
                                     VehicleId = vd.VehicleId
                                 }).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                if (lastDebit != null)
                    list.Add(lastDebit);
            }

            var usageType = (from d in list.GroupBy(g => g.UsageTypeId)
                             select new RVehicleDto()
                             {
                                 Name = d.First().Name,
                                 Count = d.GroupBy(g => g.VehicleId).Count()
                             }).ToList();
            return usageType;
        }
        public async Task<List<RVehicleDto>> FixVehicleTotalAmount(RFilterModelDto model)
        {
            var list = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                             join va in _vehicleAmountRepository.GetAll() on v.Id equals va.VehicleId
                                             join vt in _vehicleTransferLogRepository.GetAll() on v.Id equals vt.VehicleId into vtL
                                             from vt in vtL.DefaultIfEmpty()
                                             join l in _lookUpListRepository.GetAll() on vt.TransferTypeId equals l.Id into lL
                                             from l in lL.DefaultIfEmpty()
                                             where v.FixtureTypeId == (int)FixtureType.Ownership
                                             select new RVehicleDto()
                                             {
                                                 Status = v.Status,
                                                 Name = l.Name,
                                                 Amount = va.Amount
                                             });

            var activeVehicle = new RVehicleDto()
            {
                Amount = list.Where(w => w.Status == true).Sum(s => s.Amount), //aktif araç tutar
                Name = "Aktif Araç"
            };

            var groupSale = list.Where(w => w.Status == false).ToList()//silinmiş araç tutar
                .GroupBy(g => g.Name)
                .Select(s => new RVehicleDto()
                {
                    Name = s.First().Name,
                    Amount = s.Sum(s => s.Amount)
                }).ToList();

            groupSale.Add(activeVehicle);
            return groupSale;
        }
        public async Task<List<RVehicleDto>> ModelYearVehicleCount(RFilterModelDto model)
        {
            var list = await Task.FromResult((from v in _vehicleRepository.GetAll()
                                              where v.Status
                                              select new RVehicleDto()
                                              {
                                                  VehicleModelYear = v.ModelYear
                                              }).ToList());

            var groupByModelYear = list.GroupBy(g => g.VehicleModelYear)
                .Select(s => new RVehicleDto()
                {
                    VehicleModelYear = s.First().VehicleModelYear ?? "Belirsiz",
                    Count = s.Count()
                }).OrderByDescending(o => o.VehicleModelYear).ToList();

            return groupByModelYear;
        }
        public async Task<List<RVehicleDto>> ModelYearPercentVehicleCount(RFilterModelDto model)
        {
            var dateNowYear = DateTime.Now.Year;
            var list = await Task.FromResult((from v in _vehicleRepository.GetAll()
                                              where v.Status
                                              select new RVehicleDto()
                                              {
                                                  Count = string.IsNullOrEmpty(v.ModelYear) ? -1 : dateNowYear - Int32.Parse(v.ModelYear)
                                              }).ToList());

            var zeroVs3Year = list.Where(w => w.Count >= 0 && w.Count <= 3).Count();
            var fourVs6Year = list.Where(w => w.Count >= 4 && w.Count <= 6).Count();
            var sixThenUp = list.Where(w => w.Count > 6).Count();
            var noModelYear = list.Where(w => w.Count == -1).Count();

            var allVehicleCount = (decimal)list.Count();
            var result = new List<RVehicleDto>();
            result.Add(new RVehicleDto() { VehicleModelYear = " 0-3 Yaş", Amount = (decimal)((decimal)zeroVs3Year * 100 / allVehicleCount) });
            result.Add(new RVehicleDto() { VehicleModelYear = " 4-6 Yaş", Amount = (decimal)((decimal)fourVs6Year * 100) / allVehicleCount });
            result.Add(new RVehicleDto() { VehicleModelYear = " 7 ve Üzeri", Amount = (decimal)((decimal)sixThenUp * 100) / allVehicleCount });
            result.Add(new RVehicleDto() { VehicleModelYear = " Bilinmeyen", Amount = (decimal)((decimal)noModelYear * 100) / allVehicleCount });

            return result.Where(w => w.Amount > 0).ToList();
        }

        #endregion

        #region VehicleFuel Page
        public async Task<RDashboardDto> HeaderInfoFuel(RFilterModelDto model)
        {
            //const string cacheKey = "HeaderInfoFuel";
            //var cacheList = GetMemoryCache(cacheKey);
            //if (cacheList != null)
            //    return (RDashboardDto)cacheList;

            model = SetStartEndDate(model);
            var fuelList = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleFuelDto>();
            foreach (var item in debitList)
            {
                var temp = fuelList.Where(w =>
                    w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList();
                temp.ForEach(f => f.UnitName = item.UnitName);
                result.AddRange(temp);
            }

            var mostProjectName = result.GroupBy(d => d.UnitName)
                .Select(g => new
                {
                    Key = g.First().UnitName,
                    Value = g.Sum(s => s.DiscountAmount)
                }).OrderByDescending(o => o.Value).Take(1).FirstOrDefault();

            var mostPlateAmount = result.GroupBy(d => d.VehicleId)
                .Select(g => new
                {
                    Key = g.First().Plate,
                    Value = g.Sum(s => s.DiscountAmount)
                }).OrderByDescending(o => o.Value).Take(1).FirstOrDefault();

            var totalKm = result.Sum(s => s.Km);

            var totalLiter = result.Sum(s => s.Liter);

            var plateCount = result.GroupBy(d => d.VehicleId).Count();
            var totalAmount = result.Count > 0 ? result.Sum(s => s.Amount) : 0;
            var result2 = new RDashboardDto
            {
                DateMonth = model.StartDate.ToString("MMM yyyy"),//Kullanılıyor
                DifferenceAmount = (model.ParentUnitId == null && model.IsAdmin) ? (result.Count > 0 ? (fuelList.Sum(s => s.Amount) - result.Sum(s => s.Amount)) : 0) : 0,
                TotalAmount = totalAmount,
                TotalKm = totalKm,
                TotalLiter = totalLiter,
                DiscountTotalAmount = result.Count > 0 ? result.Sum(s => s.DiscountAmount) : 0,
                MostPlateAmount = mostPlateAmount != null ? new EFuelLogDto { Plate = mostPlateAmount.Key, TotalAmount = mostPlateAmount.Value } : new EFuelLogDto { Plate = "", TotalAmount = 0 },
                MostProjectAmount = mostProjectName != null ? new EFuelLogDto { UnitName = mostProjectName.Key, TotalAmount = mostProjectName.Value } : new EFuelLogDto { UnitName = "", TotalAmount = 0 },
                PercentageUnit = mostProjectName?.Value / totalAmount ?? 0,
                PercentagePlate = mostPlateAmount?.Value / totalAmount ?? 0,
                PlateCount = plateCount.ToString(),
                DebitVehicleCount = debitList.GroupBy(g => g.VehicleId).Count(),
            };
            //AddMemoryCache(cacheKey, result2);
            return result2;
        }
        public async Task<List<RVehicleFuelDto>> GetFuelListRange(RFilterModelDto model)
        {
            //Aktif-Pasif araçlar tüm datalar listelenmeli
            var list = await Task.FromResult(from fl in _fuelLogRepository.GetAll()
                                             join v in _vehicleRepository.GetAll() on fl.VehicleId equals v.Id
                                             join le in _lookUpListRepository.GetAll() on v.EnginePowerId equals le.Id
                                             join l in _lookUpListRepository.GetAll() on fl.FuelStationId equals l.Id
                                             where fl.Status == true
                                             select new RVehicleFuelDto
                                             {
                                                 VehicleId = v.Id,
                                                 Plate = v.Plate,
                                                 FuelDate = fl.TransactionDate,
                                                 Amount = fl.TotalAmount,
                                                 Km = fl.Km ?? 0,
                                                 Liter = fl.Liter ?? 0,
                                                 EnginePowerName = le.Name,
                                                 EnginePowerId = le.Id,
                                                 SupplierName = l.Name,
                                                 SupplierId = l.Id,
                                                 FuelStationId = fl.FuelStationId,
                                                 IsPublisher = fl.IsPublisher,
                                                 DiscountPercent = fl.DiscountPercent,
                                                 DiscountAmount = fl.TotalAmount - fl.TotalAmount * fl.DiscountPercent / 100
                                             });
            if (model.FuelStationId > 0)
                list = list.Where(w => w.FuelStationId == model.FuelStationId);

            if (model.EnginePowerId > 0)
                list = list.Where(w => w.EnginePowerId == model.EnginePowerId);

            if (!model.IsAdmin)//admin değilse yayınlanmamış verileri gösterme
                list = list.Where(w => w.IsPublisher == true);

            if (model.StartDate != DateTime.MinValue)
                list = list.Where(w => w.FuelDate >= model.StartDate && w.FuelDate < model.EndDate);

            if (model.VehicleId > 0)
                list = list.Where(w => w.VehicleId == model.VehicleId);

            return list.ToList();
        }
        public async Task<List<RVehicleFuelDto>> MontlyFuelTotalAmount(RFilterModelDto model)
        {
            //var cacheKey = "MontlyFuelTotalAmount";
            //var cacheList = GetMemoryCache(cacheKey);
            //if (cacheList != null)
            //    return (List<RVehicleFuelDto>)cacheList;

            model = SetStartEndDate(model);
            var fuelList = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleFuelDto>();
            foreach (var item in debitList)
                result.AddRange(fuelList.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            result = result.GroupBy(g => g.FuelDate.ToString("MMM yyyy"))
                .Select(g => new RVehicleFuelDto
                {
                    FuelDate = g.First().FuelDate,
                    FuelDate2 = g.First().FuelDate.ToString("MMM yyyy"),
                    //Amount = g.Sum(s => s.TotalAmount),
                    DiscountAmount = g.Sum(s => s.DiscountAmount)
                }).OrderBy(o => o.FuelDate).ToList();
            //AddMemoryCache(cacheKey, result);
            return result;
        }//Aylık yakıt raporu
        public async Task<List<RVehicleFuelDto>> ProjectFuelTotalAmount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            var fuellist = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);

            var management = debitList.GroupBy(g => g.UnitName).Select(s => new RVehicleFuelDto()
            {
                UnitName = s.Key,
                //Plates = new List<string>(),
                Amount = 0
            }).ToList();

            management.Add(new RVehicleFuelDto() { UnitName = "Havuz", /*Plates = new List<string>(),*/ Amount = 0 });
            foreach (var item in debitList)
            {
                var temp = fuellist.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList();
                if (temp.Count > 0)
                {
                    if (item.State == (int)DebitState.Debit || item.State == (int)DebitState.InService)
                    {
                        if (string.IsNullOrEmpty(item.UnitName))//Servisteki araç havuzdan sonra servise alındıysa tutar havuza yazılır
                            management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.DiscountAmount);
                        else
                            management.Where(w => w.UnitName == item.UnitName).FirstOrDefault().Amount += temp.Sum(s => s.DiscountAmount);
                    }
                    else if (item.State == (int)DebitState.Pool /**/)
                    {
                        management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.DiscountAmount);
                    }
                }
            }

            management = management.Where(w => w.Amount > 0).OrderBy(o => o.Amount).ToList();
            if (management.Count == 0)
                management.Add(new RVehicleFuelDto()
                {
                    UnitName = "",
                    Amount = 0,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    //DateMonth = model.DateMonth
                });
            return management;
        }//Müdürlük bazında rapor
        public async Task<List<RVehicleFuelDto>> SubProjectFuelTotalAmount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            var fuellist = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);
            var management = debitList.GroupBy(g => g.ProjectName).Select(s => new RVehicleFuelDto()
            {
                UnitName = s.Key,
                Amount = 0,
            }).ToList();

            management.Add(new RVehicleFuelDto() { UnitName = "Havuz", Amount = 0 });
            foreach (var item in debitList)
            {
                var temp = fuellist.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList();
                if (temp.Count > 0)
                {
                    if (item.State == (int)DebitState.Debit || item.State == (int)DebitState.InService)
                    {
                        if (string.IsNullOrEmpty(item.UnitName))//Havuzdan sonra servise alındıysa tutar havuza yazılır
                            management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.DiscountAmount);
                        else
                            management.Where(w => w.UnitName == item.ProjectName).FirstOrDefault().Amount += temp.Sum(s => s.DiscountAmount);

                    }
                    else if (item.State == (int)DebitState.Pool /*|| item.State == (int)DebitState.InService*/)
                        management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.DiscountAmount);
                }
            }

            management = management.Where(w => w.Amount > 0).OrderBy(o => o.Amount).ToList();
            if (management.Count == 0)
                management.Add(new RVehicleFuelDto()
                {
                    UnitName = "",
                    Amount = 0,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    //DateMonth = model.DateMonth
                });
            return management;
        }//Proje bazında rapor
        public async Task<List<RVehicleFuelDto>> VehicleFuelTotalAmount(RFilterModelDto model)
        {
            //var cacheKey = "VehicleFuelTotalAmount";
            //var cacheList = GetMemoryCache(cacheKey);
            //if (cacheList != null)
            //    return (List<RVehicleFuelDto>)cacheList;

            model = SetStartEndDate(model);
            var fuelList = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleFuelDto>();
            foreach (var item in debitList)
                result.AddRange(fuelList.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            var enginePowList = result.GroupBy(g => g.EnginePowerId).Select(s => new REnginePowerDto()
            {
                EnginePowerId = s.First().EnginePowerId,
                EnginePowerName = s.First().EnginePowerName
            }).OrderBy(o => o.EnginePowerId).ToList();

            result = result.GroupBy(g => g.VehicleId)
                .Select(s => new RVehicleFuelDto
                {
                    VehicleId = s.First().VehicleId,
                    Plate = s.First().Plate,
                    Amount = s.Sum(s => s.DiscountAmount),
                    Liter = s.Sum(s => s.Liter)
                }).OrderBy(o => o.Plate).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            result = SamePlateForNewNameFuel(result);

            if (result.Count > 0)
                result[0].EnginePowlist = enginePowList;

            //AddMemoryCache(cacheKey, result);
            return result;
        }//Plaka bazlı Yakıt 
        public async Task<List<RVehicleFuelDto>> VehicleFuelTotalKmSpend(RFilterModelDto model)
        {
            //var cacheKey = "VehicleFuelTotalKmSpend";
            //var cacheList = GetMemoryCache(cacheKey);
            //if (cacheList != null)
            //    return (List<RVehicleFuelDto>)cacheList;

            model = SetStartEndDate(model);
            var fuelList = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleFuelDto>();
            foreach (var item in debitList)
                result.AddRange(fuelList.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            var groupByPlate = result.Where(w => w.Km > 0).ToList().GroupBy(g => g.VehicleId);
            result = groupByPlate.Select(s => new RVehicleFuelDto
            {
                VehicleId = s.First().VehicleId,
                Plate = s.First().Plate,
                KmSpend = s.Sum(s => s.Amount - s.Amount * s.DiscountPercent / 100) / s.Sum(s => s.Km),
                Liter = s.Sum(s => s.Liter) / s.Sum(s => s.Km) * 100
            }).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Aynı plakada araç kiralanabilir veya eklenebilir)
            result = SamePlateForNewNameFuel(result);

            //Seçilen motor cc arasındaki ortalama km tutar
            if (model.IsAverageKmAmount && result.Count > 0)
            {
                var plateCount = result.Count();
                var totalKmSpent = result.Sum(s => s.KmSpend);
                var totalLiter = result.Sum(s => s.Liter);
                result.ForEach(f => f.AverageKmAmount = (totalKmSpent / plateCount));
                result.ForEach(f => f.AverageLiter = (totalLiter / plateCount));
            }
            //AddMemoryCache(cacheKey, result);
            return result;
        } //Plaka bazlı Km harcama 

        public async Task<List<RVehicleFuelDto>> VehicleFuelTotalSupplierAmount(RFilterModelDto model)
        {
            //var cacheKey = "VehicleFuelTotalSupplierAmount";
            //var cacheList = GetMemoryCache(cacheKey);
            //if (cacheList != null)
            //    return (List<RVehicleFuelDto>)cacheList;

            model = SetStartEndDate(model);
            var fuelList = await GetFuelListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleFuelDto>();
            foreach (var item in debitList)
                result.AddRange(fuelList.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            result = result.GroupBy(g => g.SupplierId)
                .Select(s => new RVehicleFuelDto
                {
                    SupplierName = s.First().SupplierName,
                    Amount = s.Sum(p => p.Amount)
                }).ToList();

            //AddMemoryCache(cacheKey, result);
            return result;
        }   //Tedarikçi bazlı yakıt harcama 
        public List<RVehicleFuelDto> SamePlateForNewNameFuel(List<RVehicleFuelDto> resultPlate)
        {
            var isSamePlate = _vehicleRepository.GetAll().Select(s => new Vehicle() { Plate = s.Plate }).ToList().GroupBy(g => g.Plate)
                .Select(s => new RVehicleFuelDto() { Count = s.Count(), Plate = s.First().Plate })
                .Where(w => w.Count > 1).OrderBy(o => o.VehicleId).ToList();

            foreach (var p in isSamePlate)
            {
                var samePlate = resultPlate.Where(w => w.Plate == p.Plate).ToList();
                for (int i = 0; i < samePlate.Count; i++)
                {
                    var temp = resultPlate.FirstOrDefault(f => f.VehicleId == samePlate[i].VehicleId);
                    if (temp != null)
                        resultPlate.FirstOrDefault(f => f.VehicleId == temp.VehicleId).Plate += "-" + temp.VehicleId;
                }
            }

            return resultPlate;
        }//Aynı plakalara 1,2,3 gibi ekler getirir
        public RFilterModelDto SetStartEndDate(RFilterModelDto model)
        {
            if (model.StartDate == DateTime.MinValue) //first load page
            {
                var dateNow = DateTime.Now;
                model.StartDate = new DateTime(dateNow.AddMonths(-2).Year, dateNow.AddMonths(-2).Month, 1);
                model.EndDate = dateNow;// new DateTime(2020, 12, 31);
                                        //model.DateMonth = model.StartDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("tr")) + "-" +
                                        //            model.EndDate.Value.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("tr"));
            }
            return model;
        }
        public async Task<List<RVehicleFuelDto>> VehicleDebitAndMonthlyFuelCompare(RFilterModelDto model)
        {
            var list = new List<RVehicleFuelDto>();
            if (model.ParentUnitId == null)
            {
                model = SetStartEndDate(model);
                var fuelList = await GetFuelListRange(model);
                var debitList = await GetDebitListRange(model);

                var result = new List<RVehicleFuelDto>();
                foreach (var item in debitList)
                    result.AddRange(fuelList.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

                if (fuelList.Sum(s => s.Amount) != result.Sum(s => s.Amount))
                {
                    foreach (var item in fuelList)
                    {
                        var debit = result.Where(w => w.VehicleId == item.VehicleId && w.FuelDate == item.FuelDate).ToList();
                        var ss = debit.Where(w => item.FuelDate > w.StartDate || w.EndDate <= item.FuelDate).FirstOrDefault();
                        if (ss == null)
                        {
                            var debitInfo = debitList.Where(w => w.VehicleId == item.VehicleId).FirstOrDefault();
                            var temp = new RVehicleFuelDto()
                            {
                                Plate = item.Plate,
                                Amount = item.Amount,
                                FuelDate = item.FuelDate,
                                DebitDateRange = debitInfo == null ? "<span class='label bg-slate-800'>Tarih aralığında zimmeti bulunamadı</span>" : ("<span class='label bg-success-300'>" + debitInfo.StartDate.ToString("dd/MM/yyyy") + "</span> - <span class='label bg-success-300'>" + debitInfo.EndDate.ToString("dd/MM/yyyy") + "</span>"),
                            };
                            list.Add(temp);
                        }

                    }
                }
            }
            return list.OrderBy(o => o.FuelDate).ToList();
        }
        #endregion

        #region VehicleMaintenance Page
        public async Task<RDashboardDto> HeaderInfoMaintenance(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.EndDate = model.EndDate.AddDays(1);
            var mainList = await GetMaintenanceListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleMaintenanceDto>();
            foreach (var item in debitList)
            {
                var temp = mainList.Where(w =>
                    w.InvoiceDate >= item.StartDate && w.InvoiceDate < item.EndDate && w.VehicleId == item.VehicleId).ToList();
                temp.ForEach(f => f.UnitName = item.UnitName);
                result.AddRange(temp);
            }

            var mostProjectAmount = result.GroupBy(d => d.UnitName)
                .Select(g => new EFuelLogDto()
                {
                    UnitName = g.First().UnitName,
                    TotalAmount = g.Sum(s => s.Amount),
                    //UserFaultAmount = g.Sum(s => s.UserFaultAmount)??0
                }).OrderByDescending(o => o.TotalAmount).Take(1).FirstOrDefault();

            var mostPlateAmount = result.GroupBy(d => d.VehicleId)
              .Select(g => new EFuelLogDto()
              {
                  Plate = g.First().Plate,
                  TotalAmount = g.Sum(s => s.Amount),
                  UserFaultAmount = g.Sum(s => s.UserFaultAmount) ?? 0
              }).OrderByDescending(o => o.TotalAmount).Take(1).FirstOrDefault();

            var plateCount = result.GroupBy(d => d.VehicleId).Count();
            var totalAmount = result.Count > 0 ? result.Sum(s => s.Amount) : 0;
            var result2 = new RDashboardDto
            {
                DateMonth = model.StartDate.ToString("MMM yyyy"),//Kullanılıyor
                TotalAmount = totalAmount,
                MostPlateAmount = mostPlateAmount != null ? new EFuelLogDto { Plate = mostPlateAmount.Plate, TotalAmount = mostPlateAmount.TotalAmount, UserFaultAmount = mostPlateAmount.UserFaultAmount } : new EFuelLogDto { Plate = "", TotalAmount = 0, UserFaultAmount = 0 },
                MostProjectAmount = mostProjectAmount != null ? new EFuelLogDto { UnitName = mostProjectAmount.UnitName, TotalAmount = mostProjectAmount.TotalAmount } : new EFuelLogDto { UnitName = "", TotalAmount = 0 },
                PercentageUnit = mostProjectAmount?.TotalAmount / totalAmount ?? 0,
                PercentagePlate = mostPlateAmount?.TotalAmount / totalAmount ?? 0,
                PlateCount = plateCount.ToString(),
                DebitVehicleCount = debitList.GroupBy(g => g.VehicleId).Count(),
                UserFaultAmount = result.Sum(s => s.UserFaultAmount) ?? 0
            };
            return result2;
        }
        //Aylık rapor
        public async Task<List<RVehicleMaintenanceDto>> MontlyMaintenanceTotalAmount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.EndDate = model.EndDate.AddDays(1);
            var mainList = await GetMaintenanceListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleMaintenanceDto>();
            foreach (var item in debitList)
                result.AddRange(mainList.Where(w => w.InvoiceDate >= item.StartDate && w.InvoiceDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            result = result.GroupBy(g => g.InvoiceDate2)
                .Select(g => new RVehicleMaintenanceDto
                {
                    InvoiceDate = g.First().InvoiceDate,//silme
                    InvoiceDate2 = g.First().InvoiceDate.ToString("MMM yyyy"),
                    Amount = g.Sum(s => s.Amount)
                }).OrderBy(o => o.InvoiceDate).ToList();

            return result;
        }
        //Müdürlük bazında rapor
        public async Task<List<RVehicleMaintenanceDto>> ProjectMaintenanceTotalAmount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.EndDate = model.EndDate.AddDays(1);
            var mainlist = await GetMaintenanceListRange(model);
            var debitList = await GetDebitListRange(model);

            var management = debitList.GroupBy(g => g.UnitName).Select(s => new RVehicleMaintenanceDto()
            {
                UnitName = s.Key,
                Amount = 0,
                UserFaultAmount = 0
            }).ToList();

            management.Add(new RVehicleMaintenanceDto() { UnitName = "Havuz", /*Plates = new List<string>(),*/ Amount = 0 });
            foreach (var item in debitList)
            {
                var temp = mainlist.Where(w => w.InvoiceDate >= item.StartDate && w.InvoiceDate < item.EndDate && w.VehicleId == item.VehicleId).ToList();
                if (temp.Count > 0)
                {
                    if (item.State == (int)DebitState.Debit || item.State == (int)DebitState.InService)
                    {
                        if (string.IsNullOrEmpty(item.UnitName))//Havuzdan sonra servise alındıysa tutar havuza yazılır
                        {
                            management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.Amount);
                            management.Where(w => w.UnitName == "Havuz").FirstOrDefault().UserFaultAmount += temp.Sum(s => s.UserFaultAmount);
                        }
                        else
                        {
                            management.Where(w => w.UnitName == item.UnitName).FirstOrDefault().Amount += temp.Sum(s => s.Amount);
                            management.Where(w => w.UnitName == item.UnitName).FirstOrDefault().UserFaultAmount += temp.Sum(s => s.UserFaultAmount);
                        }
                    }
                    else if (item.State == (int)DebitState.Pool)
                    {
                        management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.Amount);
                        management.Where(w => w.UnitName == "Havuz").FirstOrDefault().UserFaultAmount += temp.Sum(s => s.UserFaultAmount);
                    }
                }
            }

            management = management.Where(w => w.Amount > 0).OrderBy(o => o.Amount).ToList();
            if (management.Count == 0)
                management.Add(new RVehicleMaintenanceDto()
                {
                    UnitName = "",
                    Amount = 0,
                    UserFaultAmount = 0,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    //DateMonth = model.DateMonth
                });
            return management;
        }
        //Proje bazında rapor
        public async Task<List<RVehicleMaintenanceDto>> SubProjectMaintenanceTotalAmount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.EndDate = model.EndDate.AddDays(1);
            var mainlist = await GetMaintenanceListRange(model);
            var debitList = await GetDebitListRange(model);

            var management = debitList.GroupBy(g => g.ProjectName).Select(s => new RVehicleMaintenanceDto()
            {
                UnitName = s.Key,
                //Plates = new List<string>(),
                Amount = 0,
                UserFaultAmount = 0,
                DateMonth = s.First().DateMonth
            }).ToList();

            management.Add(new RVehicleMaintenanceDto() { UnitName = "Havuz", /*Plates = new List<string>(),*/ Amount = 0 });
            foreach (var item in debitList)
            {
                var temp = mainlist.Where(w => w.InvoiceDate >= item.StartDate && w.InvoiceDate < item.EndDate && w.VehicleId == item.VehicleId).ToList();
                if (temp.Count > 0)
                {
                    if (item.State == (int)DebitState.Debit || item.State == (int)DebitState.InService)
                    {
                        if (string.IsNullOrEmpty(item.UnitName))//Havuzdan sonra servise alındıysa tutar havuza yazılır
                        {
                            management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.Amount);
                            management.Where(w => w.UnitName == "Havuz").FirstOrDefault().UserFaultAmount += temp.Sum(s => s.UserFaultAmount);
                        }
                        else
                        {
                            management.Where(w => w.UnitName == item.ProjectName).FirstOrDefault().Amount += temp.Sum(s => s.Amount);
                            management.Where(w => w.UnitName == item.ProjectName).FirstOrDefault().UserFaultAmount += temp.Sum(s => s.UserFaultAmount);
                        }
                    }
                    else if (item.State == (int)DebitState.Pool)
                    {
                        management.Where(w => w.UnitName == "Havuz").FirstOrDefault().Amount += temp.Sum(s => s.Amount);
                        management.Where(w => w.UnitName == "Havuz").FirstOrDefault().UserFaultAmount += temp.Sum(s => s.UserFaultAmount);
                    }
                }
            }

            management = management.Where(w => w.Amount > 0).OrderBy(o => o.Amount).ToList();
            if (management.Count == 0)
                management.Add(new RVehicleMaintenanceDto()
                {
                    UnitName = "",
                    Amount = 0,
                    UserFaultAmount = 0,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    DateMonth = model.DateMonth
                });
            return management;
        }
        //Plaka bazında toplam
        public async Task<object> VehicleMaintenanceTotalAmount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.EndDate = model.EndDate.AddDays(1);
            var mainList = await GetMaintenanceListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleMaintenanceDto>();
            foreach (var item in debitList)
                result.AddRange(mainList.Where(w => w.InvoiceDate >= item.StartDate && w.InvoiceDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            if (model.IsTotalAmountForPlate) //plaka bazlı toplam tutarı toplayarak listeler
            {
                result = result.GroupBy(g => g.VehicleId)
                               .Select(s => new RVehicleMaintenanceDto
                               {
                                   Plate = s.First().Plate,
                                   Amount = s.Sum(s => s.Amount),
                                   UserFaultAmount = s.Sum(s => s.UserFaultAmount) == 0 ? null : s.Sum(s => s.UserFaultAmount),
                                   VehicleId = s.First().VehicleId
                               }).OrderBy(o => o.Plate).ToList();

                result = SamePlateForNewNameMaintenance(result); //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            }
            else //plaka bazlı tekil tutarları listeler
            {
                result = result.GroupBy(g => g.VehicleId)
                    .Select(s => new RVehicleMaintenanceDto
                    {
                        //InvoiceDate = s.First().InvoiceDate,
                        Plate = s.First().Plate,
                        VehicleId = s.First().VehicleId,
                        Amounts = s.Where(w => w.VehicleId == s.First().VehicleId).ToList().Select(s => s.Amount).ToList()
                    }).OrderBy(o => o.Plate).ToList();

                result = SamePlateForNewNameMaintenance(result); //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
                int count = 0;
                var list = new List<Dictionary<string, string>>();
                foreach (var item in result)
                {
                    var subList = new Dictionary<string, string>();
                    foreach (var item2 in item.Amounts)
                    {
                        var name = "Amount" + count;
                        if (count == 0)
                        {
                            subList.Add("Plate", item.Plate);
                            subList.Add(name, item2.ToString().Replace(",", "."));
                        }
                        else
                            subList.Add(name, item2.ToString().Replace(",", "."));

                        count++;
                    }
                    list.Add(subList);
                    count = 0;
                }

                return list.ToList();
            }

            return result;
        }

        //Aynı plakalara -1,2,3 gibi ekler getirir
        public List<RVehicleMaintenanceDto> SamePlateForNewNameMaintenance(List<RVehicleMaintenanceDto> resultPlate)
        {
            var isSamePlate = _vehicleRepository.GetAll().Select(s => new Vehicle() { Plate = s.Plate }).ToList().GroupBy(g => g.Plate)
                .Select(s => new RVehicleFuelDto() { Count = s.Count(), Plate = s.First().Plate })
                .Where(w => w.Count > 1).OrderBy(o => o.VehicleId).ToList();

            foreach (var p in isSamePlate)
            {
                var samePlate = resultPlate.Where(w => w.Plate == p.Plate).ToList();
                for (int i = 0; i < samePlate.Count; i++)
                {
                    var temp = resultPlate.FirstOrDefault(f => f.VehicleId == samePlate[i].VehicleId);
                    if (temp != null)
                        resultPlate.FirstOrDefault(f => f.VehicleId == temp.VehicleId).Plate += "-" + temp.VehicleId;
                }
            }

            return resultPlate;
        }

        //Firma bazında rapor
        public async Task<List<RVehicleMaintenanceDto>> FirmVehicleMaintenanceTotalAmountAndCount(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.EndDate = model.EndDate.AddDays(1);
            var mainList = await GetMaintenanceListRange(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleMaintenanceDto>();
            foreach (var item in debitList)
                result.AddRange(mainList.Where(w => w.InvoiceDate >= item.StartDate && w.InvoiceDate < item.EndDate && w.VehicleId == item.VehicleId).ToList());

            result = result.ToList().GroupBy(g => g.SupplierName)
                .Select(s => new RVehicleMaintenanceDto
                {
                    PlateCount = result.Where(w => w.SupplierId == s.First().SupplierId).GroupBy(g => g.VehicleId).Count(),
                    SupplierName = s.First().SupplierName,
                    Amount = s.Sum(s => s.Amount),
                }).OrderByDescending(o => o.SupplierName).ToList();

            return result;
        }
        public async Task<List<RVehicleFuelDto>> GetDebitListRange(RFilterModelDto model)
        {
            var all = await
                Task.FromResult(from vd in _vehicleDebitRepository.GetAll()
                                join v in _vehicleRepository.GetAll() on vd.VehicleId equals v.Id

                                join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                                from vr in vrL.DefaultIfEmpty()
                                join rt in _lookUpListRepository.GetAll() on vr.RentTypeId equals rt.Id into rtL
                                from rt in rtL.DefaultIfEmpty()
                                join lr in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lr.Id into lrL
                                from lr in lrL.DefaultIfEmpty()

                                join user in _userRepository.GetAll() on vd.DebitUserId equals user.Id into userL
                                from user in userL.DefaultIfEmpty()
                                join u in _unitRepository.GetAll() on vd.UnitId equals u.Id into uL
                                from u in uL.DefaultIfEmpty()
                                join u2 in _unitRepository.GetAll() on u.ParentId equals u2.Id into u2L
                                from u2 in u2L.DefaultIfEmpty()
                                where vd.Status && (vd.State == (int)DebitState.Debit || vd.State == (int)DebitState.Pool || vd.State == (int)DebitState.InService)
                                && vd.StartDate <= model.EndDate
                                select new RVehicleFuelDto
                                {
                                    Status = v.Status,
                                    Plate = v.Plate,
                                    FixtureTypeId = v.FixtureTypeId,
                                    //UsageTypeId = vd.UsageTypeId,
                                    VehicleId = vd.VehicleId,
                                    UnitId = vd.UnitId,
                                    ParentUnitId = u2.Id,

                                    RentFirmName = lr.Name,
                                    RentType = vr.RentTypeId,
                                    RentTypeName = rt.Name,

                                    UserNameSurname = user.Name + " " + user.Surname + "-" + user.MobilePhone,
                                    StartDate = vd.StartDate <= model.StartDate ? model.StartDate : vd.StartDate,
                                    EndDate = (vd.EndDate == null || vd.EndDate > model.EndDate) ? model.EndDate : vd.EndDate.Value,
                                    UnitName = u2.Code != null ? u2.Code.Replace("\r", String.Empty).Replace("\n", String.Empty) : u2.Name,
                                    ProjectName = u.Code != null ? u.Code.Replace("\r", String.Empty).Replace("\n", String.Empty) : u.Name,
                                    State = vd.State,
                                });

            all = all.Where(w => w.EndDate > w.StartDate);

            if (model.ParentUnitId > 0)
                all = all.Where(w => w.ParentUnitId == model.ParentUnitId);

            if (model.UnitId > 0)
                all = all.Where(w => w.UnitId == model.UnitId);

            if (model.VehicleId > 0)
                all = all.Where(w => w.VehicleId == model.VehicleId);

            if (!string.IsNullOrEmpty(model.Plate))
                all = all.Where(w => w.Plate == model.Plate);

            var list = all.Where(w => w.EndDate <= model.EndDate).ToList();

            list.Where(w => w.State == (int)DebitState.Pool).ToList().ForEach(f => { f.UnitName = "Havuz"; f.ProjectName = "Havuz"; });
            return list.OrderBy(o => o.StartDate).ToList();
        }
        public async Task<List<RVehicleFuelDto>> GetDebitListRangeForExcel(RFilterModelDto model)
        {
            var all = await
                Task.FromResult(from vd in _vehicleDebitRepository.GetAll()
                                join v in _vehicleRepository.GetAll() on vd.VehicleId equals v.Id
                                join user in _userRepository.GetAll() on vd.DebitUserId equals user.Id into userL
                                from user in userL.DefaultIfEmpty()
                                join u in _unitRepository.GetAll() on vd.UnitId equals u.Id into uL
                                from u in uL.DefaultIfEmpty()
                                join u2 in _unitRepository.GetAll() on u.ParentId equals u2.Id into u2L
                                from u2 in u2L.DefaultIfEmpty()

                                join lt in _lookUpListRepository.GetAll() on v.VehicleTypeId equals lt.Id into ltL
                                from lt in ltL.DefaultIfEmpty()
                                join b in _vehicleBrandModelRepository.GetAll() on v.VehicleModelId equals b.Id
                                join bs in _vehicleBrandModelRepository.GetAll() on b.ParentId equals bs.Id

                                where vd.Status && (vd.State == (int)DebitState.Debit || vd.State == (int)DebitState.Pool || vd.State == (int)DebitState.InService)
                                && vd.StartDate <= model.EndDate
                                select new RVehicleFuelDto
                                {
                                    Status = v.Status,
                                    Plate = v.Plate,
                                    FixtureTypeId = v.FixtureTypeId,
                                    //UsageTypeId = vd.UsageTypeId,
                                    VehicleId = vd.VehicleId,
                                    UnitId = vd.UnitId,
                                    VehicleTypeName = lt.Name ?? "",
                                    VehicleModelName = bs.Name + "/" + b.Name,
                                    ParentUnitId = u2.Id,
                                    UserNameSurname = user.Name + " " + user.Surname + "-" + user.MobilePhone,
                                    StartDate = vd.StartDate <= model.StartDate ? model.StartDate : vd.StartDate,
                                    EndDate = (vd.EndDate == null || vd.EndDate > model.EndDate) ? model.EndDate : vd.EndDate.Value,
                                    UnitName = u2.Code != null ? u2.Code.Replace("\r", String.Empty).Replace("\n", String.Empty) : u2.Name,
                                    ProjectName = u.Code != null ? u.Code.Replace("\r", String.Empty).Replace("\n", String.Empty) : u.Name,
                                    State = vd.State,
                                });

            all = all.Where(w => w.EndDate > w.StartDate);

            if (model.ParentUnitId > 0)
                all = all.Where(w => w.ParentUnitId == model.ParentUnitId);

            if (model.UnitId > 0)
                all = all.Where(w => w.UnitId == model.UnitId);

            if (model.VehicleId > 0)
                all = all.Where(w => w.VehicleId == model.VehicleId);

            var list = all.Where(w => w.EndDate <= model.EndDate).ToList();
            return list;
        }
        //Not: Hgs yüklemeler dahil edilmiyor !!!!!!
        public async Task<List<RVehicleMaintenanceDto>> GetMaintenanceListRange(RFilterModelDto model)
        {
            var list = await Task.FromResult(from m in _maintenanceRepository.GetAll()
                                             join u in _userRepository.GetAll() on m.RequestUserId equals u.Id
                                             join l in _lookUpListRepository.GetAll() on m.SupplierId equals l.Id
                                             join c in _cityRepository.GetAll() on l.CityId equals c.Id into cL
                                             from c in cL.DefaultIfEmpty()
                                             join v in _vehicleRepository.GetAll() on m.VehicleId equals v.Id
                                             where m.Status == true && m.SupplierId != (int)Supplier.Ptt
                                             select new RVehicleMaintenanceDto()
                                             {
                                                 SupplierId = m.SupplierId,
                                                 VehicleId = v.Id,
                                                 Plate = v.Plate,
                                                 UserNameSurname = u.Name + " " + u.Surname + " " + u.MobilePhone,
                                                 SupplierName = l.Name + "-" + c.Name,
                                                 InvoiceDate = m.InvoiceDate,
                                                 InvoiceDate2 = m.InvoiceDate.ToString("MMM yyyy"),
                                                 Amount = m.TotalAmount,
                                                 UserFaultAmount = m.UserFaultAmount,
                                                 UserFaultDescription = m.UserFaultDescription
                                             });

            if (model.StartDate != DateTime.MinValue)
                list = list.Where(w => w.InvoiceDate >= model.StartDate && w.InvoiceDate <= model.EndDate);

            if (model.VehicleId > 0)
                list = list.Where(w => w.VehicleId == model.VehicleId);

            if (model.MinAmount > 0)
                list = list.Where(w => w.Amount >= model.MinAmount);

            if (model.MaxAmount > 0)
                list = list.Where(w => w.Amount <= model.MaxAmount);

            return list.ToList();
        }
        #endregion

        #region VehicleRentCost Page

        public async Task<RAllReportDto> HeaderInfoVehicleCost(RFilterModelDto model)
        {
            var result = new RAllReportDto();
            model = SetThisMonthStartEndDate(model);
            var costCalc = await GetDebitListRange(model);

            var resultPlateDetail = new List<RVehicleDto>();
            foreach (var item in costCalc)
            {
                try
                {
                    if (item.EndDate.Date == DateTime.Now.Date)
                        item.EndDate = item.EndDate.AddDays(1);

                    var allCost = await CalcVehicleMonthlySplitDayCost(item.VehicleId, item.StartDate, item.EndDate);

                    if (allCost == null)
                        continue;

                    item.ArventoAmount = allCost.Sum(s => s.ArventoSim);
                    item.ExtraAmount = allCost.Sum(s => s.ExtraAmount);
                    item.Amount = allCost.Sum(s => s.Amount);
                    item.TotalAmount = item.ArventoAmount + item.ExtraAmount + item.Amount;


                    #region Plaka detay

                    if (item.Amount > 0 || item.ExtraAmount > 0 || item.ArventoAmount > 0)
                        resultPlateDetail.Add(item);

                    #endregion
                }
                catch (Exception ex)
                {
                    var ss = 25;
                }
            }

            costCalc = costCalc.Where(w => w.Amount > 0).ToList();

            #region HeaderInfoVehicleCost
            var mostPlateAmount = costCalc.GroupBy(g => g.VehicleId).Select(s => new { Key = s.First().Plate, Value = s.Sum(s => s.Amount) + s.First().ExtraAmount }).OrderByDescending(x => x.Value).FirstOrDefault();
            var mostProjectAmount = costCalc.GroupBy(g => g.UnitId).Select(s => new { Key = s.First().UnitName, Value = s.Sum(s => s.Amount) + s.Sum(s => s.ExtraAmount) }).OrderByDescending(x => x.Value).FirstOrDefault();
            var plateCount = costCalc.GroupBy(d => d.VehicleId).Count();
            var totalAmount = costCalc.Count > 0 ? (costCalc.Sum(s => s.Amount) + costCalc.Sum(s => s.ExtraAmount)) : 0;

            result.HeaderInfoVehicleCost = new RDashboardDto
            {
                DateMonth = model.StartDate.ToString("MMM yyyy"),//Kullanılıyor
                TotalAmount = totalAmount,
                MostPlateAmount = mostPlateAmount != null ? new EFuelLogDto { Plate = mostPlateAmount.Key, TotalAmount = mostPlateAmount.Value } : new EFuelLogDto { Plate = "", TotalAmount = 0 },
                MostProjectAmount = mostProjectAmount != null ? new EFuelLogDto { UnitName = mostProjectAmount.Key, TotalAmount = mostProjectAmount.Value } : new EFuelLogDto { UnitName = "", TotalAmount = 0 },
                PercentageUnit = mostProjectAmount != null ? (mostProjectAmount.Value / totalAmount) : 0,
                PercentagePlate = mostProjectAmount != null ? (mostPlateAmount.Value / totalAmount) : 0,
                PlateCount = plateCount.ToString(),
                DebitVehicleCount = costCalc.GroupBy(g => g.VehicleId).Count()
            };
            #endregion

            #region VehicleCostTotalAmount
            //Plaka bazlı rapor
            var resultPlate = costCalc.GroupBy(g => g.VehicleId).Select(s => new RVehicleCostDto()
            {
                Amount = s.Sum(s => s.Amount),
                Plate = s.First().Plate
            }).OrderBy(o => o.Plate).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            resultPlate = SamePlateForNewNameCost(resultPlate);

            //Müdürlük bazında rapor
            var resultProject = new List<RVehicleCostDto>();
            if (model.IsAdmin)
            {
                resultProject = costCalc.GroupBy(g => g.UnitName).Select(s => new RVehicleCostDto()
                {
                    Amount = s.Sum(s => s.Amount) + s.Sum(s => s.ExtraAmount),
                    UnitName = s.First().UnitName
                }).ToList();
            }
            else//Proje bazında rapor
            {
                resultProject = costCalc.GroupBy(g => g.ProjectName).Select(s => new RVehicleCostDto()
                {
                    Amount = s.Sum(s => s.Amount) + s.Sum(s => s.ExtraAmount),
                    UnitName = s.First().ProjectName
                }).ToList();
            }

            //Firma bazında 
            var resultFirmName = costCalc.GroupBy(g => g.RentFirmName).Select(s => new RVehicleCostDto()
            {
                Amount = s.Sum(s => s.Amount),
                RentFirmName = s.First().RentFirmName
            }).OrderBy(o => o.Amount).ToList();

            //Kiralama türü (Kısa/Uzun)
            var resultRentTypeName = costCalc.GroupBy(g => g.RentTypeName).Select(s => new RVehicleCostDto()
            {
                Amount = s.Sum(s => s.Amount),
                RentTypeName = s.First().RentTypeName
            }).ToList();

            result.VehicleCostTotalAmount = new RVehicleCostDto()
            {
                ChartPlate = resultPlate,
                ChartProject = resultProject,
                ChartFirmName = resultFirmName,
                ChartRentTypeName = resultRentTypeName
            };
            #endregion

            #region VehicleDetailCostTotalAmount
            result.VehicleCostTotalAmountDetail = resultPlateDetail.GroupBy(g => g.VehicleId).Select(s => new RVehicleDto()
            {
                Amount = s.Sum(s => s.Amount),
                ExtraAmount = s.Sum(s => s.ExtraAmount),
                ArventoAmount = s.Sum(s => s.ArventoAmount),//arvento+sim kart
                Plate = s.First().Plate
            }).OrderBy(o => o.Plate).ToList();
            #endregion

            return result;
        }

        //public async Task<RDashboardDto> HeaderInfoVehicleCost(RFilterModelDto model)
        //{
        //    model = SetThisMonthStartEndDate(model);
        //    var debitList = await GetDebitListRange(model);
        //    var resultt = await GetVehicleCostWithDebitList(model);
        //    int daysInMonth = defaultMonthDay;// DateTime.DaysInMonth(model.StartDate.Year, model.StartDate.Month);
        //    var costCalc = (from r in resultt
        //                    select new RVehicleCostDto()
        //                    {
        //                        CostType = r.CostType,
        //                        CostTypeName = r.CostTypeName,
        //                        VehicleId = r.VehicleId,
        //                        Plate = r.Plate,
        //                        Amount = r.Amount,
        //                        ExtraAmount = r.ExtraAmount,
        //                        UnitName = r.UnitName,
        //                        UnitId = r.UnitId,
        //                        DayCount = (int)(r.EndDate - r.StartDate).TotalDays,
        //                        DatesRangeCost = (decimal)(r.Amount / daysInMonth) * (int)(r.EndDate - r.StartDate).TotalDays,
        //                    }).ToList();


        //    var mostPlateAmount = costCalc.GroupBy(g => g.VehicleId).Select(s => new { Key = s.First().Plate, Value = s.Sum(s => s.DatesRangeCost) + s.First().ExtraAmount }).OrderByDescending(x => x.Value).FirstOrDefault();
        //    var mostProjectAmount = costCalc.GroupBy(g => g.UnitId).Select(s => new { Key = s.First().UnitName, Value = s.Sum(s => s.DatesRangeCost) + s.Sum(s => s.ExtraAmount) }).OrderByDescending(x => x.Value).FirstOrDefault();
        //    var plateCount = costCalc.GroupBy(d => d.VehicleId).Count();
        //    var totalAmount = costCalc.Count > 0
        //        ? (costCalc.Sum(s => s.DatesRangeCost) + costCalc.Sum(s => s.ExtraAmount))
        //        : 0;
        //    var result2 = new RDashboardDto
        //    {
        //        DateMonth = model.StartDate.ToString("MMM yyyy"),//Kullanılıyor
        //        TotalAmount = totalAmount,
        //        MostPlateAmount = mostPlateAmount != null ? new EFuelLogDto { Plate = mostPlateAmount.Key, TotalAmount = mostPlateAmount.Value } : new EFuelLogDto { Plate = "", TotalAmount = 0 },
        //        MostProjectAmount = mostProjectAmount != null ? new EFuelLogDto { UnitName = mostProjectAmount.Key, TotalAmount = mostProjectAmount.Value } : new EFuelLogDto { UnitName = "", TotalAmount = 0 },
        //        PercentageUnit = mostProjectAmount != null ? (mostProjectAmount.Value / totalAmount) : 0,
        //        PercentagePlate = mostProjectAmount != null ? (mostPlateAmount.Value / totalAmount) : 0,
        //        PlateCount = plateCount.ToString(),
        //        DebitVehicleCount = debitList.GroupBy(g => g.VehicleId).Count()
        //    };
        //    return result2;
        //}
        //Zimmetli tarihlerine göre kira fiyatları çekerMonthlyHgsTotalAmount
        public async Task<RVehicleCostDto> VehicleCostTotalAmount(RFilterModelDto model)
        {
            model = SetThisMonthStartEndDate(model);
            var result = await GetVehicleCostWithDebitList(model);
            int daysInMonth = defaultMonthDay;// DateTime.DaysInMonth(model.StartDate.Year, model.StartDate.Month);
            var costCalc = (from r in result
                            select new RVehicleCostDto()
                            {
                                RentTypeName = r.RentTypeName,
                                CostTypeName = r.CostTypeName,
                                RentFirmName = r.RentFirmName,
                                VehicleId = r.VehicleId,
                                Plate = r.Plate,
                                Amount = r.Amount,
                                ExtraAmount = r.ExtraAmount,
                                UnitName = r.UnitName,
                                ProjectName = r.ProjectName,
                                UnitId = r.UnitId,
                                DayCount = (int)(r.EndDate - r.StartDate).TotalDays,
                                DatesRangeCost = (decimal)(r.Amount / daysInMonth) * (int)(r.EndDate - r.StartDate).TotalDays,
                            }).ToList();

            //Plaka bazlı rapor
            var resultPlate = costCalc.GroupBy(g => g.VehicleId).Select(s => new RVehicleCostDto()
            {
                Amount = s.Sum(s => s.DatesRangeCost) + s.Sum(s => s.ExtraAmount),
                VehicleId = s.First().VehicleId,
                Plate = s.First().Plate
            }).OrderBy(o => o.Plate).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            resultPlate = SamePlateForNewNameCost(resultPlate);

            //Müdürlük bazında rapor
            var resultProject = new List<RVehicleCostDto>();
            if (model.IsAdmin)
            {
                resultProject = costCalc.GroupBy(g => g.UnitName).Select(s => new RVehicleCostDto()
                {
                    Amount = s.Sum(s => s.DatesRangeCost) + s.Sum(s => s.ExtraAmount),
                    UnitName = s.First().UnitName
                }).ToList();
            }
            else//Proje bazında rapor
            {
                resultProject = costCalc.GroupBy(g => g.ProjectName).Select(s => new RVehicleCostDto()
                {
                    Amount = s.Sum(s => s.DatesRangeCost) + s.Sum(s => s.ExtraAmount),
                    UnitName = s.First().ProjectName
                }).ToList();
            }

            //Firma bazında 
            var resultFirmName = costCalc.GroupBy(g => g.RentFirmName).Select(s => new RVehicleCostDto()
            {
                Amount = s.Sum(s => s.DatesRangeCost),
                RentFirmName = s.First().RentFirmName
            }).OrderBy(o => o.Amount).ToList();

            //Kiralama türü (Kısa/Uzun)
            var resultRentTypeName = costCalc.GroupBy(g => g.RentTypeName).Select(s => new RVehicleCostDto()
            {
                Amount = s.Sum(s => s.DatesRangeCost),
                RentTypeName = s.First().RentTypeName
            }).ToList();

            var chart = new RVehicleCostDto()
            {
                ChartPlate = resultPlate,
                ChartProject = resultProject,
                ChartFirmName = resultFirmName,
                ChartRentTypeName = resultRentTypeName
            };

            return chart;
        }
        //aynı plakalara -1,2,3 gibi sayı ekler
        public List<RVehicleCostDto> SamePlateForNewNameCost(List<RVehicleCostDto> resultPlate)
        {
            var isSamePlate = _vehicleRepository.GetAll().Select(s => new Vehicle() { Plate = s.Plate }).ToList().GroupBy(g => g.Plate)
                .Select(s => new RVehicleFuelDto() { Count = s.Count(), Plate = s.First().Plate })
                .Where(w => w.Count > 1).OrderBy(o => o.VehicleId).ToList();

            foreach (var p in isSamePlate)
            {
                var samePlate = resultPlate.Where(w => w.Plate == p.Plate).ToList();
                for (int i = 0; i < samePlate.Count; i++)
                {
                    var temp = resultPlate.FirstOrDefault(f => f.VehicleId == samePlate[i].VehicleId);
                    if (temp != null)
                        resultPlate.FirstOrDefault(f => f.VehicleId == temp.VehicleId).Plate += "-" + temp.VehicleId;
                }
            }

            return resultPlate;
        }
        public async Task<List<RVehicleCostDto>> MonthlyVehicleCost(RFilterModelDto model)
        {
            var dateSplitMonth = DateDiff.TwoDateRangeMonthSplitAddDays_1(model.StartDate, model.EndDate);
            var vehicleCostMontlyList = new List<RVehicleCostDto>();
            foreach (var item in dateSplitMonth)//ay ay araç kiralık ücretleri
            {
                var result = await GetVehicleCostWithDebitList(new RFilterModelDto()
                {
                    StartDate = item.StartDate,
                    EndDate = item.EndDate
                });
                int daysInMonth = defaultMonthDay;// DateTime.DaysInMonth(item.StartDate.Year, item.StartDate.Month);
                var costCalc = (from r in result
                                select new RVehicleCostDto()
                                {
                                    DatesRangeCost = (decimal)(r.Amount / daysInMonth) * (int)(r.EndDate - r.StartDate).TotalDays,
                                    ExtraAmount = r.ExtraAmount
                                }).ToList();

                var amount = costCalc.Sum(s => s.DatesRangeCost);
                var extra = costCalc.Sum(s => s.ExtraAmount);
                var total = costCalc.Sum(s => s.DatesRangeCost + s.ExtraAmount);

                vehicleCostMontlyList.Add(new RVehicleCostDto()
                {
                    CostDate = item.StartDate,
                    DateMonth = item.StartDate.ToString("MMM yyyy"),
                    DatesRangeCost = costCalc.Sum(s => s.DatesRangeCost + s.ExtraAmount)
                });
            }

            return vehicleCostMontlyList.Where(w => w.DatesRangeCost > 0).ToList();
        }

        //Zimmetten bağımsız kira fiyatları listeler (plaka bazlı kira)
        //public async Task<RVehicleCostDto> VehicleCostTotalAmountNotDebit(RFilterModelDto model)
        //{
        //    model = SetThisMonthStartEndDate(model);
        //    var contractList = await GetContractList(model);
        //    //int daysInMonth = DateTime.DaysInMonth(model.StartDate.Year, model.StartDate.Month);

        //    var costCalc = new List<RVehicleCostDto>();
        //    foreach (var subCost in contractList)
        //    {
        //        var extraAmounts = _vehicleAmountRepository.Where(w =>
        //            w.VehicleId == subCost.VehicleId && w.TypeId == (int)VehicleAmountType.ExtraTutar &&
        //            (w.StartDate >= model.StartDate && w.StartDate <= model.EndDate)).Sum(s => s.Amount);
        //        var temp = (from va in subCost.SubContractlist
        //                    select new RVehicleCostDto()
        //                    {
        //                        StartDate = va.StartDate <= model.StartDate ? model.StartDate : va.StartDate,
        //                        EndDate = (va.EndDate == null || va.EndDate > model.EndDate) ? model.EndDate : va.EndDate.Value,
        //                        Amount = va.Amount,
        //                        VehicleId = va.VehicleId,
        //                        Plate = va.Plate,
        //                        CostTypeName = va.CostTypeName,
        //                        CostType = va.CostType,
        //                        RentFirmName = subCost.RentFirmName,
        //                        RentType = subCost.RentType,
        //                        RentTypeName = subCost.RentTypeName,
        //                        ExtraAmount = extraAmounts
        //                    }).Where(w => w.EndDate > w.StartDate).ToList();

        //        foreach (var sub in temp)
        //        {
        //            var dateSpan = DateDiff.DateTimeSpan.CompareDates(sub.EndDate, sub.StartDate);
        //            var years = dateSpan.Years;
        //            var months = dateSpan.Months;
        //            var days = dateSpan.Days;
        //            int daysInMonth = DateTime.DaysInMonth(sub.EndDate.Year, sub.EndDate.Month);
        //            sub.Amount = (years * 12 + months) * sub.Amount + ((sub.Amount / daysInMonth) * days);
        //        }
        //        costCalc.AddRange(temp);
        //    }

        //    //Plaka bazlı rapor
        //    var resultPlate = costCalc.GroupBy(g => g.VehicleId).Select(s => new RVehicleCostDto()
        //    {
        //        Amount = s.Sum(s => s.Amount) + s.First().ExtraAmount,
        //        Plate = s.First().Plate
        //    }).ToList();

        //    //Firma bazında 
        //    var resultFirmName = costCalc.GroupBy(g => g.RentFirmName).Select(s => new RVehicleCostDto()
        //    {
        //        Amount = s.Sum(s => s.Amount),
        //        RentFirmName = s.First().RentFirmName
        //    }).OrderBy(o => o.Amount).ToList();

        //    //Kiralama türü (Kısa/Uzun)
        //    var resultRentTypeName = costCalc.GroupBy(g => g.RentTypeName).Select(s => new RVehicleCostDto()
        //    {
        //        Amount = s.Sum(s => s.Amount),
        //        RentTypeName = s.First().RentTypeName
        //    }).ToList();

        //    var chart = new RVehicleCostDto()
        //    {
        //        ChartPlate = resultPlate,
        //        ChartFirmName = resultFirmName,
        //        ChartRentTypeName = resultRentTypeName
        //    };
        //    return chart;
        //}

        int defaultMonthDay = 30;
        public async Task<object> VehicleDetailCostTotalAmount(RFilterModelDto model)
        {
            model = SetThisMonthStartEndDate(model);
            var result = await GetVehicleCostWithDebitList(model);
            int daysInMonth = defaultMonthDay;// DateTime.DaysInMonth(model.StartDate.Year, model.StartDate.Month);
            var costCalc = (from r in result
                            select new RVehicleCostDto()
                            {
                                CostType = r.CostType,
                                CostTypeName = r.CostTypeName,
                                VehicleId = r.VehicleId,
                                Plate = r.Plate,
                                Amount = r.Amount,
                                ExtraAmount = r.ExtraAmount,
                                UnitName = r.UnitName,
                                UnitId = r.UnitId,
                                DayCount = (int)(r.EndDate - r.StartDate).TotalDays,
                                DatesRangeCost = (decimal)(r.Amount / daysInMonth) * (int)(r.EndDate - r.StartDate).TotalDays,
                            }).ToList();

            //Plaka sayısı
            var resultPlate = costCalc.GroupBy(g => g.VehicleId).Select(s => new RVehicleCostDto()
            {
                ExtraAmount = s.Sum(s => s.ExtraAmount),
                VehicleId = s.First().VehicleId,
                Plate = s.First().Plate,
            }).OrderBy(o => o.Plate).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            resultPlate = SamePlateForNewNameCost(resultPlate);

            //Plaka bazlı detail
            var resultPlateDetail = new List<Dictionary<string, string>>();
            foreach (var item in resultPlate)
            {
                var temp = costCalc.Where(w => w.VehicleId == item.VehicleId)
                    .Select(s => new RVehicleCostDto()
                    {
                        CostType = s.CostType,
                        CostTypeName = s.CostTypeName,
                        Plate = s.Plate,
                        Amount = s.DatesRangeCost
                    });

                var costTypeGroup = temp.GroupBy(g => g.CostType).Select(s => new RVehicleCostDto
                {
                    Plate = s.First().Plate,
                    CostTypeName = s.First().CostTypeName,
                    CostType = s.First().CostType,
                    Amount = /*s.Select(s => s.Amount).ToList()//*/s.GroupBy(g => g.VehicleId).Sum(s => s.Sum(s => s.Amount))
                });

                var subList = new Dictionary<string, string>();
                subList.Add("Plate", item.Plate);

                //Kira bedeli
                var rentCost = costTypeGroup.Where(w => w.CostType == (int)VehicleAmountType.KiraBedeli).Sum(s => s.Amount);
                subList.Add("rentcost", rentCost.ToString().Replace(",", "."));

                //Extra giderler
                if (item.ExtraAmount > 0)
                    subList.Add("extracost", item.ExtraAmount.ToString().Replace(",", "."));

                //Diğer giderler
                var arventoSim = costTypeGroup.Where(w => (w.CostType == (int)VehicleAmountType.ArventoMaliyet || w.CostType == (int)VehicleAmountType.SimKartMaliyet)).Sum(s => s.Amount);
                if (arventoSim > 0)
                    subList.Add("othercost", arventoSim.ToString().Replace(",", "."));

                resultPlateDetail.Add(subList);
                subList = null;
            }
            return resultPlateDetail;
        }

        //Verilen aracın tarih aralığında günlük ücretini hesaplar
        public async Task<List<RVehicleDayCostDto>> CalcVehicleMonthlySplitDayCost(int vehicleId, DateTime start, DateTime end)
        {
            var dates = new List<RVehicleDayCostDto>();
            try
            {
                var allAmount = _vehicleAmountRepository.Where(w => w.Status && w.VehicleId == vehicleId && w.EndDate > start).OrderByDescending(o => o.EndDate).ToList();
                if (allAmount.Count == 0)
                    return null;

                var totalMonth = DateDiff.TwoDateRangeMonthSplitForCost(start, end);

                foreach (var item in totalMonth)
                {
                    bool dayHalf = false;

                    var lastDay = new DateTime(item.StartDate.Year, item.StartDate.Month, 1).AddMonths(1).AddDays(-1).Day;
                    if (lastDay == 28 || lastDay == 29 || lastDay == 31)
                        dayHalf = true;

                    var dt = item.StartDate.Date;
                    var totalDay = item.StartDate.Day;
                    var subDay = item.EndDate.Subtract(item.StartDate).Days;
                    var last = totalDay + subDay;

                    if (item.EndDate.AddDays(-1).Day > 27)
                        last = 31;

                    for (int i = totalDay; i < last; i++)
                    {
                        decimal? vehicleCost = 0, arventoCost = 0, extraCost = 0, simCost = 0;

                        if (dayHalf && i > 28)
                        {
                            if (dates.Count == 0)
                                continue;

                            vehicleCost = dates[dates.Count - 1].Amount;
                            arventoCost = dates[dates.Count - 1].ArventoSim;
                            dates.Add(new RVehicleDayCostDto()
                            {
                                Amount = (decimal)vehicleCost,
                                ArventoSim = (decimal)arventoCost,
                                DayCount = i
                            });
                        }
                        else
                        {
                            vehicleCost = allAmount.FirstOrDefault(w => w.VehicleId == vehicleId && w.TypeId == (int)VehicleAmountType.KiraBedeli && w.StartDate <= dt && dt < w.EndDate)?.Amount;
                            arventoCost = allAmount.FirstOrDefault(w => w.VehicleId == vehicleId && w.TypeId == (int)VehicleAmountType.ArventoMaliyet && w.StartDate <= dt && dt < w.EndDate)?.Amount;
                            simCost = allAmount.FirstOrDefault(w => w.VehicleId == vehicleId && w.TypeId == (int)VehicleAmountType.SimKartMaliyet && w.StartDate <= dt && dt < w.EndDate)?.Amount;
                            extraCost = allAmount.Where(w => w.VehicleId == vehicleId && w.TypeId == (int)VehicleAmountType.ExtraTutar && w.StartDate == dt).Sum(s => s.Amount);

                            decimal vehicleDayCost = 0, arventoDayCost = 0;


                            var monthDay = 30;// DateTime.DaysInMonth(dt.Year, dt.Month);
                            if (vehicleCost != null)
                                vehicleDayCost = (decimal)(vehicleCost / monthDay);

                            if (arventoCost > 0 || simCost > 0)
                                arventoDayCost = (decimal)((arventoCost + simCost) / monthDay);


                            dates.Add(new RVehicleDayCostDto()
                            {
                                Amount = vehicleDayCost,
                                ExtraAmount = (decimal)extraCost,
                                ArventoSim = arventoDayCost,
                                DayCount = i
                            });
                        }
                        dt = dt.AddDays(1);
                    }
                }

            }
            catch (Exception ex)
            {
                var ss = 25;
            }
            return dates;
        }

        public async Task<List<RVehicleCostDto>> GetVehicleCostWithDebitList(RFilterModelDto model)
        {
            //var contractList = await GetContractList(model);
            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleCostDto>();
            foreach (var item in debitList)
            {
                //if (contractList.Count > 0)
                //{
                //Kira bedeli tarihleri arasındaki extra tutarlar ekleniyor
                var extraAmounts = _vehicleAmountRepository.Where(w =>
                    w.VehicleId == item.VehicleId && w.TypeId == (int)VehicleAmountType.ExtraTutar &&
                    (w.StartDate >= item.StartDate && w.StartDate < item.EndDate)).ToList();

                var subCostList = (from va in _vehicleAmountRepository.GetAll()
                                   join v in _vehicleRepository.GetAll() on va.VehicleId equals v.Id

                                   join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId
                                   join rt in _lookUpListRepository.GetAll() on vr.RentTypeId equals rt.Id
                                   join lr in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lr.Id

                                   join l in _lookUpListRepository.GetAll() on va.TypeId equals l.Id
                                   where v.Id == item.VehicleId && va.StartDate <= item.StartDate && va.EndDate < item.EndDate && va.TypeId != (int)VehicleAmountType.ExtraTutar
                                   select new RVehicleCostDto()
                                   {
                                       StartDate = va.StartDate,
                                       EndDate = va.EndDate.Value,
                                       Amount = va.Amount,
                                       VehicleId = va.VehicleId,
                                       ExtraAmount = va.ExtraAmount == null ? 0 : va.ExtraAmount.Value,
                                       Plate = v.Plate,
                                       UnitName = item.UnitName,
                                       ProjectName = item.ProjectName,
                                       UnitId = item.UnitId,
                                       CostTypeName = l.Code,
                                       CostType = l.Id,
                                       RentFirmName = lr.Name,
                                       RentType = vr.RentTypeId,
                                       RentTypeName = rt.Name
                                   }).ToList();

                result.AddRange(subCostList);

                if (result.Any())
                    result[0].ExtraAmount = extraAmounts.Sum(s => s.Amount);
                //}
            }

            return result;
        }
        public async Task<List<RVehicleCostDto>> GetContractList(RFilterModelDto filterModel)
        {
            var list = await Task.FromResult(from va in _vehicleAmountRepository.GetAll()
                                             join v in _vehicleRepository.GetAll() on va.VehicleId equals v.Id
                                             join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId
                                             join rt in _lookUpListRepository.GetAll() on vr.RentTypeId equals rt.Id
                                             join lr in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lr.Id
                                             join l in _lookUpListRepository.GetAll() on va.TypeId equals l.Id
                                             where v.FixtureTypeId == (int)FixtureType.ForRent && va.TypeId != (int)VehicleAmountType.ExtraTutar
                                             select new RVehicleCostDto()
                                             {
                                                 Status = v.Status,
                                                 StartDate = va.StartDate /*<= filterModel.StartDate ? filterModel.StartDate : va.StartDate*/,
                                                 EndDate = va.EndDate.Value,
                                                 VehicleId = va.VehicleId,
                                                 Plate = v.Plate,
                                                 Amount = va.Amount,
                                                 VehicleContractId = va.VehicleContractId,
                                                 CostTypeName = l.Code, //Türler 
                                                 CostType = l.Id,
                                                 RentType = vr.RentTypeId,
                                                 RentTypeName = rt.Name, //Kiralama tipi (Kısa/uzun dönem)
                                                 RentFirmName = lr.Name //Kiralama firması
                                             });

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.RentTypeId > 0)
                list = list.Where(w => w.RentType == filterModel.RentTypeId);

            //list = list.Where(w => w.EndDate > w.StartDate);
            list = list.Where(w => w.EndDate <= filterModel.StartDate);

            var plates = list.ToList().GroupBy(g => g.VehicleContractId).Select(s => new RVehicleCostDto()
            {
                SubContractlist = list.Where(w => w.VehicleId == s.First().VehicleId && w.VehicleContractId == s.First().VehicleContractId)
                    .Select(sb => new RVehicleSubCostDto()
                    {
                        VehicleId = s.First().VehicleId,
                        Plate = s.First().Plate,
                        StartDate = sb.StartDate,
                        EndDate = sb.EndDate,
                        Amount = sb.Amount,
                        CostTypeName = sb.CostTypeName,
                        CostType = sb.CostType
                    }).ToList(),
                VehicleContractId = s.First().VehicleContractId,
                Status = s.First().Status,
                Plate = s.First().Plate,
                VehicleId = s.First().VehicleId,
                RentFirmName = s.First().RentFirmName,
                RentTypeName = s.First().RentTypeName
            }).ToList();

            //Araç sözleşme tarihinden erken teslim edildiyse son zimmet tarihi baz alınıyor
            var lastContract = plates.OrderByDescending(o => o.VehicleContractId).FirstOrDefault();
            //for (int i = 0; i < plates.Count; i++)
            //{
            if (!lastContract.Status)
            {
                var endDate = SetLastDebitEndDate(lastContract.VehicleId, lastContract.EndDate);
                lastContract.SubContractlist.ForEach(f => f.EndDate = endDate);
            }
            //}

            return plates;
        }
        public DateTime SetLastDebitEndDate(int vehicleId, DateTime endDate)
        {
            var debit = _vehicleDebitRepository
                .Where(w => w.VehicleId == vehicleId && w.State != (int)DebitState.Deleted)
                .OrderByDescending(o => o.Id).FirstOrDefault();
            if (debit != null)
                endDate = debit.EndDate.Value;

            return endDate;
        }
        public RFilterModelDto SetThisMonthStartEndDate(RFilterModelDto model)
        {
            if (model.StartDate == DateTime.MinValue) //first load page
            {
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                model.StartDate = firstDayOfMonth;
                model.EndDate = firstDayOfMonth.AddMonths(1);
            }
            return model;
        }
        #endregion

        #region Just Hgs
        public async Task<RVehicleCostDto> GetHgsReport(RFilterModelDto model)
        {
            var list = await HgsWithDebitlist(model);
            if (list.Count > 0)
            {
                var result = list.GroupBy(g => g.VehicleId).Select(s => new RVehicleCostDto()
                {
                    Plate = s.First().Plate,
                    VehicleId = s.First().VehicleId,
                    Amount = s.Sum(s => s.Amount),
                }).OrderBy(o => o.Plate).ToList();

                result = SamePlateForNewNameCost(result); //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Aynı plakada araç kiralanabilir veya eklenebilir)

                var plateCount = result.Count;
                var totalAmount = result.Sum(s => s.Amount);
                return new RVehicleCostDto()
                {
                    ChartPlate = result,
                    PlateCount = plateCount,
                    Amount = totalAmount
                };
            }
            else
                return null;
        }
        public async Task<List<RVehicleCostDto>> MonthlyHgsTotalAmount(RFilterModelDto model)
        {
            var result = await HgsWithDebitlist(model);
            var groupByResult = result.GroupBy(g => g.InvoiceDate2).Select(s => new RVehicleCostDto()
            {
                CostDate = s.First().InvoiceDate,
                DateMonth = s.First().InvoiceDate2,
                Amount = s.Sum(s => s.Amount),
            }).ToList();
            return groupByResult;
        }
        public async Task<List<RVehicleMaintenanceDto>> HgsWithDebitlist(RFilterModelDto model, bool isMainTypeAllWithJoin = false)
        {
            var hgsList = await Task.FromResult(from m in _maintenanceRepository.GetAll()
                                                join u in _userRepository.GetAll() on m.RequestUserId equals u.Id
                                                join l in _lookUpListRepository.GetAll() on m.SupplierId equals l.Id
                                                join c in _cityRepository.GetAll() on l.CityId equals c.Id into cL
                                                from c in cL.DefaultIfEmpty()
                                                join v in _vehicleRepository.GetAll() on m.VehicleId equals v.Id
                                                where m.Status == true && m.SupplierId == (int)Supplier.Ptt
                                                select new RVehicleMaintenanceDto()
                                                {
                                                    Id = m.Id,
                                                    VehicleId = v.Id,
                                                    Plate = v.Plate,
                                                    UserNameSurname = u.Name + " " + u.Surname + " " + u.MobilePhone,
                                                    InvoiceDate = m.InvoiceDate,
                                                    InvoiceDate2 = m.InvoiceDate.ToString("MMM yyyy"),
                                                    Amount = m.TotalAmount
                                                });

            if (model.StartDate != DateTime.MinValue)
                hgsList = hgsList.Where(w => w.InvoiceDate >= model.StartDate && w.InvoiceDate <= model.EndDate);

            var debitList = await GetDebitListRange(model);

            var result = new List<RVehicleMaintenanceDto>();
            foreach (var item in debitList)//Zimmetli bilgileri ekleniyor
            {
                var unitName = item.ProjectName;
                if (item.State == (int)DebitState.Pool)
                    unitName = "Havuz";
                else if (item.State == (int)DebitState.InService)
                    unitName = "Servis";

                var temp = (from m in hgsList
                            where m.InvoiceDate >= item.StartDate && m.InvoiceDate < item.EndDate && m.VehicleId == item.VehicleId
                            select new RVehicleMaintenanceDto()
                            {
                                Id = m.Id,
                                VehicleId = m.VehicleId,
                                Plate = m.Plate,
                                UserNameSurname = m.UserNameSurname,
                                InvoiceDate = m.InvoiceDate,
                                InvoiceDate2 = m.InvoiceDate2,
                                Amount = m.Amount,
                                UnitName = unitName,
                                DebitNameSurname = item.UserNameSurname
                            }).ToList();
                result.AddRange(temp);
            }

            if (model.TypeId > 0)
            {
                foreach (var item2 in result)
                    item2.MaintenanceTypeList = _maintenanceTypeRepository
                        .Where(w => w.MaintenanceId == item2.Id && w.TypeId == model.TypeId).ToList();

                result = result.Where(w => w.MaintenanceTypeList.Count > 0).ToList();
            }

            //İlgili satıra bağlı tüm tipler string formatına çevirir
            if (isMainTypeAllWithJoin)
            {
                foreach (var item2 in result)
                {
                    //Bakım/Onarım tipi
                    var temp = await Task.FromResult((from m in _maintenanceTypeRepository.GetAll()
                                                      join l in _lookUpListRepository.GetAll() on m.TypeId equals l.Id
                                                      where m.MaintenanceId == item2.Id
                                                      select new RVehicleMaintenanceDto()
                                                      {
                                                          Name = l.Name
                                                      }).ToList());
                    item2.AllMaintenanceTypeWithJoin = String.Join(",", temp.Select(s => s.Name).ToList());
                }
            }

            return result;
        }
        #endregion

        #region Trip
        #region User
        public async Task<RDashboardDto> HeaderInfoTripUser(RFilterModelDto model)
        {
            model = SetStartEndDate(model);

            var allTrip = await Task.FromResult((from t in _tripRepository.GetAll()
                                                 where t.Status && t.DriverId == model.UserId && t.State == (int)TripState.EndTrip &&
                                                 (t.StartDate >= model.StartDate && t.StartDate <= model.EndDate)
                                                 select t));

            if (model.VehicleId > 0)
                allTrip = allTrip.Where(w => w.VehicleId == model.VehicleId);

            var result2 = new RDashboardDto
            {
                TotalCount = allTrip.Count(),
                TotalKm = allTrip.Sum(s => s.EndKm - s.StartKm)
            };
            return result2;
        }

        public async Task<List<RVehicleFuelDto>> TripTotalCountUser(RFilterModelDto model)
        {
            model = SetStartEndDate(model);

            var allTrip = await Task.FromResult((from t in _tripRepository.GetAll()
                                                 where t.Status && t.DriverId == model.UserId && t.State == (int)TripState.EndTrip &&
                                                 (t.StartDate >= model.StartDate && t.StartDate <= model.EndDate)
                                                 select t));

            if (model.VehicleId > 0)
                allTrip = allTrip.Where(w => w.VehicleId == model.VehicleId);

            var groupList = allTrip.ToList().GroupBy(g => g.StartDate.ToString("MMM yyyy"))
                .Select(g => new RVehicleFuelDto
                {
                    FuelDate2 = g.First().StartDate.ToString("MMM yyyy"),
                    Count = g.Count()
                }).OrderBy(o => o.FuelDate).ToList();

            return groupList;
        }

        public async Task<List<RVehicleFuelDto>> TripTotalKmUser(RFilterModelDto model)
        {
            model = SetStartEndDate(model);

            var allTrip = await Task.FromResult((from t in _tripRepository.GetAll()
                                                 where t.Status && t.DriverId == model.UserId && t.State == (int)TripState.EndTrip &&
                                                 (t.StartDate >= model.StartDate && t.StartDate <= model.EndDate)
                                                 select t));

            if (model.VehicleId > 0)
                allTrip = allTrip.Where(w => w.VehicleId == model.VehicleId);

            var groupList = allTrip.ToList().GroupBy(g => g.StartDate.ToString("MMM yyyy"))
                .Select(g => new RVehicleFuelDto
                {
                    FuelDate2 = g.First().StartDate.ToString("MMM yyyy"),
                    Count = Convert.ToInt32(g.Sum(s => s.EndKm - s.StartKm))
                }).OrderBy(o => o.FuelDate).ToList();

            return groupList;
        }
        #endregion

        #region Manager
        public async Task<RDashboardDto> HeaderInfoTrip(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            var tripLists = await GetTripList(model);

            var allowedList = tripLists.Where(w => w.EndDate != null).ToList();//göreve başlayıp ret edilenleri alma
            var mostProjectName = allowedList.GroupBy(d => d.UnitName)
                .Select(g => new
                {
                    Key = g.First().UnitName,
                    Value = g.Sum(s => s.EndKm - s.StartKm)
                }).OrderByDescending(o => o.Value).Take(1).FirstOrDefault();

            var result2 = new RDashboardDto
            {
                DateMonth = model.StartDate.ToString("MMM yyyy"),//Kullanılıyor
                TotalCount = allowedList.Count(),//toplam görev sayısı
                PlateCount = allowedList.GroupBy(g => g.VehicleId).Count().ToString(),//Tekil plaka görev sayısı
                TotalKm = allowedList.Sum(s => s.EndKm - s.StartKm),//Toplam km
                MostProjectAmount = mostProjectName != null ? new EFuelLogDto { UnitName = mostProjectName.Key, Km = mostProjectName.Value } : new EFuelLogDto { UnitName = "", Km = 0 }
            };
            return result2;
        }

        public async Task<List<RVehicleFuelDto>> PlateTrip(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            var tripLists = await GetTripList(model);

            var allowedList = tripLists.Where(w => w.EndDate != null).ToList();//göreve başlayıp ret edilenleri alma
            var result = allowedList.GroupBy(g => g.VehicleId)
               .Select(s => new RVehicleFuelDto
               {
                   VehicleId = s.First().VehicleId,
                   Plate = s.First().Plate,
                   Count = s.Count(),
                   TotalKm = (decimal)s.Sum(s => s.EndKm - s.StartKm)
               }).OrderBy(o => o.Plate).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            result = SamePlateForNewNameFuel(result);
            return result;
        }
        public async Task<List<RVehicleFuelDto>> PlateTripKm(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            var tripLists = await GetTripList(model);

            var allowedList = tripLists.Where(w => w.EndDate != null).ToList();//göreve başlayıp ret edilenleri alma
            var result = allowedList.GroupBy(g => g.DriverId)
               .Select(s => new RVehicleFuelDto
               {
                   UserNameSurname = s.First().NameSurname,
                   Count = s.Count(),
                   TotalKm = (decimal)s.Sum(s => s.EndKm - s.StartKm)
               }).OrderBy(o => o.Plate).ToList();

            //Aynı plakalara yeni isimlendirme yapılıyor. (Not: Ayın plakada araç kiralanabilir veya eklenebilir)
            result = SamePlateForNewNameFuel(result);
            return result;
        }

        public async Task<List<ETripDto>> MonthlyTrip(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            var tripLists = await GetTripList(model);

            var allowedList = tripLists.Where(w => w.EndDate != null).ToList();//göreve başlayıp ret edilenleri alma
            var result = allowedList.GroupBy(g => g.StartDate.ToString("MMM yyyy"))
                .Select(g => new ETripDto
                {
                    StartDate = g.First().StartDate,
                    TransactionDate = g.First().StartDate.ToString("MMM yyyy"),
                    DebitPlateCount = g.Count(),
                    TotalKm = (decimal)g.Sum(s => s.EndKm - s.StartKm)
                }).OrderBy(o => o.StartDate).ToList();
            //AddMemoryCache(cacheKey, result);
            return result;
        }

        public async Task<List<RVehicleFuelDto>> ProjectTripTotal(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            //model.State = (int)TripState.EndTrip;
            var tripLists = await GetTripList(model);

            var allowedList = tripLists.Where(w => w.EndDate != null).ToList();//göreve başlayıp ret edilenleri alma
            var management = allowedList.GroupBy(g => g.UnitName)
                .Select(s => new RVehicleFuelDto()
                {
                    UnitName = s.Key,
                    Count = s.Count(),
                    TotalKm = (decimal)s.Sum(x => x.EndKm - x.StartKm)
                }).ToList();

            if (management == null)
            {
                management.Add(new RVehicleFuelDto()
                {
                    UnitName = "",
                    TotalKm = 0,
                    Count = 0,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                });
            }

            return management;
        }//Müdürlük bazında rapor
        public async Task<List<RVehicleFuelDto>> SubProjectTrip(RFilterModelDto model)
        {
            model = SetStartEndDate(model);
            model.State = (int)TripState.EndTrip;
            var tripList = await GetTripList(model);

            var management = tripList.GroupBy(g => g.ProjectName).Select(s => new RVehicleFuelDto()
            {
                UnitName = s.Key,
                Count = s.Count(),
                TotalKm = (decimal)s.Sum(x => x.EndKm - x.StartKm)
            }).ToList();

            if (management == null)
            {
                management.Add(new RVehicleFuelDto()
                {
                    UnitName = "",
                    TotalKm = 0,
                    Count = 0,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                });
            }
            return management;
        }//Proje bazında rapor

        public async Task<List<ETripDto>> GetTripList(RFilterModelDto model)
        {
            var list = await Task.FromResult((from t in _tripRepository.GetAll()
                                              join u in _userRepository.GetAll() on t.DriverId equals u.Id
                                              join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                                              join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()
                                              where t.Status
                                              select new ETripDto()
                                              {
                                                  Plate = v.Plate,
                                                  ParentUnitId = unit2.Id,
                                                  UnitId = unit.Id,
                                                  UnitName = unit2.Code,
                                                  ProjectName = unit.Code,
                                                  CreatedBy = t.CreatedBy,
                                                  VehicleId = t.VehicleId,
                                                  DriverId = t.DriverId,
                                                  NameSurname = u.Name + " " + u.Surname,
                                                  StartDate = t.StartDate,
                                                  EndDate = t.EndDate,
                                                  StartKm = t.StartKm,
                                                  IsManagerAllowed = t.IsManagerAllowed,
                                                  EndKm = t.EndKm,
                                                  State = t.State
                                              }));

            if (model.StartDate != DateTime.MinValue)
                list = list.Where(w => w.StartDate >= model.StartDate && w.StartDate < model.EndDate);

            if (model.VehicleId > 0)
                list = list.Where(w => w.VehicleId == model.VehicleId);

            if (model.State > 0)
                list = list.Where(w => w.State == model.State);

            if (model.UnitId > 0)
                list = list.Where(w => w.UnitId == model.UnitId);

            if (model.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == model.ParentUnitId);

            if (model.CreatedBy > 0)
                list = list.Where(w => w.CreatedBy == model.CreatedBy);

            return list.ToList();
        }
        #endregion

        public RFilterModelDto SetStartEndDateTrip(RFilterModelDto model)
        {
            if (model.StartDate == DateTime.MinValue) //first load page
            {
                var dateNow = DateTime.Now;
                model.StartDate = new DateTime(dateNow.AddMonths(-1).Year, dateNow.AddMonths(-1).Month, 1);
                model.EndDate = dateNow;// new DateTime(2020, 12, 31);
                                        //model.DateMonth = model.StartDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("tr")) + "-" +
                                        //            model.EndDate.Value.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("tr"));
            }
            return model;
        }
        #endregion

        #region In-Memory Cache

        //private bool AddMemoryCache(string cacheKey, object list)
        //{
        //    bool result = false;
        //    try
        //    {
        //        //if (!_memoryCache.TryGetValue(cacheKey, out list))
        //        //{
        //        var options = new MemoryCacheEntryOptions()
        //        {
        //            AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
        //            Priority = CacheItemPriority.High,
        //        };
        //        _memoryCache.Set<object>(cacheKey, list, options);
        //        result = true;
        //        //}
        //    }
        //    catch (Exception e) { }

        //    return result;
        //}
        //private bool DeleteMemoryCache(string cacheKey)
        //{
        //    var result = false;
        //    try
        //    {
        //        _memoryCache.Remove(cacheKey);
        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        // ignored
        //    }

        //    return result;
        //}
        //public object GetMemoryCache(string cacheKey) => _memoryCache.Get<object>(cacheKey);
        #endregion
    }
}