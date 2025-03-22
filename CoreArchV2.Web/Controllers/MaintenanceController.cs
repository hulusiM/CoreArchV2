using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class MaintenanceController : AdminController
    {
        private readonly IFileService _fileService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;



        public MaintenanceController(IMapper mapper,
            IUnitOfWork uow,
            IFileService fileService,
            IReportService reportService,
            IMaintenanceService maintenanceService)
        {
            _mapper = mapper;
            _uow = uow;
            _fileService = fileService;
            _reportService = reportService;
            _maintenanceService = maintenanceService;
        }

        public IActionResult Index() => View();
        public IActionResult HgsIndex() => View();

        public IActionResult MaintenanceGetAll(int? page, bool isHgs, EMaintenanceDto filterModel)
        {
            var result = _maintenanceService.GetAllWithPaged(page, isHgs, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Maintenance/MaintenanceGetAll"));
            return Json(result);
        }
        public async Task<IActionResult> InsertUpdate(IList<IFormFile> files, EMaintenanceDto tempModel)
        {
            if (tempModel.UserFaultAmount == 0)
                tempModel.UserFaultAmount = null;

            var result = new EResultDto();
            if (tempModel.Id > 0)
                result = await _maintenanceService.Update(files, tempModel);
            else
            {
                tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = await _maintenanceService.Insert(files, tempModel);
            }
            return Json(result);
        }
        public IActionResult GetById(int id) => Json(_maintenanceService.GetById(id));
        public IActionResult Delete(int id) => Json(_maintenanceService.Delete(id));
        public IActionResult GetByVehicleIdMaintenanceHistory(int vehicleId, bool isHgsFilter) => Json(_maintenanceService.GetByVehicleIdMaintenanceHistory(vehicleId, isHgsFilter));
        public IActionResult GetMaintenanceFilesList(int maintenanceId) => Json(_maintenanceService.GetFilesList(maintenanceId));

    }
}