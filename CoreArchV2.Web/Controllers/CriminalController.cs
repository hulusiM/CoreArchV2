using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class CriminalController : AdminController
    {
        private readonly ICriminalLogService _criminalLogService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;


        public CriminalController(IMapper mapper,
            IUnitOfWork uow,
            ICriminalLogService criminalLogService,
            IFileService fileService)
        {
            _mapper = mapper;
            _uow = uow;
            _fileService = fileService;
            _criminalLogService = criminalLogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CriminalGetAll(int? page, ECriminalLogDto filterModel)
        {
            var result = _criminalLogService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Criminal/CriminalGetAll"));
            return Json(result);
        }

        public IActionResult InsertUpdate(IList<IFormFile> files, ECriminalLogDto tempModel)
        {
            var result = new EResultDto();
            if (tempModel.Id > 0)
            {
                result = _criminalLogService.Update(files, tempModel);
            }
            else
            {
                tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _criminalLogService.Insert(files, tempModel);
            }

            return Json(result);
        }

        public IActionResult GetById(int id)
        {
            return Json(_criminalLogService.GetById(id));
        }

        public IActionResult Delete(int id)
        {
            return Json(_criminalLogService.Delete(id));
        }
    }
}