using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Note;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.SignalR;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Transactions;
using Entity = CoreArchV2.Core.Enum.Entity;

namespace CoreArchV2.Services.Services
{
    public class VehicleService : IVehicleService
    {
        #region
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Color> _colorRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly ILogger<VehicleService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleAmount> _vehicleAmountRepository;
        private readonly IGenericRepository<VehicleBrandModel> _vehicleBrandModelRepository;
        private readonly IGenericRepository<VehicleCity> _vehicleCityRepository;
        private readonly IGenericRepository<VehicleContract> _vehicleContractRepository;
        private readonly IGenericRepository<VehicleDebit> _vehicleDebitRepository;
        private readonly IGenericRepository<VehicleExaminationDate> _vehicleExaminationDateRepository;
        private readonly IGenericRepository<VehicleFile> _vehicleFileRepository;
        private readonly IGenericRepository<VehicleMaterial> _vehicleMaterialRepository;
        private readonly IGenericRepository<VehicleRent> _vehicleRentRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehicleTransferFile> _vehicleTransferFileRepository;
        private readonly IGenericRepository<VehicleTransferLog> _vehicleTransferLogRepository;
        private readonly IGenericRepository<Maintenance> _vehicleMaintenanceRepository;
        private readonly IGenericRepository<FuelLog> _fuelGenericRepository;
        private readonly IGenericRepository<OneNote> _oneNoteGenericRepository;
        private readonly IGenericRepository<VehiclePhysicalImageFile> _vehiclePhysicalImageFileGenericRepository;
        private readonly IGenericRepository<VehiclePhysicalImage> _vehiclePhysicalImageGenericRepository;
        private readonly IReportService _reportService;
        private readonly ICacheService _cacheService;
        private readonly ILicenceWebService _licenceService;
        private readonly IWebHostEnvironment _env;
        private readonly IMailService _mailService;
        #endregion


        public VehicleService(IUnitOfWork uow,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<VehicleService> logger,
            IHubContext<SignalRHub> hubContext,
            IReportService reportService,
            IMailService mailService,
            IWebHostEnvironment env,
            ILicenceWebService licenceService)
        {
            _uow = uow;
            _env = env;
            _hubContext = hubContext;
            _mapper = mapper;
            _logger = logger;
            _mailService = mailService;
            _cacheService = cacheService;
            _licenceService = licenceService;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _vehicleDebitRepository = uow.GetRepository<VehicleDebit>();
            _vehicleExaminationDateRepository = uow.GetRepository<VehicleExaminationDate>();
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _cityRepository = uow.GetRepository<City>();
            _vehicleRentRepository = uow.GetRepository<VehicleRent>();
            _vehicleFileRepository = uow.GetRepository<VehicleFile>();
            _unitRepository = uow.GetRepository<Unit>();
            _vehicleCityRepository = uow.GetRepository<VehicleCity>();
            _vehicleTransferLogRepository = uow.GetRepository<VehicleTransferLog>();
            _vehicleTransferFileRepository = uow.GetRepository<VehicleTransferFile>();
            _vehicleBrandModelRepository = uow.GetRepository<VehicleBrandModel>();
            _colorRepository = uow.GetRepository<Color>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _messageRepository = uow.GetRepository<Message>();
            _vehicleMaterialRepository = uow.GetRepository<VehicleMaterial>();
            _vehicleAmountRepository = uow.GetRepository<VehicleAmount>();
            _vehicleContractRepository = uow.GetRepository<VehicleContract>();
            _vehicleMaintenanceRepository = uow.GetRepository<Maintenance>();
            _fuelGenericRepository = uow.GetRepository<FuelLog>();
            _oneNoteGenericRepository = uow.GetRepository<OneNote>();
            _vehiclePhysicalImageFileGenericRepository = uow.GetRepository<VehiclePhysicalImageFile>();
            _vehiclePhysicalImageGenericRepository = uow.GetRepository<VehiclePhysicalImage>();
            _reportService = reportService;
        }

        public PagedList<EVehicleDto> GetAllWithPaged(int? page, EVehicleDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            //Her tabloya left join yapılması gerekiyor
            var list = from v in _vehicleRepository.GetAll()
                       join u in _userRepository.GetAll() on v.CreatedBy equals u.Id
                       join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                       from vr in vrL.DefaultIfEmpty()
                       join u2 in _userRepository.GetAll() on v.LastUserId equals u2.Id into uL
                       from u2 in uL.DefaultIfEmpty()
                       join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                       from unit in unitL.DefaultIfEmpty()
                       join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                       from unit2 in unit2L.DefaultIfEmpty()
                       join model in _vehicleBrandModelRepository.GetAll() on v.VehicleModelId equals model.Id into modelL
                       from model in modelL.DefaultIfEmpty()
                       join model2 in _vehicleBrandModelRepository.GetAll() on model.ParentId equals model2.Id into model2L
                       from model2 in model2L.DefaultIfEmpty()
                       join l in _lookUpListRepository.GetAll() on v.FixtureTypeId equals l.Id into lL
                       from l in lL.DefaultIfEmpty()
                       join l2 in _lookUpListRepository.GetAll() on v.VehicleTypeId equals l2.Id into l2L
                       from l2 in l2L.DefaultIfEmpty()
                       join l3 in _lookUpListRepository.GetAll() on v.FuelTypeId equals l3.Id into l3L
                       from l3 in l3L.DefaultIfEmpty()
                       select new EVehicleDto
                       {
                           Id = v.Id,
                           Plate = "<a onclick='funcEditVehicle(" + v.Id +
                                   ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>",
                           Plate2 = v.Plate,
                           UnitId = unit.Id,
                           ParentUnitId = unit2.Id,
                           EngineNo = v.EngineNo,
                           DebitUserId = v.LastUserId,
                           ChassisNo = v.ChassisNo,
                           LastStatus = v.LastStatus,
                           FixtureTypeId = v.FixtureTypeId,
                           FirmTypeId = v.FixtureTypeId == (int)FixtureType.ForRent ? vr.FirmTypeId : 0,
                           CustomButton =
                               "<li title='Aracı düzenle' class='text-primary'><a onclick='funcEditVehicle(" +
                               v.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                               "<li class='text-danger'><a data-toggle='modal' onclick='funcDeleteVehicle(" +
                               v.Id + ");'><i class='icon-trash'></i></a></li>",
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           DebitNameSurname = u2.Name + " " + u2.Surname + "/" + u2.MobilePhone,
                           FixtureName = l.Name,
                           VehicleTypeName = l2.Name,
                           UsageTypeId = _vehicleDebitRepository.GetAll().Where(w => w.VehicleId == v.Id && w.Status && w.State == (int)DebitState.Debit).OrderByDescending(o => o.Id).FirstOrDefault().UsageTypeId,
                           FuelTypeName = l3.Name,
                           Status = v.Status,
                           IsLeasing = v.IsLeasing,
                           IsPartnerShipRent = v.IsPartnerShipRent,
                           GearTypeId = v.GearTypeId,
                           StatusName = v.Status
                               ? "<span class='label bg-green full-width'>Aktif</span>"
                               : "<span class='label bg-danger full-width'>Pasif</span>",
                           VehicleModelName = model2.Name != null ? model2.Name + "/" + model.Name : "",
                           UnitName = unit2.Name + "/" + unit.Name
                       };

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            if (filterModel.DebitUserId > 0)
                list = list.Where(w => w.DebitUserId == filterModel.DebitUserId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.Id == filterModel.VehicleId);

            if (filterModel.FirmTypeId > 0)
                list = list.Where(w => w.FirmTypeId == filterModel.FirmTypeId);

            if (filterModel.FixtureTypeId > 0)
                list = list.Where(w => w.FixtureTypeId == filterModel.FixtureTypeId);

            if (!string.IsNullOrEmpty(filterModel.Plate))
                list = list.Where(w => w.Plate2 == filterModel.Plate);

            if (filterModel.Status2 == 0)
                list = list.Where(w => w.Status == true);
            else if (filterModel.Status2 == 1)
                list = list.Where(w => w.Status == false);

            if (filterModel.UsageTypeId > 0)
                list = list.Where(w => w.UsageTypeId == filterModel.UsageTypeId);

            if (filterModel.GearTypeId > 0)
                list = list.Where(w => w.GearTypeId == filterModel.GearTypeId);

            var result = new PagedList<EVehicleDto>(list.OrderBy(o => o.UnitName), page, PagedCount.GridKayitSayisi);

            foreach (var item in result)
            {
                if (!item.Status)
                    item.CustomButton = "<li class='text-orange'><a data-toggle='modal' onclick='funcDeleteVehicle(" + item.Id + ");'><i class='icon-search4'></i></a></li>";

                if (item.LastStatus == (int)DebitState.Pool)
                    item.DebitNameSurname = "<span class='label bg-primary-300 full-width'>Havuzda</span>";
                else if (item.LastStatus == (int)DebitState.InService)
                    item.DebitNameSurname = "<span class='label bg-danger-300 full-width'>Serviste</span>";
            }
            return result;
        }
        public Vehicle FindPlate(string plate)
        {
            var result = _vehicleRepository.FirstOrDefault(w => w.Plate == plate);
            return result;
        }
        public List<Vehicle> FindPlateList(string plate)
        {
            var result = _vehicleRepository
                .GetAll().Where(w => w.Plate == plate)
                .ToList();
            return result;
        }
        public List<EVehicleDto> GetAllVehicleList(EVehicleDto filterModel)
        {
            var list = (from v in _vehicleRepository.GetAll()
                        join k in _vehicleExaminationDateRepository.GetAll() on v.Id equals k.VehicleId
                        join c in _cityRepository.GetAll() on v.LastCityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        join u in _userRepository.GetAll() on v.CreatedBy equals u.Id
                        join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                        from vr in vrL.DefaultIfEmpty()
                        join u2 in _userRepository.GetAll() on v.LastUserId equals u2.Id into uL
                        from u2 in uL.DefaultIfEmpty()
                        join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                        from unit in unitL.DefaultIfEmpty()
                        join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                        from unit2 in unit2L.DefaultIfEmpty()
                        join model in _vehicleBrandModelRepository.GetAll() on v.VehicleModelId equals model.Id into modelL
                        from model in modelL.DefaultIfEmpty()
                        join model2 in _vehicleBrandModelRepository.GetAll() on model.ParentId equals model2.Id into model2L
                        from model2 in model2L.DefaultIfEmpty()
                        join lFirm in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lFirm.Id into lFirmL
                        from lFirm in lFirmL.DefaultIfEmpty()
                        join lGear in _lookUpListRepository.GetAll() on v.GearTypeId equals lGear.Id into lGearL
                        from lGear in lGearL.DefaultIfEmpty()
                        select new EVehicleDto
                        {
                            VehicleId = v.Id,
                            Plate = v.Plate,
                            Status = v.Status,
                            StatusName = v.Status == false ? "Silinmiş" : "Aktif",
                            ChassisNo = v.ChassisNo,
                            EngineNo = v.EngineNo,
                            VehicleModelName = model2.Name != null ? model2.Name + "/" + model.Name : "",
                            LicenceSeri = v.LicenceSeri,
                            LicenceNo = v.LicenceNo,
                            ModelYear = v.ModelYear,
                            FixtureName = v.FixtureTypeId != null ? _lookUpListRepository.Find(v.FixtureTypeId.Value).Name : "",
                            FixtureTypeId = v.FixtureTypeId,
                            FirmTypeId = v.FixtureTypeId == (int)FixtureType.ForRent ? vr.FirmTypeId : 0,
                            VehicleTypeName = v.VehicleTypeId != null
                                ? _lookUpListRepository.Find(v.VehicleTypeId.Value).Name
                                : "",
                            FuelTypeName = v.FuelTypeId != null ? _lookUpListRepository.Find(v.FuelTypeId.Value).Name : "",
                            UnitName = unit2.Name != null ? unit2.Name + "/" + unit.Name : "",
                            UnitId = unit.Id,
                            ParentUnitId = unit2.Id,
                            RentFirmName = lFirm.Name,
                            UsageTypeId = _vehicleDebitRepository.GetAll().Where(w => w.VehicleId == v.Id && w.Status && w.State == (int)DebitState.Debit).OrderByDescending(o => o.Id).FirstOrDefault().UsageTypeId,
                            ArventoNo = v.ArventoNo,
                            DebitUserId = u2.Id,
                            IsTtsName = v.IsTts == true ? "Var" : "Yok",
                            IsHgsName = v.IsHgs == true ? "Var" : "Yok",
                            DebitNameSurname = u2.Name != null ? u2.Name + " " + u2.Surname + "/" + u2.MobilePhone : "",
                            GearTypeName = lGear.Name,
                            PartnerShipName = v.IsPartnerShipRent == true ? "Var" : "Yok",
                            LeasingName = v.IsLeasing == true ? "Evet" : "Hayır",
                            LastKm = v.LastKm,
                            LastCityName = c2.Name + "-" + c.Name,
                            KDocumentEndDate = k.KDocumentEndDate,
                            KaskoEndDate = k.KaskoEndDate,
                            TrafficEndDate = k.TrafficEndDate,
                            ExaminationEndDate = k.ExaminationEndDate
                        });

            if (filterModel.DebitUserId > 0)
                list = list.Where(w => w.DebitUserId == filterModel.DebitUserId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.Id == filterModel.VehicleId);

            if (filterModel.FirmTypeId > 0)
                list = list.Where(w => w.FirmTypeId == filterModel.FirmTypeId);

            if (filterModel.FixtureTypeId > 0)
                list = list.Where(w => w.FixtureTypeId == filterModel.FixtureTypeId);

            if (!string.IsNullOrEmpty(filterModel.Plate))
                list = list.Where(w => w.Plate2 == filterModel.Plate);

            if (filterModel.Status2 == 0)//Aktif
                list = list.Where(w => w.Status);
            else if (filterModel.Status2 == 1)//Pasif
                list = list.Where(w => !w.Status);

            if (filterModel.UsageTypeId > 0)
                list = list.Where(w => w.UsageTypeId == filterModel.UsageTypeId);

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            var result = list.ToList();
            for (int i = 0; i < result.Count(w => w.FixtureTypeId == (int)FixtureType.ForRent); i++)
            {
                var contract = _vehicleContractRepository.Where(f => f.VehicleId == result[i].VehicleId).OrderByDescending(o => o.CreatedDate).Take(1).FirstOrDefault();
                if (contract != null)
                    result[i].ContractDateRange = contract.StartDate.ToString("dd/MM/yyyy") + " - " +
                                                  contract.EndDate.ToString("dd/MM/yyyy");
            }

            foreach (var item in result)
            {
                var lastDebit = (from vd in _vehicleDebitRepository.GetAll()
                                 join l in _lookUpListRepository.GetAll() on vd.UsageTypeId equals l.Id
                                 where vd.Status && vd.VehicleId == item.VehicleId
                                 select new EVehicleDto()
                                 {
                                     VehicleDebitId = vd.Id,
                                     UsageTypeName = l.Name
                                 }).OrderByDescending(o => o.VehicleDebitId).FirstOrDefault();

                item.UsageTypeName = lastDebit?.UsageTypeName;
            }
            return result;
        }
        public List<EVehicleDto> GetActiveVehicleForExcel(RFilterModelDto filterModel)
        {
            var list = (from v in _vehicleRepository.GetAll()
                        join c in _cityRepository.GetAll() on v.LastCityId equals c.Id into cL
                        from c in cL.DefaultIfEmpty()
                        join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id into c2L
                        from c2 in c2L.DefaultIfEmpty()
                        join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                        from vr in vrL.DefaultIfEmpty()
                        join u in _userRepository.GetAll() on v.CreatedBy equals u.Id
                        join u2 in _userRepository.GetAll() on v.LastUserId equals u2.Id into uL
                        from u2 in uL.DefaultIfEmpty()
                        join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                        from unit in unitL.DefaultIfEmpty()
                        join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                        from unit2 in unit2L.DefaultIfEmpty()
                        join model in _vehicleBrandModelRepository.GetAll() on v.VehicleModelId equals model.Id into modelL
                        from model in modelL.DefaultIfEmpty()
                        join model2 in _vehicleBrandModelRepository.GetAll() on model.ParentId equals model2.Id into model2L
                        from model2 in model2L.DefaultIfEmpty()
                        join lFirm in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lFirm.Id into lFirmL
                        from lFirm in lFirmL.DefaultIfEmpty()
                        join fix in _lookUpListRepository.GetAll() on v.FixtureTypeId equals fix.Id
                        where v.Status
                        select new EVehicleDto
                        {
                            VehicleId = v.Id,
                            FixtureTypeId = v.FixtureTypeId,
                            Plate = v.Plate,
                            UnitId = unit.Id,
                            ParentUnitId = unit2.Id,
                            LastKm = v.LastKm,
                            LastCityName = c2.Name + "-" + c.Name,
                            VehicleModelName = model2.Name != null ? model2.Name + "/" + model.Name : "",
                            UnitParentName = v.LastUnitId == null ? "-" : unit2.Name,
                            UnitName = v.LastUnitId == null ? "-" : unit.Name,
                            RentFirmName = lFirm.Name ?? "-",
                            ContractDateRange = "-",
                            FixtureName = fix.Name,
                            DebitNameSurname = v.LastUnitId == null ? "Havuzda" : (u2.Name != null ? u2.Name + " " + u2.Surname + "/" + u2.MobilePhone : "")
                        });

            //Müdürlük bazında filtre
            if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            //Proje bazında filtre
            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            var result = list.ToList();
            foreach (var item in result)
            {
                var contract = _vehicleContractRepository.Where(w => w.Status && w.VehicleId == item.VehicleId)
                    .OrderByDescending(o => o.Id).FirstOrDefault();//son sözleşme getir
                var rentCost = _vehicleAmountRepository.Where(w => w.Status && w.VehicleId == item.VehicleId && w.TypeId == (int)VehicleAmountType.KiraBedeli)
                    .OrderByDescending(o => o.EndDate).FirstOrDefault();

                if (contract != null)
                    item.ContractDateRange = contract.StartDate.ToString("dd/MM/yyyy") + " - " + contract.EndDate.ToString("dd/MM/yyyy");

                if (contract != null && rentCost != null)
                {
                    item.ContractPrice = rentCost.Amount;
                    item.TotalPrice = SubTotalCost(contract.EndDate, contract.StartDate, rentCost.Amount);
                }
            }

            foreach (var item in result)
            {
                var lastDebit = (from vd in _vehicleDebitRepository.GetAll()
                                 join l in _lookUpListRepository.GetAll() on vd.UsageTypeId equals l.Id
                                 where vd.Status && vd.VehicleId == item.VehicleId
                                 select new EVehicleDto()
                                 {
                                     VehicleDebitId = vd.Id,
                                     UsageTypeName = l.Name
                                 }).OrderByDescending(o => o.VehicleDebitId).FirstOrDefault();

                item.UsageTypeName = lastDebit?.UsageTypeName;
            }

            return result.OrderBy(o => o.FixtureTypeId).ToList();

        }

        //#region Vehicle Gain
        //public async Task<List<EVehicleAmountDto>> GetVehicleGain(int vehicleId)
        //{
        //    //Aracın ilk alındığı günden bu yana kazancı ne kadar
        //    //(Araç alım fiyatı + bakım onarım) - (toplam gider + toplam bakım onarım + extra tutar)
        //    try
        //    {
        //        var dateNow = DateTime.Now;
        //        var vehicle = await _vehicleRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == vehicleId);

        //        var vehiclePrice = vehicle.VehicleFirstCost ?? 0;//Araç alım fiyatı

        //        var totalMaintananceAndUserFault = _vehicleMaintenanceRepository.Where(w => w.Status && w.VehicleId == vehicleId).ToList();
        //        var totalMaintanance = totalMaintananceAndUserFault.Sum(s => s.TotalAmount);//Toplan Bakım-Onarım
        //        var totalMaintananceUserFault = totalMaintananceAndUserFault.Sum(s => s.UserFaultAmount);//Toplam Bakım-Onarım Kullanıcı hatası

        //        var vehicleExpenseIncome = GetByVehicleIdVehicleAmountHistory(vehicleId, 0, false).VehicleAmountList;
        //        var totalExpense = vehicleExpenseIncome.Sum(s => s.AllTotalExpense);//Toplam gider
        //        //var totalIncome = vehicleExpenseIncome.Sum(s => s.AllTotalIncome);//Toplam gelir
        //        var extraTutar = vehicleExpenseIncome.Sum(s => s.ExtraAmount);//Toplam Extra Tutar


        //        return null;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //#endregion

        #region Mail

        public async Task<EResultDto> SendMailVehiclePhotography(EMailVehiclePhotographyDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var userMail = "";

                if (model.UserId > 0)
                {
                    var user = _userRepository.FirstOrDefault(f => f.Id == model.UserId && f.IsSendMail);
                    if (user == null)
                    {
                        result.Message = "Kullanıcı bulunamadı. Not: Mail gönderim onay kapalı olabilir";
                        return result;
                    }
                    userMail = user.Email;
                }

                if (!string.IsNullOrEmpty(model.SenderMailTo))
                    userMail += ";" + model.SenderMailTo;

                if (string.IsNullOrEmpty(userMail))
                {
                    result.Message = "Gönderilecek mail adresi giriniz";
                    return result;
                }

                var photoList = (from f in _vehiclePhysicalImageGenericRepository.GetAll()
                                 join fl in _vehiclePhysicalImageFileGenericRepository.GetAll() on f.Id equals fl.VehiclePhysicalImageId
                                 join file in _fileUploadRepository.GetAll() on fl.FileUploadId equals file.Id
                                 where f.Status && f.VehicleId == model.VehicleId && f.Id == model.VehiclePhysicalImageId
                                 select file).ToList();

                //var rootUrl = Path.Combine(_env.WebRootPath, "uploads/physicalimage/");


                var path = Directory.GetCurrentDirectory();

                var filePath = Path.Combine($"{path}/wwwroot/uploads/physicalimage/");

                if (photoList.Any())
                {
                    var send = _mailService.SendMailWithAttach(new EMailDto()
                    {
                        To = userMail,
                        Cc = model.SenderMailCc,
                        Bcc = model.SenderMailBcc,
                        Subject = model.Subject,
                        Body = model.Body,
                        RootUrl = filePath,
                        Attachments = photoList
                    });
                    result.IsSuccess = send;
                    result.Message = send == true ? "İşlem Başarılı" : "Hata Oluştu";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Gönderilecek fotoğraf olmadığı için gönderim yapılamadı.";
                }

                return result;
            }
            catch (Exception ex) { result.Message = "Kayıt sırasında hata oluştu!"; }
            return result;
        }
        #endregion


        #region Vehicle Table

        public EResultDto InsertVehicle(EVehicleFixRentDto tempModel)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                tempModel.EntityId = (int)Entity.FirmName;
                tempModel.Plate = tempModel.Plate.ToUpper();

                if (_vehicleRepository.Any(a => a.Status && a.Plate.ToUpper() == tempModel.Plate)/* isPlate != null && isPlate.FirstOrDefault(w => w.Status == true) != null*/)
                    result.Message = "Bu plakada aktif araç bulunmaktadır.<br/>Aynı plakada araç eklenemez";
                else
                {
                    //Lisans kontrolü
                    var licence = _licenceService.AddVehicle(tempModel.CreatedBy, JsonConvert.SerializeObject(tempModel));//Lisans araç ekleme
                    if (licence.Result.StatusCode == System.Net.HttpStatusCode.OK && licence.Result.Success)
                    {
                        if (tempModel.Id > 0)
                            result = VehicleActivePassiveControl(tempModel.VehicleId);
                        else
                        {
                            result = ArventoNoControl(tempModel.ArventoNo, null);
                            if (result.IsSuccess)
                            {
                                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //Vehicle
                                    var entity = InsertVehicleTable(tempModel);

                                    //VehicleRent
                                    if (tempModel.FixtureTypeId == (int)FixtureType.ForRent)
                                        InsertVehicleRent(tempModel, entity.Id);

                                    _uow.SaveChanges();
                                    scope.Complete();
                                    result.Id = entity.Id;
                                    result.IsSuccess = true;
                                    result.Message = "Kayıt başarıyla eklenmiştir";
                                    //await _cacheService.Clear(MemoryCache_.HomePageActiveVehicle);
                                }
                            }
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = licence.Result.Data;
                    }
                }
            }
            catch (Exception ex) { result.Message = "Kayıt sırasında hata oluştu!"; }
            return result;
        }
        public Vehicle InsertVehicleTable(EVehicleFixRentDto tempModel)
        {
            try
            {
                var model = _mapper.Map<Vehicle>(tempModel);
                model.CreatedBy = tempModel.CreatedBy;
                model.Plate = model.Plate.ToUpper().Replace(" ", "").Trim().ToUpper();
                var entity = _vehicleRepository.Insert(model);
                _uow.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                return new Vehicle();
            }
        }
        public void InsertVehicleRent(EVehicleFixRentDto tempModel, int vehicleId)
        {
            var modelVehicleRent = _mapper.Map<VehicleRent>(tempModel);
            modelVehicleRent.VehicleId = vehicleId;
            _vehicleRentRepository.Insert(modelVehicleRent);
        }
        public EResultDto UpdateVehicle(EVehicleFixRentDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(tempModel.Id);
                if (result.IsSuccess)
                {
                    var entity = _vehicleRepository.FindForInsertUpdateDelete(tempModel.Id);
                    if (entity.Plate != tempModel.Plate && _vehicleRepository.Any(a =>
                            a.Status && a.Id != tempModel.Id &&
                            a.Plate.ToUpper() == tempModel.Plate.Replace(" ", "").ToUpper()))
                    {
                        result.Message = "Bu plakada aktif araç bulunmaktadır.<br/>Aynı plakada araç eklenemez";
                        result.IsSuccess = false;
                    }
                    else
                    {
                        result = ArventoNoControl(tempModel.ArventoNo, tempModel.Id);
                        if (result.IsSuccess)
                        {
                            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                if (tempModel.FixtureTypeId == (int)FixtureType.ForRent) //Kiralık
                                {
                                    var entityRent = _vehicleRentRepository.Find(tempModel.VehicleRentId);
                                    var vehicleRent = _mapper.Map(tempModel, entityRent);
                                    vehicleRent.VehicleId = tempModel.Id;
                                    _vehicleRentRepository.Update(vehicleRent);
                                }

                                var newEntity = _mapper.Map(tempModel, entity);
                                _vehicleRepository.Update(newEntity);

                                _uow.SaveChanges();
                                scope.Complete();
                                result.Message = "Kayıt başarıyla güncellenmiştir";
                                result.Id = entity.Id;
                                result.IsSuccess = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { result.Message = "Güncelleme sırasında hata oluştu!"; result.IsSuccess = false; }
            return result;
        }
        public EResultDto ArventoNoControl(string arventoNo, int? vehicleId)
        {
            var result = new EResultDto() { IsSuccess = true };
            if (!string.IsNullOrEmpty(arventoNo))
            {
                var arventoNumberList = _vehicleRepository.Where(w => w.ArventoNo == arventoNo).ToList();
                if (vehicleId > 0)
                    arventoNumberList = arventoNumberList.Where(w => w.Id != vehicleId).ToList();
                if (arventoNumberList.Any())
                {

                    result.Message = "Bu plakaya bağlı arvento numarası bulunmaktadır.<br/>Plaka(lar): " + string.Join(",", arventoNumberList.Select(s => s.Plate).ToArray());
                    result.IsSuccess = false;
                }
            }
            return result;
        }
        public EVehicleFixRentDto GetByIdVehicle(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _vehicleRepository.Find(id);
                var model = _mapper.Map<EVehicleFixRentDto>(entity);

                //VehicleRent Table
                if (entity.FixtureTypeId == (int)FixtureType.ForRent)
                {
                    var vehicleRent = _vehicleRentRepository.Where(w => w.VehicleId == entity.Id).FirstOrDefault();
                    model = _mapper.Map<EVehicleFixRentDto>(vehicleRent);
                    //Ortak bilgiler
                    model.Status = entity.Status;
                    model.VehicleTypeId = entity.VehicleTypeId;
                    model.FixtureTypeId = entity.FixtureTypeId;
                    model.Plate = entity.Plate;
                    model.VehicleModelId = entity.VehicleModelId;
                    model.VehicleRentId = vehicleRent.Id;
                    model.ArventoNo = entity.ArventoNo;
                    model.MaxSpeed = entity.MaxSpeed;
                    model.IsHgs = entity.IsHgs;
                    model.EnginePowerId = entity.EnginePowerId;
                    model.IsTts = entity.IsTts;
                    model.ModelYear = entity.ModelYear;
                    model.IsLeasing = entity.IsLeasing;
                    model.IsPartnerShipRent = entity.IsPartnerShipRent;
                    model.GearTypeId = entity.GearTypeId;
                    model.LastKm = entity.LastKm;
                }

                model.Id = entity.Id;
                return model;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Veri çekme sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
                return new EVehicleFixRentDto();
            }
        }
        public async Task<EResultDto> BulkCostUpdate(decimal amountPercent)
        {
            var result = new EResultDto();
            try
            {
                //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                //{
                var allActiveVehicle = await Task.FromResult(_vehicleRepository.Where(w => w.Status && w.FixtureTypeId == (int)FixtureType.Ownership).ToList());

                var lastCostlist = allActiveVehicle.Where(w => w.VehicleLastCost > 0).ToList();
                if (lastCostlist.Any())
                {
                    lastCostlist.ForEach(f => f.VehicleLastCost = f.VehicleLastCost + (f.VehicleLastCost * amountPercent / 100));
                    _vehicleRepository.UpdateRange(allActiveVehicle);

                    _uow.SaveChanges();

                    result.Message = "Sistemde kayıtlı toplam <b>" + allActiveVehicle.Count + "</b> adet mülkiyet araç içinde, <b>" + lastCostlist.Count + "</b> adet aracın güncel bedeli %" + amountPercent + " oranında değişmiştir.";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Aktif araç listesi içinde güncel bedeli girilen araç bulunamadı";
                }
                //    scope.Complete();
                //}
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }
        public Vehicle GetByIdJustVehicle(int id) => _vehicleRepository.FirstOrDefault(f => f.Id == id);
        public List<EVehicleDto> GetAllVehicleList()
        {
            var list = (from v in _vehicleRepository.GetAll()
                        where v.Status
                        select new EVehicleDto()
                        {
                            VehicleId = v.Id,
                            Plate = v.Plate
                        }).ToList();
            return list;
        }


        #endregion

        #region VehicleExaminationDate Table

        public EResultDto InsertVehicleExaminationDate(VehicleExaminationDate model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var entity = _vehicleExaminationDateRepository.Insert(model);
                    _uow.SaveChanges();
                    result.Id = model.Id;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        public EResultDto UpdateVehicleExaminationDate(VehicleExaminationDate tempModel)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(tempModel.VehicleId);
                if (result.IsSuccess)
                {
                    var entity = _vehicleExaminationDateRepository.Find(tempModel.Id);
                    var newEntity = _mapper.Map(tempModel, entity);
                    _vehicleExaminationDateRepository.Update(newEntity);
                    _uow.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Güncelleme sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        public VehicleExaminationDate GetByIdVehicleExaminationDate(int id)
        {
            return _vehicleExaminationDateRepository.FirstOrDefault(f => f.VehicleId == id);
        }

        #endregion

        #region VehicleDebit Table
        public EResultDto InsertVehicleDebit(VehicleDebit model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var vehicleEntity = _vehicleRepository.Find(model.VehicleId);
                        var oldEndtity = _vehicleDebitRepository.Where(w => w.VehicleId == model.VehicleId)
                            .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                        if (oldEndtity != null && oldEndtity.StartDate >= model.StartDate)
                        {
                            result.IsSuccess = false;
                            result.Message = "Zimmet tarihi <b>" + oldEndtity.StartDate.ToString("dd/MM/yyy") +
                                             "</b> tarihinden sonra olmalıdır";
                            return result;
                        }

                        //if (vehicleEntity.LastStatus == (int)DebitState.InService)
                        //{
                        //    result.IsSuccess = false;
                        //    result.Message = "Araç serviste olduğundan çıkarıp öyle zimmet yapabilirsiniz";
                        //    return result;
                        //}

                        //Önceki zimmet kaydının endDate'i şimdiki zimmetin startDate'i olarak güncelle
                        if (oldEndtity != null)
                        {
                            oldEndtity.EndDate = model.StartDate;
                            _vehicleDebitRepository.Update(oldEndtity);
                        }

                        //aynı plakadan daha önce eklenmişse endDate < şimdiki startDate olmalı
                        var beforeRecordPlate = _vehicleRepository.Where(w => w.Plate == vehicleEntity.Plate && w.Id != vehicleEntity.Id).OrderByDescending(o => o.Id).FirstOrDefault();
                        if (beforeRecordPlate != null)
                        {
                            var lastDebit = _vehicleDebitRepository.Where(w => w.VehicleId == beforeRecordPlate.Id && w.State == (int)DebitState.Debit)
                                .OrderByDescending(o => o.Id).FirstOrDefault();

                            if (lastDebit != null && lastDebit.EndDate > model.StartDate)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bu araç daha önce eklenip silinmiş,şimdi zimmet tarihi önceki aracın zimmet bitiş tarihinden sonra olmalıdır. " +
                                                 "Önceki araç bilgileri:<br/> " +
                                                 "Plaka: <b>" + beforeRecordPlate.Plate + " (" + beforeRecordPlate.Id + ")</b><br/>" +
                                                 "Zimmet bitiş tarihi: <b>" + lastDebit.EndDate.Value.ToString("dd/MM/yyy") + "</b>";
                                return result;
                            }
                        }

                        //DebitHistory insert
                        model.State = (int)DebitState.Debit;
                        var entity = _vehicleDebitRepository.Insert(model);

                        //Vehicle update
                        //var vehicleEntity = _vehicleRepository.Find(model.VehicleId);
                        vehicleEntity.LastUserId = entity.DebitUserId;
                        vehicleEntity.LastUnitId = entity.UnitId;
                        vehicleEntity.LastStatus = (int)DebitState.Debit;
                        _vehicleRepository.Update(vehicleEntity);

                        _uow.SaveChanges();
                        scope.Complete();
                        result.Id = entity.Id;
                        result.Message = "Araç kullanıcıya başarıyla zimmetlendi";
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }

            return result;
        }
        public EResultDto UpdateVehicleDebit(VehicleDebit model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var oldDebit = _vehicleDebitRepository.FindForInsertUpdateDelete(model.Id);
                    if (oldDebit != null)
                    {
                        if (oldDebit.StartDate != model.StartDate)
                        {
                            result.IsSuccess = false;
                            result.Message = "Zimmet tarihi değiştirilemez. </br><b>Not: Zimmetli geçmisinde ilgili satırı silip tekrar ekleyebilirsiniz.</b>";
                            return result;
                        }

                        var lastDebit = _vehicleDebitRepository.Where(w => w.VehicleId == oldDebit.VehicleId)
                            .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                        if (lastDebit.Id == model.Id)//son zimmetli bilgilerini değişebilir
                        {
                            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                var debit = _mapper.Map(model, oldDebit);
                                _vehicleDebitRepository.Update(debit);

                                var vehicle = _vehicleRepository.FindForInsertUpdateDelete(model.VehicleId);
                                vehicle.LastUnitId = debit.UnitId;
                                vehicle.LastUserId = debit.DebitUserId;
                                _vehicleRepository.Update(vehicle);

                                _uow.SaveChanges();
                                scope.Complete();
                                result.Message = "Düzenleme işlemi başarılı";
                            }
                        }
                        else
                            result.Message = "Sadece son zimmetli bilgileri üzerinden işlem yapılabilir";
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }
        public List<EVehicleDto> GetLastDebitUserHistory(int vehicleId)
        {
            try
            {
                var result = (from vd in _vehicleDebitRepository.GetAll()
                              join l in _lookUpListRepository.GetAll() on vd.UsageTypeId equals l.Id into lL
                              from l in lL.DefaultIfEmpty()
                              join v in _vehicleRepository.GetAll() on vd.VehicleId equals v.Id
                              join u in _userRepository.GetAll() on vd.DebitUserId equals u.Id into uL
                              from u in uL.DefaultIfEmpty()
                              join u2 in _userRepository.GetAll() on vd.CreatedBy equals u2.Id
                              join du in _userRepository.GetAll() on vd.DeliveryUserId equals du.Id into duL
                              from du in duL.DefaultIfEmpty()
                              join c in _cityRepository.GetAll() on vd.CityId equals c.Id into cL
                              from c in cL.DefaultIfEmpty()
                              join un in _unitRepository.GetAll() on vd.UnitId equals un.Id into unL
                              from un in unL.DefaultIfEmpty()
                              join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                              from un1 in un1L.DefaultIfEmpty()
                              where vd.Status && vd.VehicleId == vehicleId && vd.Status
                              select new EVehicleDto
                              {
                                  VehicleDebitId = vd.Id,
                                  VehicleId = v.Id,
                                  Status = v.Status,
                                  UnitName = un1.Name + "-" + un.Name,
                                  DeliveryUserName = du.Name != null ? du.Name + " " + du.Surname : "",
                                  CreatedUserName = u2.Name + " " + u2.Surname,
                                  CreatedDate = vd.CreatedDate,
                                  DebitNameSurname = u.Name + " " + u.Surname + "/" + u.MobilePhone,
                                  DebitCreatedDate = vd.CreatedDate,
                                  DebitStartDate = vd.StartDate,
                                  DebitEndDate = vd.EndDate,
                                  UnitId = vd.UnitId, //not delete
                                  Plate = v.Plate,
                                  LastKm = vd.LastKm,
                                  DebitState = u.Name + " " + u.Surname + "/" + u.MobilePhone,
                                  DebitState2 = vd.State, //not delete
                                  LastStatus = v.LastStatus,
                                  CityName = c.Name ?? "",
                                  DebitUserId = vd.DebitUserId, //not delete
                                  UsageTypeName = l.Name,
                                  CustomButton = "",
                                  Description = vd.Description,
                                  TempPlateNo = vd.TempPlateNo
                              }).OrderByDescending(o => o.VehicleDebitId).Take(30).ToList();


                foreach (var item in result)
                {
                    if (item.DebitState2 == (int)DebitState.Pool)
                        item.DebitState = "<span style='width: 100%;' class='label bg-orange-800'>Havuz</span>";
                    else if (item.DebitState2 == (int)DebitState.InService)
                        item.DebitState = "<span style='width: 100%;' class='label bg-orange-300'>Servis</span>";
                }

                return result;
            }
            catch (Exception)
            {
                return new List<EVehicleDto>();
            }
        }
        public EVehicleDto GetByIdVehicleDebit(int vehicleId)
        {
            var result = GetLastDebitUserHistory(vehicleId)
                .Where(w => w.DebitState2 == (int)DebitState.Debit || w.DebitState2 == (int)DebitState.Pool || w.DebitState2 == (int)DebitState.InService).Take(1)
                .FirstOrDefault();
            if (result != null && !result.Status)//result.Status--> Araç silinmiş demek
            {
                var debitEndDate = result.DebitEndDate;
                var files = (from vt in _vehicleTransferLogRepository.GetAll()
                             join vf in _vehicleTransferFileRepository.GetAll() on vt.Id equals vf.VehicleTransferId
                             join fu in _fileUploadRepository.GetAll() on vf.FileUploadId equals fu.Id
                             where vt.VehicleId == result.VehicleId
                             select new EFileUploadDto
                             {
                                 Id = fu.Id,
                                 VehicleId = vt.VehicleId,
                                 VehicleFileId = vf.Id,
                                 Name = fu.Name,
                                 Extention = fu.Extention,
                                 FileSize = fu.FileSize
                             }).ToList();

                result = GetLastDebitUserHistory(vehicleId).Take(1).FirstOrDefault();//silme tarihi ve bilgileri
                result.DebitEndDate = debitEndDate;
                result.VehicleTransfer = (from vt in _vehicleTransferLogRepository.GetAll()
                                          join l in _lookUpListRepository.GetAll() on vt.TransferTypeId equals l.Id
                                          where vt.VehicleId == vehicleId
                                          select new EVehicleTransferFileDto()
                                          {
                                              VehicleId = vt.VehicleId,
                                              SalesCost = vt.SalesCost,
                                              TransferTypeId = vt.TransferTypeId,
                                              TransferTypeName = l.Name,
                                              DebitVehicleEndDate = debitEndDate.Value,
                                              Date = vt.Date,
                                              Description = vt.Description
                                          }).FirstOrDefault();

                result.files = files;
            }
            return result;
        }
        public VehicleDebit GetByVehicleDebitId(int vehicleDebitId) => _vehicleDebitRepository.Find(vehicleDebitId);
        public VehicleDebit GetByDebitWithVehicleId(int vehicleId) => //vehicleId değerine göre son zimmet bilgileri geitirir
            _vehicleDebitRepository.Where(w => w.VehicleId == vehicleId).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
        public EResultDto VehicleDebitSetUserNull(VehicleDebit model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var lastVehicleDebit = _vehicleDebitRepository.Where(w => w.VehicleId == model.VehicleId)
                                       .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();

                    if (lastVehicleDebit.State == (int)DebitState.Pool)
                    {
                        result.IsSuccess = false;
                        result.Message = "Araç zaten havuza alınmıştır";
                    }
                    else if (model.EndDate > DateTime.Now)
                    {
                        result.IsSuccess = false;
                        result.Message = "Havuza alma tarihi, bugünden sonraki gün olamaz.";
                    }
                    else if (lastVehicleDebit.StartDate >= model.EndDate)
                    {
                        result.IsSuccess = false;
                        result.Message = "Havuza alma tarihi, bir önceki işlem başlangıç tarihi olan " +
                                         lastVehicleDebit.StartDate.ToString("dd/MM/yyy") + " tarihinden sonra olmalıdır";
                    }
                    else
                    {
                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Vehicle last değerler güncelleniyor
                            var vehicle = _vehicleRepository.Find(model.VehicleId);
                            vehicle.LastStatus = (int)DebitState.Pool;
                            vehicle.LastUserId = null;
                            vehicle.LastUnitId = null;
                            _vehicleRepository.Update(vehicle);

                            //VehicleDebit endDate tarihi güncelleniyor
                            lastVehicleDebit.EndDate = model.EndDate;
                            _vehicleDebitRepository.Update(lastVehicleDebit);

                            //VehicleDebit tablosuna havuza atıldığına dair yeni satır ekleniyor
                            var pool = new VehicleDebit
                            {
                                CreatedBy = model.CreatedBy,
                                StartDate = model.EndDate.Value,
                                VehicleId = vehicle.Id,
                                State = (int)DebitState.Pool,
                                Description = model.Description
                            };
                            _vehicleDebitRepository.Insert(pool);

                            _uow.SaveChanges();
                            scope.Complete();
                            result.IsSuccess = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
            }

            return result;
        }

        public EResultDto VehicleSetService(EVehicleServiceDto model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var lastDebit = _vehicleDebitRepository.Any(w => w.VehicleId == model.VehicleId);
                    if (!lastDebit)
                    {
                        result.IsSuccess = false;
                        result.Message = "Araca daha önce zimmet yapılmamış, zimmet yapıp sonra servise alabilirsiniz.";
                        return result;
                    }

                    if (model.Id > 0)//edit button
                    {
                        var vehicleDebit = _vehicleDebitRepository.FindForInsertUpdateDelete(model.Id);
                        //vehicleDebit.StartDate = model.StartDate;//tarihi güncelleyemez
                        vehicleDebit.Description = model.Description;
                        vehicleDebit.TempPlateNo = model.TempPlateNo;
                        _vehicleDebitRepository.Update(vehicleDebit);
                        _uow.SaveChanges();
                    }
                    else
                    {
                        var lastVehicleDebit = _vehicleDebitRepository.Where(w => w.VehicleId == model.VehicleId && w.Status)
                                           .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();

                        if (lastVehicleDebit.State == (int)DebitState.InService)
                        {
                            result.IsSuccess = false;
                            result.Message = "Araç zaten servise alınmıştır";
                        }
                        else if (lastVehicleDebit.StartDate >= model.StartDate)
                        {
                            result.IsSuccess = false;
                            result.Message = "Servise alma tarihi, bir önceki işlem başlangıç tarihi olan " +
                                             lastVehicleDebit.StartDate.ToString("dd/MM/yyy") + " tarihinden sonra olmalıdır";
                        }
                        else
                        {
                            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //Vehicle last değerler güncelleniyor
                                var vehicle = _vehicleRepository.Find(model.VehicleId);
                                vehicle.LastStatus = (int)DebitState.InService;
                                vehicle.LastUserId = null;
                                _vehicleRepository.Update(vehicle);
                                _uow.SaveChanges();

                                lastVehicleDebit.EndDate = model.StartDate;
                                _vehicleDebitRepository.Update(lastVehicleDebit);
                                _uow.SaveChanges();

                                var inservice = new VehicleDebit
                                {
                                    CreatedBy = model.CreatedBy,
                                    StartDate = model.StartDate,
                                    VehicleId = vehicle.Id,
                                    UnitId = vehicle.LastUnitId,
                                    State = (int)DebitState.InService,
                                    TempPlateNo = model.TempPlateNo,
                                    Description = model.Description
                                };
                                _vehicleDebitRepository.Insert(inservice);
                                _uow.SaveChanges();

                                scope.Complete();
                                result.IsSuccess = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
            }

            return result;
        }

        public EResultDto VehicleOutService(EVehicleServiceDto model)
        {
            var result = new EResultDto();
            try
            {
                var debit = _vehicleDebitRepository.FindForInsertUpdateDelete(model.VehicleDebitId.Value);
                result = VehicleActivePassiveControl(debit.VehicleId);
                if (result.IsSuccess)
                {
                    //servisten önceki son zimmetli bilgileri
                    var lastDebit = _vehicleDebitRepository.Where(w => w.VehicleId == debit.VehicleId && w.State != (int)DebitState.InService).OrderByDescending(o => o.Id).FirstOrDefault();
                    var vehicle = _vehicleRepository.FindForInsertUpdateDelete(debit.VehicleId);
                    if (vehicle.LastStatus != (int)DebitState.InService)
                    {
                        result.IsSuccess = false;
                        result.Message = "Araç serviste olmadığından bu işlem yapılamaz.";
                    }
                    else if (lastDebit.State == (int)DebitState.Pool)
                    {
                        result.IsSuccess = false;
                        result.Message = "Araç servisten önce havuzdaysa tekrar <b>havuza</b> alınabilir ya da <b>kişiye</b> zimmetlenebilir";
                    }
                    else
                    {

                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            lastDebit.Id = 0;
                            lastDebit.State = (int)DebitState.Debit;
                            lastDebit.CreatedDate = DateTime.Now;
                            lastDebit.StartDate = DateTime.Now;
                            lastDebit.EndDate = null;
                            _vehicleDebitRepository.Insert(lastDebit);

                            debit.EndDate = DateTime.Now;
                            _vehicleDebitRepository.Update(debit);

                            vehicle.LastStatus = (int)DebitState.Debit;
                            vehicle.LastUserId = lastDebit.DebitUserId;
                            _vehicleRepository.Update(vehicle);

                            _uow.SaveChanges();
                            scope.Complete();
                            result.IsSuccess = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
            }

            return result;
        }



        //public EResultDto VehicleSetService(EVehicleServiceDto model)
        //{
        //    var result = new EResultDto();
        //    try
        //    {
        //        result = VehicleActivePassiveControl(model.VehicleId);
        //        if (result.IsSuccess)
        //        {
        //            if (model.Type == 1 || model.Type == 0)
        //            {
        //                var lastVehicleDebit = _vehicleDebitRepository.Where(w => w.VehicleId == model.VehicleId)
        //                                   .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();

        //                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    bool isDebitInsert = true;
        //                    var vehicle = _vehicleRepository.FindForInsertUpdateDelete(model.VehicleId);
        //                    if (model.Type == 1)//servise al
        //                    {
        //                        if (vehicle.IsService.GetValueOrDefault(false) && model.Id < 0)//IsService=true --> serviste
        //                        {
        //                            result.IsSuccess = false;
        //                            result.Message = "Araç zaten servistedir";
        //                            return result;
        //                        }
        //                        else if (lastVehicleDebit.StartDate >= model.StartDate)
        //                        {
        //                            result.IsSuccess = false;
        //                            result.Message = "Servise alma tarihi,zimmet başlangıç tarihi olan " +
        //                                             lastVehicleDebit.StartDate.ToString("dd/MM/yyy") + " tarihinden sonra olmalıdır";
        //                            return result;
        //                        }

        //                        if (model.Id > 0)//InService edit
        //                        {
        //                            isDebitInsert = false;
        //                            var vehicleDebit = _vehicleDebitRepository.FindForInsertUpdateDelete(model.Id.Value);
        //                            vehicleDebit.StartDate = model.StartDate;
        //                            vehicleDebit.Description = model.Description;
        //                            vehicleDebit.TempPlateNo = model.TempPlateNo.Replace(" ", "");
        //                            _vehicleDebitRepository.Update(vehicleDebit);
        //                        }
        //                        else//InService insert
        //                        {
        //                            vehicle.LastStatus = (int)DebitState.InService;
        //                            vehicle.IsService = true;
        //                        }
        //                    }
        //                    else if (model.Type == 0)//servisten çıkar
        //                    {
        //                        var serviceIn = _vehicleDebitRepository.GetAll().Where(w => w.VehicleId == model.VehicleId && w.State == (int)DebitState.InService).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
        //                        if (model.StartDate < serviceIn.StartDate)
        //                        {
        //                            result.IsSuccess = false;
        //                            result.Message = "Servisten çıkış tarihi, giriş tarihinden sonra olmalıdır.";
        //                            return result;
        //                        }
        //                        if (!vehicle.IsService.GetValueOrDefault(false))
        //                        {
        //                            result.IsSuccess = false;
        //                            result.Message = "Araç zaten servis değildir";
        //                            return result;
        //                        }
        //                        else
        //                        {
        //                            vehicle.LastStatus = (int)DebitState.OutService;
        //                            vehicle.IsService = false;
        //                        }
        //                    }

        //                    if (isDebitInsert)
        //                    {
        //                        //Vehicle last değerler güncelleniyor
        //                        vehicle.LastUserId = null;
        //                        vehicle.LastUnitId = null;
        //                        _vehicleRepository.Update(vehicle);

        //                        //VehicleDebit endDate tarihi güncelleniyor
        //                        lastVehicleDebit.EndDate = model.StartDate;
        //                        _vehicleDebitRepository.Update(lastVehicleDebit);

        //                        var debit = new VehicleDebit()
        //                        {
        //                            CreatedBy = model.CreatedBy,
        //                            StartDate = model.StartDate,
        //                            VehicleId = model.VehicleId,
        //                            TempPlateNo = model.TempPlateNo,
        //                            State = model.Type == 1 ? (int)DebitState.InService : (int)DebitState.OutService,
        //                            Description = model.Description
        //                        };
        //                        _vehicleDebitRepository.Insert(debit);
        //                    }
        //                    _vehicleRepository.Update(vehicle);

        //                    _uow.SaveChanges();
        //                    scope.Complete();
        //                    result.IsSuccess = true;
        //                    result.Message = "Kayıt başarıyla eklenmiştir";
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        result.IsSuccess = false;
        //        result.Message = "Kayıt sırasında hata oluştu!";
        //    }

        //    return result;
        //}

        public EResultDto VehicleDebitDelete(int vehicleDebitId)
        {
            var result = new EResultDto();
            try
            {
                var entity = _vehicleDebitRepository.Find(vehicleDebitId);
                var vehicleId = entity.VehicleId;
                var vehicleEntity = _vehicleRepository.Find(vehicleId);

                result = VehicleActivePassiveControl(vehicleId);
                if (result.IsSuccess)
                {
                    //Silinen zimmet aralığında bakım/onarım var mı?
                    var debitEndDate = entity.EndDate == null ? DateTime.MaxValue : entity.EndDate;
                    var maintenance = _vehicleMaintenanceRepository.Where(w => w.Status == true &&
                            w.VehicleId == vehicleId && entity.StartDate <= w.InvoiceDate && w.InvoiceDate <= debitEndDate)
                        .ToList();
                    if (maintenance.Count > 0)
                    {
                        result.Message = "Bu zimmet aralığında aracın " + maintenance.Count + " adet <b>Bakım/Onarım</b> kaydı bulunduğu için silinemez.";
                        result.IsSuccess = false;
                        return result;
                    }
                    //Silinen zimmet aralığında yakıt var mı?
                    var fuel = _fuelGenericRepository.Where(w => w.Status == true &&
                        w.VehicleId == vehicleId && entity.StartDate <= w.TransactionDate && w.TransactionDate <= debitEndDate).ToList();
                    if (fuel.Count > 0)
                    {
                        result.Message = "Bu zimmet aralığında aracın " + fuel.Count + " adet <b>yakıt</b> kaydı bulunduğu için silinemez.";
                        result.IsSuccess = false;
                        return result;
                    }

                    //if (entity.State == (int)DebitState.InService)//silinen data servis kayıtları
                    //{
                    //    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        //VehicleDebit ilgili satır siliniyor
                    //        _vehicleDebitRepository.Delete(entity);

                    //        if (vehicleEntity.IsService.GetValueOrDefault(false))
                    //            vehicleEntity.IsService = false;
                    //        else
                    //            vehicleEntity.IsService = true;

                    //        _vehicleRepository.Update(vehicleEntity);
                    //        _uow.SaveChanges();
                    //        scope.Complete();
                    //        result.IsSuccess = true;
                    //    }
                    //}
                    //else
                    //{
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //VehicleDebit ilgili satır siliniyor
                        _vehicleDebitRepository.Delete(entity);
                        _uow.SaveChanges();

                        //VehicleDebit son kaydın endDate null yapılıyor
                        var getLastDebit = GetLastDebitUserHistory(vehicleId).Take(1).FirstOrDefault();
                        if (getLastDebit != null)
                        {
                            var debitEntity = _vehicleDebitRepository.Find(getLastDebit.VehicleDebitId);
                            debitEntity.EndDate = null;
                            _vehicleDebitRepository.Update(debitEntity);
                        }

                        //son zimmet hareketinin LastState,lastUserId değeri araç tablosuna yazılıyor 
                        vehicleEntity.LastStatus = getLastDebit == null ? null : getLastDebit.DebitState2;
                        vehicleEntity.LastUnitId = getLastDebit == null ? null : getLastDebit.UnitId;
                        vehicleEntity.LastUserId = getLastDebit == null ? null : getLastDebit.DebitUserId;

                        _vehicleRepository.Update(vehicleEntity);
                        _uow.SaveChanges();
                        scope.Complete();
                        result.IsSuccess = true;
                    }
                    //}
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
            }
            return result;
        }

        public EVehicleDto GetByVehicleIdLastDebit(int vehicleId)//plakaya ait son zimmetli bilgisini listeler
        {
            var result = (from v in _vehicleRepository.GetAll()
                          join u in _userRepository.GetAll() on v.LastUserId equals u.Id into uL
                          from u in uL.DefaultIfEmpty()
                          where v.Id == vehicleId
                          select new EVehicleDto()
                          {
                              ArventoNo = v.ArventoNo,
                              DebitNameSurname = u.Name + " " + u.Surname
                          }).FirstOrDefault();
            return result;
        }
        public async Task<RVehicleFuelDto> GetVehicleDebitRangeByPlate(string plate, DateTime date)//plaka ve tarihe göre zimmetli aracı getir
        {
            var debitList = await _reportService.GetDebitListRange(new RFilterModelDto() { Plate = plate, StartDate = date, EndDate = DateTime.Now });
            var isRange = debitList.FirstOrDefault(w => w.StartDate <= date && date < w.EndDate);
            return isRange ?? null;
        }
        #endregion

        #region VehicleFile Table

        public EVehicleDto GetByVehicleIdFileList(int vehicleId)
        {
            var entityDto = new EVehicleDto();
            var vehicle = _vehicleRepository.Find(vehicleId);
            var files = (from v in _vehicleRepository.GetAll()
                         join vf in _vehicleFileRepository.GetAll() on v.Id equals vf.VehicleId
                         join fu in _fileUploadRepository.GetAll() on vf.FileUploadId equals fu.Id
                         where v.Id == vehicleId
                         select new EFileUploadDto
                         {
                             Id = fu.Id,
                             VehicleId = v.Id,
                             VehicleFileId = vf.Id,
                             Name = fu.Name,
                             Extention = fu.Extention,
                             FileSize = fu.FileSize
                         }).ToList();

            entityDto.files = files;
            entityDto.Status = vehicle.Status;
            return entityDto;
        }

        public EVehicleDto GetByVehicleIdLastLoadImageList(int vehicleId, int typeId)
        {
            var result = new EVehicleDto();

            var lastLoad = _vehiclePhysicalImageGenericRepository.Where(w => w.VehicleId == vehicleId && w.TypeId == typeId)
                .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
            result.files = null;

            if (lastLoad != null)
            {
                var files = (from vf in _vehiclePhysicalImageFileGenericRepository.GetAll()
                             join fu in _fileUploadRepository.GetAll() on vf.FileUploadId equals fu.Id
                             where vf.VehiclePhysicalImageId == lastLoad.Id
                             select new EFileUploadDto
                             {
                                 Id = fu.Id,
                                 VehicleId = vehicleId,
                                 VehicleFileId = vf.Id,
                                 Name = fu.Name,
                                 Extention = fu.Extention,
                                 FileSize = fu.FileSize
                             }).ToList();

                result.files = files;
                result.CreatedDate = lastLoad.CreatedDate;
                var user = _userRepository.Find(lastLoad.CreatedBy);
                result.DebitNameSurname = user.Name + " " + user.Surname;
                result.VehiclePhysicalImageId = lastLoad.Id;
            }

            return result;
        }


        public EResultDto VehicleDelete(IList<IFormFile> files, EVehicleTransferFileDto IncomingModel)
        {
            var result = new EResultDto();
            try
            {
                var model = _vehicleRepository.Find(IncomingModel.VehicleId);
                if (model != null && model.Status)
                {
                    var licence = _licenceService.DeleteVehicle(IncomingModel.DeleteUserId, JsonConvert.SerializeObject(IncomingModel));//Lisans araç silme
                    if (licence.Result.StatusCode == System.Net.HttpStatusCode.OK && licence.Result.Success)
                    {
                        //zimmetli sona erme tarihinden sonra bakım/onarım varmı ?
                        var maintenance = _vehicleMaintenanceRepository.Where(w => w.Status &&
                            w.VehicleId == IncomingModel.VehicleId &&
                            w.InvoiceDate >= IncomingModel.DebitVehicleEndDate)
                        .FirstOrDefault();
                        if (maintenance != null)
                        {
                            result.Message = maintenance.InvoiceDate.ToString("MM/dd/yyyy") + " tarihinde aracın <b>Bakım/Onarım</b> kaydı bulunmaktadır. Bu tarihten sonra silebilirsiniz";
                            result.IsSuccess = false;
                            return result;
                        }
                        //zimmetli sona erme tarihinden sonra yakıt varmı ?
                        var fuel = _fuelGenericRepository.Where(w => w.Status &&
                            w.VehicleId == IncomingModel.VehicleId &&
                            w.TransactionDate >= IncomingModel.DebitVehicleEndDate).FirstOrDefault();
                        if (fuel != null)
                        {
                            result.Message = fuel.TransactionDate.ToString("MM/dd/yyyy") + " tarihinde aracın <b>yakıt</b> kaydı bulunmaktadır. Bu tarihten sonra silebilirsiniz";
                            result.IsSuccess = false;
                            return result;
                        }

                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //VehicleDebit set EndDate
                            var vehicleDebit = _vehicleDebitRepository.Where(w => w.VehicleId == IncomingModel.VehicleId)
                                .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                            if (vehicleDebit == null)
                            {
                                result.Message = "Araç üzerine daha önce hiç zimmet atanmamış, atayıp öyle silmeyi deneyin";
                                result.IsSuccess = false;
                                return result;
                            }

                            if (vehicleDebit.EndDate == null)
                            {
                                if (IncomingModel.DebitVehicleEndDate != DateTime.MinValue &&
                                    vehicleDebit.StartDate >= IncomingModel.DebitVehicleEndDate)
                                {
                                    result.Message =
                                        "Zimmetli sona erme tarihi başlangıç tarihinden büyük olmalıdır. Başlangıç tarihi: <b>" +
                                        vehicleDebit.StartDate.ToString("dd/MM/yyy") + "</b>";
                                    result.IsSuccess = false;
                                    return result;
                                }

                                vehicleDebit.EndDate = IncomingModel.DebitVehicleEndDate;
                                var deleteDebitEntity = new VehicleDebit
                                {
                                    StartDate = DateTime.Now,
                                    CreatedBy = IncomingModel.DeleteUserId,
                                    VehicleId = vehicleDebit.VehicleId,
                                    State = (int)DebitState.Deleted
                                };
                                _vehicleDebitRepository.Insert(deleteDebitEntity);
                                _vehicleDebitRepository.Update(vehicleDebit);
                            }

                            //VehicleTransferLog table insert
                            var vehicleTransferLogId = VehicleTransferLogInsert(IncomingModel);

                            if (files.Count > 0)
                            {
                                var fs = new FileService(_uow, _env);
                                result = fs.FileUploadInsertLogistics(files);
                                if (!result.IsSuccess)
                                {
                                    result.IsSuccess = false;
                                    result.Message = "Dosya yüklemede hata oluştu";
                                    fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/logistics/");
                                    return result;
                                }
                                //VehicleTransferFile insert
                                foreach (var item in result.Ids)
                                {
                                    var vehicleTransferFileEnt = _vehicleTransferFileRepository.Insert(new VehicleTransferFile
                                    {
                                        FileUploadId = item,
                                        VehicleTransferId = vehicleTransferLogId
                                    });
                                }
                            }

                            //Vehicle update
                            model.Status = Convert.ToBoolean(Status.Passive);
                            model.LastUserId = null;
                            model.LastUnitId = null;
                            model.ArventoNo = null;
                            model.LastStatus = (int)DebitState.Deleted;
                            _vehicleRepository.Update(model);

                            _uow.SaveChanges();
                            scope.Complete();
                            result.Message = "İşlem başarılı";
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = licence.Result.Data;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Araç zaten silinmiş";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }

            return result;
        }


        public EResultDto VehicleFileInsert(IList<IFormFile> files, int vehicleId)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                result = VehicleActivePassiveControl(vehicleId);
                if (result.IsSuccess)
                {
                    result = fs.FileUploadInsertLogistics(files);
                    if (result.IsSuccess)
                    {
                        foreach (var item in result.Ids)
                        {
                            var entity = _vehicleFileRepository.Insert(new VehicleFile
                            {
                                FileUploadId = item,
                                VehicleId = vehicleId
                            });
                        }
                        _uow.SaveChanges();
                        result.IsSuccess = true;
                        result.Id = vehicleId;
                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Dosya yüklemede hata oluştu";
                    }
                }
            }
            catch (Exception)
            {
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/logistics/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }


        public EResultDto VehiclePhotographyInsert(EVehiclePhysicalImageLoadDto model)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {

                    var fileName = model.VehicleId + "_" + model.CreatedBy + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_Web";

                    result = fs.FileUploadInsertVehiclePhysicalImage(model.files, fileName);

                    if (result.IsSuccess)
                    {
                        //Aracın son fotoğrafları mobilden/web'den yüklenince yeni kayıt açarak devam eder. Diğer tiplerinde son kayıt üzerine update geçer


                        var lastRecord = _vehiclePhysicalImageGenericRepository
                             .Where(f => f.Status && f.VehicleId == model.VehicleId && f.TypeId == model.TypeId)
                             .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();

                        var physicalId = 0;
                        if (model.TypeId == (int)TypeList.LastVehicleImage || lastRecord == null)
                        {
                            var imageRep = _vehiclePhysicalImageGenericRepository.Insert(new VehiclePhysicalImage()
                            {
                                VehicleId = model.VehicleId,
                                CreatedBy = model.CreatedBy,
                                TypeId = model.TypeId
                            });
                            _uow.SaveChanges();
                            physicalId = imageRep.Id;
                        }
                        else if (lastRecord != null)
                            physicalId = lastRecord.Id;

                        foreach (var item in result.Ids)
                        {
                            _vehiclePhysicalImageFileGenericRepository.Insert(new VehiclePhysicalImageFile()
                            {
                                FileUploadId = item,
                                VehiclePhysicalImageId = physicalId
                            });
                        }

                        _uow.SaveChanges();
                        result.IsSuccess = true;
                        result.Id = model.VehicleId;
                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Dosya yüklemede hata oluştu";
                    }
                }
            }
            catch (Exception)
            {
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/physicalimage/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }


        public int VehicleTransferLogInsert(EVehicleTransferFileDto IncomingModel)
        {
            var entity = _mapper.Map<VehicleTransferLog>(IncomingModel);
            entity.CreatedBy = IncomingModel.DeleteUserId;
            var result = _vehicleTransferLogRepository.Insert(_mapper.Map<VehicleTransferLog>(entity));
            _uow.SaveChanges();
            return result.Id;
        }

        #endregion

        #region VehicleMaterial Table

        public EResultDto InsertVehicleMaterial(int vehicleId, int[] materials, int createdBy)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(vehicleId);
                if (result.IsSuccess)
                {
                    //Eski kayıtları silip yenisi varsa ekliyoruz
                    var oldEntity = _vehicleMaterialRepository.Where(w => w.VehicleId == vehicleId);
                    if (oldEntity.Count() > 0)
                        _vehicleMaterialRepository.DeleteRange(oldEntity);

                    foreach (var item in materials)
                        _vehicleMaterialRepository.Insert(new VehicleMaterial
                        {
                            CreatedBy = createdBy,
                            TypeId = item,
                            VehicleId = vehicleId
                        });

                    _uow.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }

            return result;
        }

        //Şimdilik sürekli aktif edildi
        public EResultDto VehicleActivePassiveControl(int vehicleId)
        {
            var result = new EResultDto();
            var vehicle = _vehicleRepository.Find(vehicleId);
            if (!vehicle.Status)
            {
                result.IsSuccess = false;
                result.Message = "Pasif araç üzerinden işlem yapılamaz";
            }
            return result;
        }

        public int[] GetByIdVehicleMaterial(int vehicleId)
        {
            return _vehicleMaterialRepository.GetAll().Where(w => w.VehicleId == vehicleId).Select(s => s.TypeId)
                .ToArray();
        }

        #endregion

        #region VehicleAmount Table

        public EResultDto InsertVehicleAmount(VehicleAmount model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var contracts = _vehicleContractRepository
                        .Where(w => w.VehicleId == model.VehicleId)
                        .OrderByDescending(o => o.EndDate).ToList();

                    //Kayıtlı sözleşme tarihleri yoksa tutar giremez
                    if (contracts.Count() == 0)
                    {
                        result.IsSuccess = false;
                        result.Message = "Sözleşme tarihleri girmeden tutar girilemez";
                        return result;
                    }

                    //Girilen tutar sözleşme tarihleri arasında olmalıdır
                    foreach (var item in contracts)
                    {
                        if (item.StartDate <= model.StartDate && model.StartDate < item.EndDate)
                        {
                            if (model.TypeId == (int)VehicleAmountType.ExtraTutar)
                                model.EndDate = model.StartDate;
                            else
                                model.EndDate = item.EndDate;//Default olarak amount endDate tarihi sözleşme bitiş tarihi olarak veriliyor
                            model.VehicleContractId = item.Id;
                            var lastRecord = _vehicleAmountRepository
                                .Where(w => w.VehicleId == model.VehicleId && item.StartDate <= w.StartDate && w.StartDate < item.EndDate && w.TypeId == model.TypeId)
                                .OrderByDescending(o => o.Id).FirstOrDefault();

                            if (lastRecord == null)//İlk kayıt
                            {
                                var entity = _vehicleAmountRepository.Insert(model);
                                _uow.SaveChanges();
                                result.Id = entity.Id;
                                result.Message = "Kayıt başarıyla eklenmiştir";
                            }
                            else if (model.TypeId == (int)VehicleAmountType.ExtraTutar)
                            {
                                var entity = _vehicleAmountRepository.Insert(model);
                                _uow.SaveChanges();
                            }
                            else if (lastRecord.StartDate < model.StartDate && model.StartDate < lastRecord.EndDate)
                            {
                                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //eski kaydın endDate tarihi güncelleniyor
                                    lastRecord.EndDate = model.StartDate;
                                    _vehicleAmountRepository.Update(lastRecord);

                                    var entity = _vehicleAmountRepository.Insert(model);
                                    _uow.SaveChanges();

                                    scope.Complete();
                                    result.Id = entity.Id;
                                    result.Message = "Kayıt başarıyla eklenmiştir";
                                }
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = "Girilen tutar <b style='color:orange;'>" + lastRecord.StartDate.ToString("dd/MM/yyyy") + "(Dahil)-" + lastRecord.EndDate.Value.ToString("dd/MM/yyyy") +
                                                 "(Hariç)</b> tarihleri arasında olmalıdır";
                            }

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = "Girilen tutar <b style='color:orange;'>" + item.StartDate.ToString("dd/MM/yyyy") + "(Dahil)-" + item.EndDate.ToString("dd/MM/yyyy") +
                                              "(Hariç)</b> tarihleri arasında olmalıdır";
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }

            return result;
        }

        public EVehicleContractWithAmountDto GetByVehicleIdVehicleAmountHistory(int vehicleId, int vehicleAmountTypeId, bool isAdmin = false)
        {
            //var vehicle = _vehicleRepository.Find(vehicleId);

            //if (vehicle.FixtureTypeId == (int)FixtureType.ForRent) //Kiralık araç-Sözleşme tarihleri olması gerekiyor
            return GetForRentContractAndAmount(vehicleId, vehicleAmountTypeId, isAdmin);
            //return GetForFixContractAndAMount(vehicleId, vehicleAmountTypeId);
        }

        public EVehicleContractWithAmountDto GetForRentContractAndAmount(int vehicleId, int vehicleAmountTypeId, bool isAdmin)
        {
            var contractLastData = _vehicleContractRepository
                .Where(w => w.VehicleId == vehicleId)
                .OrderByDescending(o => o.EndDate)
                .Take(1)
                .FirstOrDefault();

            if (contractLastData != null)
            {
                var list = (from va in _vehicleAmountRepository.GetAll()
                            join l in _lookUpListRepository.GetAll() on va.TypeId equals l.Id
                            where va.VehicleId == vehicleId && contractLastData.StartDate <= va.StartDate &&
                                  va.StartDate <= contractLastData.EndDate
                            select new EVehicleAmountDto
                            {
                                Id = va.Id,
                                VehicleId = va.VehicleId,
                                VehicleContractId = contractLastData.Id,
                                ContractStartDate = contractLastData.StartDate,
                                ContractEndDate = contractLastData.EndDate,
                                StartDate = va.StartDate,
                                EndDate = va.EndDate == null ? contractLastData.EndDate : va.EndDate,
                                AmountExpense = va.Amount,
                                AmountIncome = va.AmountIncome ?? 0,
                                VehicleAmountTypeName = l.Name,
                                ExtraAmount = va.ExtraAmount,
                                Description = va.Description,
                                TypeId = l.Id,
                                CustomButton = ""
                            });

                if (vehicleAmountTypeId > 0)
                    list = list.Where(w => w.TypeId == vehicleAmountTypeId);

                //Delete button add
                var amountList = list.ToList();
                if (isAdmin)
                {
                    var typeGroupLastRecord = amountList.GroupBy(g => g.TypeId).Select(s => s.OrderByDescending(o => o.EndDate).FirstOrDefault());//Birden fazla ödeme türü varsa son kayda delete butonu ekle
                    foreach (var amount in amountList)
                    {
                        if (typeGroupLastRecord.Any(w => w.Id == amount.Id))
                            amount.CustomButton = "<a data-toggle='modal' onclick='funcDeleteVehicleAmount(" +
                                                  amount.VehicleContractId + "," + amount.Id +
                                                  ")'><i class='icon-trash text-danger'></i></a>";
                    }
                }

                var result = new EVehicleContractWithAmountDto
                {
                    StartDate = contractLastData.StartDate,
                    EndDate = contractLastData.EndDate,
                    VehicleAmountList = TotalAmountCalc(amountList).OrderByDescending(o => o.VehicleAmountTypeName).ThenByDescending(t => t.StartDate).ToList()
                };
                return result;
            }
            return null;
        }

        public EVehicleContractWithAmountDto GetForFixContractAndAMount(int vehicleId, int vehicleAmountTypeId)
        {
            var list = from va in _vehicleAmountRepository.GetAll()
                       join l in _lookUpListRepository.GetAll() on va.TypeId equals l.Id
                       where va.VehicleId == vehicleId
                       select new EVehicleAmountDto
                       {
                           StartDate = va.StartDate,
                           AmountExpense = va.Amount,
                           VehicleAmountTypeName = l.Name,
                           ExtraAmount = va.ExtraAmount,
                           Description = va.Description,
                           TypeId = l.Id
                       };

            if (vehicleAmountTypeId > 0)
                list = list.Where(w => w.TypeId == vehicleAmountTypeId);

            return new EVehicleContractWithAmountDto
            {
                VehicleAmountList = list.OrderByDescending(o => o.StartDate).ToList()
            };
        }

        #endregion

        #region VehicleContract

        public EResultDto InsertVehicleContract(VehicleContract model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var firstDebit = _vehicleDebitRepository.Where(w => w.Status && w.VehicleId == model.VehicleId).OrderBy(o => o.Id).FirstOrDefault();
                    if (firstDebit == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "Aracın zimmet kaydı bulunmamaktadır. Zimmet yapıp sonra ekleyiniz.";
                        return result;
                    }

                    if (model.StartDate < firstDebit.StartDate)
                    {
                        result.IsSuccess = false;
                        result.Message = $"Aracın ilk zimmet tarihi {firstDebit.StartDate.ToString("dd/MM/yyyy")}. Bu tarihe eşit veya sonraki bir gün giriniz";
                        return result;
                    }

                    //Son kayıt çekiliyor,
                    var lastEntity = _vehicleContractRepository.Where(w => w.VehicleId == model.VehicleId && w.Status).OrderByDescending(o => o.StartDate).FirstOrDefault();
                    if (lastEntity != null && lastEntity.EndDate > model.StartDate)
                    {
                        result.IsSuccess = false;
                        result.Message = "Başlangıç tarihi, bir önceki sözleşme bitiş tarihinden büyük veya eşit olmalıdır";
                    }
                    else if (model.StartDate >= model.EndDate)
                    {
                        result.IsSuccess = false;
                        result.Message = "Bitiş tarihi, başlangıç tarihinden büyük olmalıdır";
                    }
                    else
                    {
                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (lastEntity != null)
                            {
                                var vehicle = _vehicleRepository.Find(model.VehicleId);
                                lastEntity.LastKm = vehicle.LastKm;
                                _vehicleContractRepository.Update(lastEntity);
                            }

                            var entity = _vehicleContractRepository.Insert(model);
                            _uow.SaveChanges();
                            scope.Complete();

                            result.Id = entity.Id;
                            result.Message = "Sözleşme bilgileri başarıyla eklenmiştir";
                        }
                    }
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }

        public EResultDto UpdateVehicleContract(VehicleContract model)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(model.VehicleId);
                if (result.IsSuccess)
                {
                    var firstDebit = _vehicleDebitRepository.Where(w => w.Status && w.VehicleId == model.VehicleId).OrderBy(o => o.Id).FirstOrDefault();

                    if (model.StartDate < firstDebit.StartDate)
                    {
                        result.IsSuccess = false;
                        result.Message = $"Aracın ilk zimmet tarihi {firstDebit.StartDate.ToString("dd/MM/yyyy")}. Bu tarihe eşit veya sonraki bir gün giriniz";
                        return result;
                    }


                    if (model.StartDate >= model.EndDate)
                    {
                        result.IsSuccess = false;
                        result.Message = "Sözleşme bitiş tarihi,başlangıç tarihinden büyük olmalıdır";
                    }
                    else
                    {
                        var contract = _vehicleContractRepository.FindForInsertUpdateDelete(model.Id);
                        if (contract != null)
                        {
                            var allContract = _vehicleContractRepository.Where(w => w.Status && w.VehicleId == model.VehicleId)
                                .OrderByDescending(o => o.Id).ToList();
                            if (allContract[0].Id == contract.Id)
                            {
                                var contractAmountList = _vehicleAmountRepository.Where(w =>
                                    w.VehicleContractId == model.Id && w.VehicleId == model.VehicleId).ToList();
                                if (contractAmountList.Count > 0)
                                {
                                    result.Message = "Sözleşme aralığına tutar bilgileri girilmiş, silip tekrar deneyiniz";
                                    result.IsSuccess = false;
                                }
                                else if (contractAmountList.Any(a => a.StartDate >= model.StartDate && a.EndDate < model.EndDate))
                                {
                                    result.Message = "Tarihler, sözleşme aralığında bulunan tutar tarihlerinden sonra olmalıdır.";
                                    result.IsSuccess = false;
                                }
                                else
                                {
                                    contract.StartDate = model.StartDate;
                                    contract.EndDate = model.EndDate;
                                    contract.FirstKm = model.FirstKm;
                                    contract.MaxKmLimit = model.MaxKmLimit;
                                    contract.CreatedBy = contract.CreatedBy;
                                    contract.CreatedDate = DateTime.Now;
                                    _vehicleContractRepository.Update(contract);
                                    _uow.SaveChanges();
                                    result.Message = "Düzenleme işlemi başarılı";
                                }
                            }
                            else
                            {
                                result.Message = "Son sözleşme tarihleri üzerinde değişiklik yapılabilir.";
                                result.IsSuccess = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }

        public EResultDto DeleteVehicleContract(int vehicleId)
        {
            var result = new EResultDto();
            try
            {
                result = VehicleActivePassiveControl(vehicleId);
                if (result.IsSuccess)
                {
                    var lastContract = _vehicleContractRepository.Where(w => w.Status && w.VehicleId == vehicleId).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                    if (lastContract != null)
                    {
                        var contractAmountList = _vehicleAmountRepository.Where(w => w.VehicleContractId == lastContract.Id && w.VehicleId == vehicleId).ToList();
                        if (contractAmountList.Count > 0)
                        {
                            result.Message = "Sözleşme aralığına tutar bilgileri girilmiş, silip tekrar deneyiniz";
                            result.IsSuccess = false;
                        }
                        else
                        {
                            lastContract.Status = false;
                            _vehicleContractRepository.Update(lastContract);
                            _uow.SaveChanges();

                            lastContract = _vehicleContractRepository.Where(w => w.Status && w.VehicleId == vehicleId).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                            if (lastContract != null)
                            {
                                lastContract.LastKm = null;
                                _vehicleContractRepository.Update(lastContract);
                                _uow.SaveChanges();
                            }

                            result.Message = "Silme işlemi başarılı";
                        }
                    }
                    else
                    {
                        result.Message = "Araca ait sözleşme bulunamadı";
                        result.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }

        public List<EVehicleContractWithAmountDto> GetByIdVehicleIdContractDateAndAmount(int vehicleId)
        {
            var result = new List<EVehicleContractWithAmountDto>();
            var list = _vehicleContractRepository.Where(w => w.Status && w.VehicleId == vehicleId).OrderByDescending(o => o.EndDate).ToList();
            foreach (var item in list)
            {
                var startDate = item.StartDate;
                var endDate = item.EndDate;
                var firstKm = item.FirstKm;
                var maxKmLimit = item.MaxKmLimit;
                //Contract date arasındaki yapılan masraflar
                var amountList = (from v in _vehicleAmountRepository.GetAll()
                                  join l in _lookUpListRepository.GetAll() on v.TypeId equals l.Id
                                  where v.Status && v.VehicleId == vehicleId && startDate <= v.StartDate && v.StartDate < endDate
                                  select new EVehicleAmountDto
                                  {
                                      Id = v.Id,
                                      VehicleId = v.VehicleId,
                                      VehicleContractId = item.Id,
                                      StartDate = v.StartDate,
                                      EndDate = v.EndDate == null ? DateTime.MinValue : v.EndDate,
                                      AmountExpense = v.Amount,
                                      AmountIncome = v.AmountIncome ?? 0,
                                      ExtraAmount = v.ExtraAmount,
                                      TypeId = v.TypeId,
                                      Description = v.Description,
                                      VehicleAmountTypeName = l.Name,
                                  }).ToList();

                amountList = TotalAmountCalc(amountList);
                var temp = new EVehicleContractWithAmountDto
                {
                    Id = item.Id,
                    FirstKm = firstKm,
                    MaxKmLimit = maxKmLimit,
                    StartDate = startDate,
                    EndDate = endDate,
                    VehicleAmountList = amountList.OrderByDescending(o => o.VehicleAmountTypeName).ThenByDescending(t => t.StartDate).ToList()
                };
                result.Add(temp);
            }

            if (result.Any())
                result[0].VehicleLastKm = _vehicleRepository.Find(vehicleId).LastKm;
            return result;
        }

        public EResultDto DeleteVehicleAmount(int vehicleContractId, int vehicleAmountId)
        {
            var result = new EResultDto();
            try
            {
                var vehicleId = _vehicleAmountRepository.Find(vehicleAmountId).VehicleId;
                result = VehicleActivePassiveControl(vehicleId);
                if (result.IsSuccess)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var contractAmountList = _vehicleAmountRepository.Where(w => w.VehicleContractId == vehicleContractId);//sözleşme altındaki tüm tutarlar
                        var amount = contractAmountList.FirstOrDefault(w => w.Id == vehicleAmountId);//silinecek olan tutar

                        //Birden fazla kira bedeli olabilir, start-end date tarihleri güncelleniyor
                        if (amount.TypeId == (int)VehicleAmountType.KiraBedeli)
                        {
                            var contract = _vehicleContractRepository.Find(vehicleContractId);
                            var amountTypeList = contractAmountList.Where(w => w.TypeId == amount.TypeId).OrderByDescending(o => o.Id).ToList();//o tipteki tüm fiyatları listeler
                            if (amountTypeList.Count() == 1)//tek kayıtsa direkt sil
                                _vehicleAmountRepository.Delete(amountTypeList[0]);
                            else if (amountTypeList.Count() > 1)//aynı türde birden fazla kayıt var son kayıt silinecek,bir önceki kaydın EndDate tarihi güncellenecek
                            {
                                //last record delete
                                var amountLastRecord = amountTypeList[0];
                                _vehicleAmountRepository.Delete(amountLastRecord);

                                //lastRecord silindikten sonraki kaydın bitiş tarihini söz.bitiş tarihi olarak güncelle
                                amountTypeList[1].EndDate = contract.EndDate;
                                _vehicleAmountRepository.Update(amountTypeList[1]);
                            }
                        }
                        else//tek seferlik giri
                            _vehicleAmountRepository.Delete(amount);

                        _uow.SaveChanges();
                        scope.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "İşlem sırasında hata oluştu";
            }

            return result;
        }

        public List<EVehicleAmountDto> TotalAmountCalc(List<EVehicleAmountDto> list/*, DateTime start, DateTime end, decimal amount, int costTypeId*/)
        {
            if (list.Any())
            {
                int vehicleId = list[0].VehicleId;
                var vehicle = _vehicleRepository.Find(vehicleId);
                var result = new List<EVehicleAmountDto>();
                var isRangeDeleteDate = false;
                var lastDebit = new VehicleDebit();
                if (!vehicle.Status)
                    lastDebit = _vehicleDebitRepository.Where(w => w.VehicleId == vehicleId && w.State != (int)DebitState.Deleted).OrderByDescending(o => o.Id).FirstOrDefault();

                list = list.OrderByDescending(o => o.Id).ToList();

                foreach (var item in list)
                {
                    var startDate = item.StartDate;
                    var endDate = item.EndDate;
                    if (endDate != null)
                    {
                        if (!vehicle.Status)//araç silinmişse, endDate'i silinme tarihi olarak ayarla
                        {
                            if (lastDebit != null && item.StartDate <= lastDebit.EndDate && lastDebit.EndDate < item.EndDate)
                            {
                                var deleteInfo = _vehicleTransferLogRepository.FirstOrDefault(f => f.VehicleId == vehicleId);
                                endDate = lastDebit.EndDate;
                                item.DeleteVehicleInfo = VehicleDeleteInfo(vehicleId);
                                item.DeleteVehicleInfo += " (" + lastDebit.EndDate.Value.ToString("dd-MM-yyyy") + ")</span> <i class='icon-info22 faa-flash animated faa-slow cursor-pointer' title='Söz.Bitiş Tarihi bu tarihe göre hesaplanır'></i>";
                                isRangeDeleteDate = true;
                            }
                            else if (item.StartDate > lastDebit.EndDate)
                            {
                                item.DeleteVehicleInfo = VehicleDeleteInfo(vehicleId);
                                item.DeleteVehicleInfo += " (" + lastDebit.EndDate.Value.ToString("dd-MM-yyyy") + ")</span> <i class='icon-info22 faa-flash animated faa-slow cursor-pointer' title='Söz.Bitiş Tarihi bu tarihe göre hesaplanır'></i>";
                                continue;
                            }
                        }

                        var dateNow = DateTime.Now.Date;
                        if (item.TypeId == (int)VehicleAmountType.ExtraTutar)
                        {
                            item.AllTotalExpense = item.AmountExpense;//extra tutar tek seferlik
                            if (startDate <= ((isRangeDeleteDate == true && dateNow > endDate.Value) ? endDate.Value : dateNow))//extra tutar bugünden önceyse al
                                item.TotalTodayExpense = item.AmountExpense;
                        }
                        else
                        {
                            //Sözleşme tarihleri arasındaki toplam tutar
                            var tempTotalExpense = SubTotalCost(endDate.Value, startDate, item.AmountExpense); //top. gider
                            item.AllTotalExpense = tempTotalExpense;

                            var tempTotalIncome = SubTotalCost(endDate.Value, startDate, item.AmountIncome); //top. gelir
                            item.AllTotalIncome = tempTotalIncome;

                            //Bugüne kadar ki toplam tutar
                            if (startDate <= dateNow && dateNow < endDate)
                            {
                                item.TotalTodayExpense = SubTotalCost(((isRangeDeleteDate == true && dateNow > endDate.Value) ? endDate.Value : dateNow), startDate, item.AmountExpense);
                                item.TotalTodayIncome = SubTotalCost(((isRangeDeleteDate == true && dateNow > endDate.Value) ? endDate.Value : dateNow), startDate, item.AmountIncome);
                            }
                            else if (dateNow < startDate)
                            {
                                item.TotalTodayExpense = 0;
                                item.TotalTodayIncome = 0;
                            }
                            else
                            {
                                item.TotalTodayExpense = tempTotalExpense;
                                item.TotalTodayIncome = tempTotalIncome;
                            }
                        }
                    }
                }
            }
            return list;
        }

        public string VehicleDeleteInfo(int vehicleId)
        {
            var result = "";
            var deleteInfo = (from v in _vehicleTransferLogRepository.GetAll()
                              join l in _lookUpListRepository.GetAll() on v.TransferTypeId equals l.Id
                              where v.VehicleId == vehicleId
                              select new EUnitDto() { StatusName = "<span class='label bg-danger'>" + l.Name }).FirstOrDefault();
            if (deleteInfo != null) result = deleteInfo.StatusName;
            return result;
        }

        public decimal SubTotalCost(DateTime end, DateTime start, decimal amount)
        {
            if (amount == 0) return 0;
            var startDateTotal = (decimal)0;//ilk ay 1'den başlamıyorsa o aya bölüp bul
            var totalAmount = (decimal)0;
            //if (start.Day > 1)
            //{
            //    int daysInMonthStart = DateTime.DaysInMonth(start.Year, start.Month);
            //    startDateTotal = (amount / daysInMonthStart) * (daysInMonthStart - start.Day + 1);
            //    start = new DateTime(start.AddMonths(1).Year, start.AddMonths(1).Month, 1);
            //}

            if (start < end)
            {
                var dateSpan = DateDiff.DateTimeSpan.CompareDates(end, start);
                var years = dateSpan.Years;
                var months = dateSpan.Months;
                var days = dateSpan.Days;
                int daysInMonth = 30;//DateTime.DaysInMonth(end.Year, end.Month);

                totalAmount = (((years * 12) + months) * amount) + ((amount / daysInMonth) * days);
            }
            return totalAmount + startDateTotal;
        }

        #endregion

        #region Notification (Home page)
        //Anasayfa bildirimler
        public async Task<ENotificationDto> GetNotificationMessages(RFilterModelDto filterModel)
        {
            var dateNow = DateTime.Now.Date;
            var add1Month = dateNow.AddDays(30);
            var plates = await Task.FromResult((from v in _vehicleRepository.GetAll()
                                                join un in _unitRepository.GetAll() on v.LastUnitId equals un.Id into unL
                                                from un in unL.DefaultIfEmpty()
                                                join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                                                from un1 in un1L.DefaultIfEmpty()
                                                join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                                                from vr in vrL.DefaultIfEmpty()
                                                join vc in _vehicleContractRepository.GetAll() on v.Id equals vc.VehicleId into vcL
                                                from vc in vcL.DefaultIfEmpty()
                                                where vc.Status && v.Status && vc.EndDate <= add1Month
                                                select new EVehicleDto()
                                                {
                                                    DebitEndDate = vc.EndDate,
                                                    Plate = v.Plate,
                                                    VehicleId = v.Id,
                                                    UnitId = un.Id,
                                                    ParentUnitId = un1.Id,
                                                }));

            if (filterModel.ParentUnitId > 0)
                plates = plates.Where(w => w.ParentUnitId == filterModel.ParentUnitId);
            else if (filterModel.UnitId > 0)
                plates = plates.Where(w => w.UnitId == filterModel.UnitId);

            int countUpContract = 0, countDownContract = 0, timeUpDocumentACar = 0;
            var contractPlateList = plates.ToList().DistinctBy(d => d.VehicleId).ToList();
            foreach (var item in contractPlateList) //Sözleşme süreleri
            {
                var lastRecord = (await _vehicleContractRepository.WhereAsync(w => w.Status && w.VehicleId == item.VehicleId)).OrderByDescending(o => o.EndDate).Take(1).FirstOrDefault();
                if (lastRecord != null)
                {
                    if (lastRecord.EndDate < dateNow)
                        countUpContract++;
                    else if (dateNow <= lastRecord.EndDate && lastRecord.EndDate <= add1Month)
                        countDownContract++;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------//

            dateNow = dateNow.AddDays(10).Date;
            var documentPlateList = await Task.FromResult(from ed in _vehicleExaminationDateRepository.GetAll()
                                                          join v in _vehicleRepository.GetAll() on ed.VehicleId equals v.Id
                                                          where v.Status && ed.Status &&
                                                          (ed.KDocumentEndDate <= dateNow || ed.ExaminationEndDate <= dateNow || ed.KaskoEndDate <= dateNow || ed.TrafficEndDate <= dateNow)
                                                          select new EVehicleDto()
                                                          {
                                                              VehicleId = v.Id
                                                          });

            var documentlist = documentPlateList.ToList().DistinctBy(d => d.VehicleId).ToList();
            foreach (var item2 in documentlist)//Trafik kasko süreleri
            {
                if (plates.Count(w => w.VehicleId == item2.VehicleId) > 0)
                    timeUpDocumentACar++;
            }

            return new ENotificationDto()
            {
                TimeUpRentACar = countUpContract, //Süresi biten
                TimeDownRentACar = countDownContract,//Süresi bitmeye -- gün kalan
                TimeUpDocumentACar = documentPlateList.Count()
            };
        }
        //sözleşme süresi biten araç listesi
        public async Task<List<RVehicleCostDto>> GetTimeUpContractVehicle(RFilterModelDto filterModel)
        {
            var dateNow = DateTime.Now.Date;
            var add1Month = dateNow.AddDays(filterModel.DayCount);
            var result = new List<RVehicleCostDto>();
            var plates = await Task.FromResult((from vc in _vehicleContractRepository.GetAll()
                                                join v in _vehicleRepository.GetAll() on vc.VehicleId equals v.Id
                                                join un in _unitRepository.GetAll() on v.LastUnitId equals un.Id into unL
                                                from un in unL.DefaultIfEmpty()
                                                join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                                                from un1 in un1L.DefaultIfEmpty()
                                                join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                                                from vr in vrL.DefaultIfEmpty()
                                                join rt in _lookUpListRepository.GetAll() on vr.RentTypeId equals rt.Id into rtL
                                                from rt in rtL.DefaultIfEmpty()
                                                join lr in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lr.Id into lrL
                                                from lr in lrL.DefaultIfEmpty()
                                                where v.Status && vc.Status /*&& dateNow <= vc.EndDate*/ && vc.EndDate <= add1Month
                                                select new RVehicleCostDto()
                                                {
                                                    Id = vc.Id,
                                                    Plate = v.Plate,
                                                    VehicleId = v.Id,
                                                    UnitId = un.Id,
                                                    VehicleTypeName = v.FixtureTypeId == (int)FixtureType.Ownership ? "Mülkiyet" : "Kiralık",
                                                    ParentUnitId = un1.Id,
                                                    StartDate = vc.StartDate,
                                                    //DayCount = (dateNow - vc.EndDate).Days,
                                                    EndDate = vc.EndDate,
                                                    RentFirmName = lr.Name ?? "--",
                                                    RentTypeName = rt.Name ?? "--",
                                                }));

            if (filterModel.DayCount > 0)
                plates = plates.Where(w => dateNow <= w.EndDate && w.EndDate <= add1Month);

            if (filterModel.ParentUnitId > 0)
                plates = plates.Where(w => w.ParentUnitId == filterModel.ParentUnitId);
            else if (filterModel.UnitId > 0)
                plates = plates.Where(w => w.UnitId == filterModel.UnitId);

            var plateList = plates.DistinctBy(d => d.VehicleId).ToList();

            foreach (var item in plateList)
            {
                var oneNote = await _oneNoteGenericRepository.AnyAsync(w => w.Status && w.VehicleId == item.VehicleId && w.Status && w.Type != 3);
                if (filterModel.DayCount == 0)
                {
                    var lastRecord = (from v in _vehicleRepository.GetAll()
                                      join vc in _vehicleContractRepository.GetAll() on v.Id equals vc.VehicleId
                                      join vr in _vehicleRentRepository.GetAll() on v.Id equals vr.VehicleId into vrL
                                      from vr in vrL.DefaultIfEmpty()
                                      join rt in _lookUpListRepository.GetAll() on vr.RentTypeId equals rt.Id into rtL
                                      from rt in rtL.DefaultIfEmpty()
                                      join lr in _lookUpListRepository.GetAll() on vr.FirmTypeId equals lr.Id into lrL
                                      from lr in lrL.DefaultIfEmpty()
                                      where v.Status && vc.Status && v.Id == item.VehicleId
                                      select new RVehicleCostDto()
                                      {
                                          VehicleId = v.Id,
                                          Plate = v.Plate,
                                          StartDate = vc.StartDate,
                                          //DayCount = (dateNow - vc.EndDate).Days,
                                          VehicleTypeName = v.FixtureTypeId == (int)FixtureType.Ownership ? "Mülkiyet" : "Kiralık",
                                          EndDate = vc.EndDate,
                                          RentFirmName = lr.Name ?? "--",
                                          RentTypeName = rt.Name ?? "--"
                                      }).OrderByDescending(o => o.EndDate).Take(1).FirstOrDefault();

                    if (oneNote)
                        lastRecord.Button = "<i title='Bu araçta not bulundu' onclick='getVehicleNote(" + item.VehicleId + ")' style='color:red;cursor: pointer;' class='icon-bell3 faa-ring animated faa-slow'></i>";

                    if (lastRecord != null && lastRecord.EndDate <= dateNow)
                        result.Add(lastRecord);
                }
                else
                {
                    if (oneNote)
                        item.Button = "<i title='Bu araçta not bulundu' onclick='getVehicleNote(" + item.VehicleId + ")' style='color:red;cursor: pointer;' class='icon-bell3 faa-ring animated faa-slow'></i>";

                    result.Add(item);
                }
            }

            return result;
        }
        public async Task<List<EVehicleExaminationDateDto>> GetTimeUpExaminationVehicle(RFilterModelDto filterModel)
        {
            var dateNow = DateTime.Now.AddDays(10).Date;
            var plates = await Task.FromResult(from ed in _vehicleExaminationDateRepository.GetAll()
                                               join v in _vehicleRepository.GetAll() on ed.VehicleId equals v.Id
                                               join un in _unitRepository.GetAll() on v.LastUnitId equals un.Id into unL
                                               from un in unL.DefaultIfEmpty()
                                               join un1 in _unitRepository.GetAll() on un.ParentId equals un1.Id into un1L
                                               from un1 in un1L.DefaultIfEmpty()
                                                   //join ft in _lookUpListRepository.GetAll() on v.FixtureTypeId equals ft.Id
                                               where v.Status && ed.Status &&
                                               (ed.KDocumentEndDate <= dateNow || ed.ExaminationEndDate <= dateNow || ed.KaskoEndDate <= dateNow || ed.TrafficEndDate <= dateNow)
                                               select new EVehicleDto()
                                               {
                                                   VehicleId = v.Id,
                                                   Plate = v.Plate,
                                                   KDocumentEndDate = ed.KDocumentEndDate,
                                                   ExaminationEndDate = ed.ExaminationEndDate,
                                                   KaskoEndDate = ed.KaskoEndDate,
                                                   TrafficEndDate = ed.TrafficEndDate,
                                                   //FixtureName = ft.Name,
                                                   UnitId = un.Id,
                                                   ParentUnitId = un1.Id,
                                                   VehicleTypeName = v.FixtureTypeId == (int)FixtureType.Ownership ? "Mülkiyet" : "Kiralık",
                                               });

            if (filterModel.ParentUnitId > 0)
                plates = plates.Where(w => w.ParentUnitId == filterModel.ParentUnitId);
            else if (filterModel.UnitId > 0)
                plates = plates.Where(w => w.UnitId == filterModel.UnitId);

            var plateList = plates.DistinctBy(d => d.VehicleId).ToList();
            var result = new List<EVehicleExaminationDateDto>();
            foreach (var item in plateList)
            {
                var temp = new EVehicleExaminationDateDto()
                {
                    VehicleId = item.VehicleId,
                    Plate = item.Plate,
                    Name = item.VehicleTypeName,
                    KDocumentEndDate = item.KDocumentEndDate != null ? DateControl(item.KDocumentEndDate.Value) : "---",
                    ExaminationEndDate = item.ExaminationEndDate != null ? DateControl(item.ExaminationEndDate.Value) : "---",
                    KaskoEndDate = item.KaskoEndDate != null ? DateControl(item.KaskoEndDate.Value) : "---",
                    TrafficEndDate = item.TrafficEndDate != null ? DateControl(item.TrafficEndDate.Value) : "---",
                };

                if (await _oneNoteGenericRepository.AnyAsync(w => w.Status && w.VehicleId == item.VehicleId && w.Status && w.Type != 3))
                    temp.Button = "<i title='Bu araçta not bulundu' onclick='getVehicleNote(" + item.VehicleId + ")' style='color:red;cursor: pointer;' class='icon-bell3 faa-ring animated faa-slow'></i>";

                result.Add(temp);
            }
            return result;
        }

        public async Task<List<EVehicleDto>> GetServiceInVehicleList()
        {
            var list = await Task.FromResult((from v in _vehicleRepository.GetAll()
                                              join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()
                                                  //join vd in _vehicleDebitRepository.GetAll() on v.Id equals vd.VehicleId
                                                  //join u in _userRepository.GetAll() on vd.CreatedBy equals u.Id
                                              where v.Status && v.LastStatus == (int)DebitState.InService
                                              select new EVehicleDto()
                                              {
                                                  Id = v.Id,
                                                  Plate = "<a onclick='funcEditVehicle(" + v.Id + ");' class='text-bold' style='font-size: 11px;'>" + v.Plate + "</a>",
                                                  //Plate2 = vd.TempPlateNo,
                                                  //DebitStartDate = vd.StartDate,
                                                  //NameSurname = u.Name + " " + u.Surname,
                                                  UnitName = unit2.Name + "/" + unit.Name
                                              }).ToList());


            foreach (var item in list)
            {
                var debit = _vehicleDebitRepository.GetAll()
                    .Where(w => w.VehicleId == item.Id && w.Status && w.State == (int)DebitState.InService)
                    .OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                if (debit != null)
                {
                    item.Plate2 = debit.TempPlateNo;
                    item.DebitStartDate = debit.StartDate;
                }
            }

            return list;
        }

        public string DateControl(DateTime date)
        {
            var dateNow = DateTime.Now.Date;
            string result;
            if (date == null)
            {
                result = "---";
            }
            else
            {
                var dateDiff = (date - dateNow).TotalDays;
                if (dateDiff >= 5)
                    result = date.ToString("dd/MM/yyyy");
                else if (0 < dateDiff && dateDiff <= 5)
                    result = "<span class='label bg-orange-300 full-width'>" + date.ToString("dd/MM/yyyy") + "</span>";
                else
                    result = "<span class='label bg-danger-300 full-width'>" + date.ToString("dd/MM/yyyy") + "</span>";

            }

            return result;
        }

        #endregion

        #region Vehicle Image
        //Bir önceki ay resim yüklemeyenleri listesi
        public async Task<List<EVehicleDto>> GetNotLoadVehicleImage(EVehicleDto filterModel)
        {
            var today = DateTime.Today;
            var first = new DateTime(today.Year, today.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);

            if (filterModel.TransactionDate != DateTime.MinValue)
            {
                today = filterModel.TransactionDate;
                first = new DateTime(today.Year, today.Month, 1);
                last = first.AddMonths(1).AddDays(-1);
            }

            var all = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                            join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                                            from unit in unitL.DefaultIfEmpty()
                                            join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                            from unit2 in unit2L.DefaultIfEmpty()
                                            join u in _userRepository.GetAll() on v.LastUserId equals u.Id into uL
                                            from u in uL.DefaultIfEmpty()
                                            where v.Status
                                            select new EVehicleDto()
                                            {
                                                Id = v.Id,
                                                UnitId = unit.Id,
                                                ParentUnitId = unit2.Id,
                                                UnitName = unit2.Name + "/" + unit.Name,
                                                Plate = v.Plate,
                                                DebitCreatedDate = null,
                                                LastStatus = v.LastStatus,
                                                TempPlateNo2 = v.Plate,
                                                Plate2 = "<a onclick='funcEditVehicle(" + v.Id + ")' class='text-bold' style='font-size: 11px; color:blue;'>" + v.Plate + "</a>",
                                                DebitNameSurname = u != null ? u.Name + " " + u.Surname + " (" + u.MobilePhone + ")" : "",
                                            });

            var load = await Task.FromResult((from vi in _vehiclePhysicalImageGenericRepository.GetAll()
                                              join v in _vehicleRepository.GetAll() on vi.VehicleId equals v.Id
                                              join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                                              from unit in unitL.DefaultIfEmpty()
                                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                                              from unit2 in unit2L.DefaultIfEmpty()
                                              join l in _userRepository.GetAll() on vi.CreatedBy equals l.Id
                                              where v.Status && vi.Status && vi.CreatedDate > first && vi.CreatedDate < last
                                              select new EVehicleDto()
                                              {
                                                  Id = vi.VehicleId,
                                                  UnitId = unit.Id,
                                                  ParentUnitId = unit2.Id,
                                                  Plate = v.Plate,
                                                  CreatedDate = vi.CreatedDate,
                                                  NameSurname = l.Name + " " + l.Surname + " (" + l.MobilePhone + ")",
                                              }).Distinct());

            if (!filterModel.IsAdmin)
            {
                var user = _userRepository.Find(filterModel.CreatedBy);
                var unit = _unitRepository.Find(user.UnitId.Value);
                filterModel.ParentUnitId = unit.ParentId;
                filterModel.UnitId = unit.Id;
            }

            if (filterModel.ParentUnitId > 0)
            {
                all = all.Where(w => w.ParentUnitId == filterModel.ParentUnitId);
                load = load.Where(w => w.ParentUnitId == filterModel.ParentUnitId);
            }

            if (filterModel.UnitId > 0)
            {
                all = all.Where(w => w.UnitId == filterModel.UnitId);
                load = load.Where(w => w.UnitId == filterModel.UnitId);
            }

            var vehicleLst = all.ToList();

            foreach (var item in vehicleLst)
            {
                if (string.IsNullOrEmpty(item.DebitNameSurname))
                {
                    if (item.LastStatus == (int)DebitState.Pool)
                        item.DebitNameSurname = "Havuzda";
                    else if (item.LastStatus == (int)DebitState.InService)
                        item.DebitNameSurname = "Serviste";
                }

                var temp = load.Where(w => w.Id == item.Id).OrderByDescending(o => o.CreatedDate).Take(1).FirstOrDefault();
                if (temp != null)
                {
                    item.NameSurname = temp.NameSurname;
                    item.DebitCreatedDate = temp.CreatedDate;
                }
            }

            //Yüklene araç sayısı info
            if (vehicleLst.Any())
                vehicleLst[0].TempPlateNo = " <b>" + vehicleLst.Count + "</b> araçtan <b>" + vehicleLst.Count(c => c.DebitCreatedDate == null) + "</b> adet yükleme bekleniyor. <br>" +
                    "Rapor Tarihi: " + first.ToString("dd.MM.yyyy") + "-" + last.ToString("dd.MM.yyyy");

            return vehicleLst.OrderBy(o => o.DebitCreatedDate).ToList();
        }
        #endregion
    }

}