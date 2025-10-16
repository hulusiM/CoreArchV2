using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class UtilityController : AdminController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ChatMessage> _chatMessageRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Color> _colorRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IGenericRepository<MaintenanceFile> _maintenanceFileRepository;
        private readonly IGenericRepository<Maintenance> _maintenanceRepository;
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleBrandModel> _vehiclebrandRepository;
        private readonly IGenericRepository<VehicleModel> _vehicleModelRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehicleDebit> _vehicleDebitRepository;
        private readonly IGenericRepository<VehicleBrandModel> _vehicleBrandRepository;
        private readonly IUtilityService _utilityService;


        public UtilityController(IUnitOfWork uow,
            IUtilityService utilityService,
            IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _cityRepository = uow.GetRepository<City>();
            _colorRepository = uow.GetRepository<Color>();
            _vehiclebrandRepository = uow.GetRepository<VehicleBrandModel>();
            _vehicleModelRepository = uow.GetRepository<VehicleModel>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _maintenanceFileRepository = uow.GetRepository<MaintenanceFile>();
            _maintenanceRepository = uow.GetRepository<Maintenance>();
            _unitRepository = uow.GetRepository<Unit>();
            _messageRepository = uow.GetRepository<Message>();
            _chatMessageRepository = uow.GetRepository<ChatMessage>();
            _vehicleDebitRepository = uow.GetRepository<VehicleDebit>();
            _vehicleBrandRepository = uow.GetRepository<VehicleBrandModel>();
            _utilityService = utilityService;
        }

        #region Logistics Unit and Project
        public IActionResult Unit() => View();
        public async Task<IActionResult> UnitGetAll(int? page, EUnitDto filterModel)
        {
            var result = await _utilityService.GetAllWithPagedUnit(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Utility/UnitGetAll"));
            return Json(result);
        }
        public IActionResult UnitGetById(int id) => Json(_utilityService.GetByIdUnit(id));
        public IActionResult UnitInsertUpdate(EUnitDto model)
        {
            var result = new EResultDto();
            if (model.Id > 0)
                result = _utilityService.UpdateUnit(model);
            else
            {
                model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _utilityService.InsertUnit(model);
            }
            return Json(result);
        }
        public IActionResult UnitDelete(int id) => Json(_utilityService.DeleteUnit(id));
        #endregion

        #region Kurum ve müdürlük (Institution)
        public IActionResult Institution() => View();//Kurum ve müdürlük
        public async Task<IActionResult> InstitutionGetAll(int? page, EUnitDto filterModel)
        {
            var result = await _utilityService.GetAllWithPagedInstitution(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Utility/InstitutionGetAll"));
            return Json(result);
        }
        public IActionResult InstitutionGetById(int id) => Json(_utilityService.GetByIdInstitution(id));
        public IActionResult InstitutionInsertUpdate(EUnitDto model)
        {
            var result = new EResultDto();
            if (model.Id > 0)
                result = _utilityService.UpdateInstitution(model);
            else
            {
                model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _utilityService.InsertInstitution(model);
            }
            return Json(result);
        }
        public IActionResult InstitutionDelete(int id) => Json(_utilityService.DeleteInstitution(id));
        #endregion


        public IActionResult GetPageList()
        {
            var sessionContext = HttpContext.Session.GetString("PageList");
            return Json(sessionContext);
        }
        public IActionResult GetColorById(int id)
        {
            return Json(_colorRepository.Find(id));
        }
        public IActionResult GetUserById(int userId)
        {
            var result = new User();
            if (userId > 0)
                result = _userRepository.Find(userId);

            return Json(new EComboboxDto { Id = result.Id, Name = result.Name + " " + result.Surname + "/" + result.MobilePhone });
        }
        public IActionResult GetVehicleById(int vehicleId)
        {
            var result = _vehicleRepository.Find(vehicleId);
            return Json(new EComboboxDto { Id = result.Id, Name = result.Plate });
        }
        public IActionResult GetCityById(int cityId)
        {
            var result = _cityRepository.Find(cityId);
            return Json(new EComboboxDto { Id = result.Id, Name = result.Name });
        }
        public IActionResult GetUnitById(int unitId)
        {
            var result = _unitRepository.Find(unitId);
            return Json(new EComboboxDto { Id = result.Id, Name = result.Name });
        }
        public IActionResult GetBrandById(int brandId)
        {
            var result = _vehicleBrandRepository.Find(brandId);
            return Json(new EComboboxDto { Id = result.Id, Name = result.Name });
        }
        //Trafik Ceza tarihinde zimmet kimde?
        public IActionResult FindDateRangeDebitUser(EVehicleDto model)
        {
            var debitVehicleList = _vehicleDebitRepository
                .Where(w => w.VehicleId == model.VehicleId && w.StartDate <= model.TransactionDate && (w.State == (int)DebitState.Pool || w.State == (int)DebitState.Debit || w.State == (int)DebitState.InService))
                .ToList();

            EUserDto result = null;
            if (debitVehicleList.Count > 0)
            {
                foreach (var t in debitVehicleList)
                    if (t.EndDate == null)
                        t.EndDate = DateTime.Now;

                result = new EUserDto();
                result = (from d in debitVehicleList.Where(w => w.StartDate <= model.TransactionDate && model.TransactionDate < w.EndDate)
                          join u in _userRepository.GetAll() on d.DebitUserId equals u.Id into uL
                          from u in uL.DefaultIfEmpty()
                          select new EUserDto()
                          {
                              FullName = d.State == (int)DebitState.Pool ? "Havuz" : (d.State == (int)DebitState.InService ? "Serviste" : (u.Name + " " + u.Surname)),
                          }).FirstOrDefault();
            }
            return Json(result);
        }
        public IActionResult GetUnitWithParentInfo(int unitId)
        {
            var entityAll = _unitRepository.GetAll()
                .Select(s => new EUnitDto { Id = s.Id, Name = s.Name, ParentId = s.ParentId });

            var unit = entityAll.FirstOrDefault(w => w.Id == unitId);
            var parentUnit = entityAll.FirstOrDefault(w => w.Id == unit.ParentId);

            var result = new EUnitsDto
            {
                UnitParentList = entityAll.Where(w => w.ParentId == null).ToList(),
                Unit = entityAll.FirstOrDefault(w => w.Id == unitId),
                ParentUnit = entityAll.FirstOrDefault(w => w.Id == unit.ParentId)
            };
            return Json(result);
        }
        public IActionResult InsertLookUpList(LookUpList entity)
        {
            var result = new EResultDto();
            try
            {
                var isAny = _lookUpListRepository.Any(w =>
                    w.Name.ToLower() == entity.Name.ToLower() && w.Type == entity.Type);
                if (isAny)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu isimde kayıt zaten var";
                }
                else
                {
                    var lastRecord = _lookUpListRepository.GetAll().OrderByDescending(o => o.Id).Take(1)
                        .FirstOrDefault();
                    entity.Id = lastRecord.Id + 1;
                    entity.Name = entity.Name.TrimStart().TrimEnd();
                    _lookUpListRepository.Insert(entity);
                    _uow.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
            }

            return Json(result);
        }
        public async Task<IActionResult> GetMessages(int messageType)
        {
            var result = await Task.FromResult((await _messageRepository
                    .WhereAsync(w => w.Status == true && w.UserId == _loginUserInfo.Id && w.Type == messageType))
                .OrderByDescending(o => o.Id).Take(10).ToList());

            return Json(result);
        }
        public async Task<IActionResult> GetChatMessages(int pageIndex)
        {
            var passMessageHistory = await Task.FromResult((from m in _chatMessageRepository.GetAll()
                                                            join u in _userRepository.GetAll() on m.UserId equals u.Id
                                                            select new EChatMessageDto
                                                            {
                                                                Id = m.Id,
                                                                UserId = m.UserId,
                                                                UserNameSurname = u.Name + " " + u.Surname,
                                                                Message = m.Message,
                                                                CreatedDate = m.CreatedDate
                                                            }).OrderByDescending(o => o.Id).Skip(pageIndex * 15).Take(15).ToList());
            return Json(passMessageHistory);
        }
        public async Task<IActionResult> MessageReadAndUnread(int messageId)
        {
            try
            {
                var message = await Task.FromResult(_messageRepository.Find(messageId));
                if (message.UnRead)
                    message.UnRead = false;
                else
                    message.UnRead = true;
                _messageRepository.Update(message);
                _uow.SaveChanges();
                return Json(true);
            }
            catch (Exception e)
            {
                return Json(false);
            }
        }
        public async Task<IActionResult> MessageDelete(int messageId)
        {
            try
            {
                var message = await Task.FromResult(_messageRepository.Find(messageId));
                message.Status = false;
                _messageRepository.Update(message);
                _uow.SaveChanges();
                return Json(true);
            }
            catch (Exception e)
            {
                return Json(false);
            }
        }
    }
}