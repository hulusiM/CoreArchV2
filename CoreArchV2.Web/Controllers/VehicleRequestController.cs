using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class VehicleRequestController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVehicleRequestService _vehicleRequestService;

        public VehicleRequestController(IMapper mapper,
            IUnitOfWork uow,
            IVehicleRequestService vehicleRequestService)
        {
            _mapper = mapper;
            _uow = uow;
            _vehicleRequestService = vehicleRequestService;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult VehicleRequestGetAll(int? page, EVehicleRequestDto filterModel)
        {
            var result = _vehicleRequestService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/VehicleRequest/VehicleRequestGetAll"));
            return Json(result);
        }

        public IActionResult InsertUpdate(EVehicleRequestDto tempModel)
        {
            var result = new EResultDto();
            if (tempModel.Id > 0)
            {
                result = _vehicleRequestService.Update(tempModel);
            }
            else
            {
                tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _vehicleRequestService.Insert(tempModel);
            }

            return Json(result);
        }

        public IActionResult GetById(int id)
        {
            return Json(_vehicleRequestService.GetById(id));
        }

        public IActionResult Delete(int id)
        {
            return Json(_vehicleRequestService.Delete(id));
        }
    }
}