using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class TireController : AdminController
    {
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ITireLogService _tireLogService;
        private readonly IUnitOfWork _uow;


        public TireController(IMapper mapper,
            ITireLogService tireLogService,
            IUnitOfWork uow,
            IFileService fileService)
        {
            _mapper = mapper;
            _uow = uow;
            _fileService = fileService;
            _tireLogService = tireLogService;
        }

        public IActionResult Index() => View();

        public IActionResult TireGetAll(int? page, ETireDto filterModel)
        {
            var result = _tireLogService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Tire/TireGetAll"));
            return Json(result);
        }

        public IActionResult InsertUpdate(ETireDto tempModel)//Depoya lastik ekle
        {
            var result = new EResultDto();
            tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            result = _tireLogService.Insert(tempModel);
            return Json(result);
        }

        public IActionResult Delete(ETireDto tempModel) => Json(_tireLogService.Delete(tempModel));

        public IActionResult TireDebitInsert(ETireDto model)//Lastik değişimi
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(_tireLogService.TireDebitInsert(model));
        }

        public IActionResult SetTireInert(ETireDto model)//lastik atıl duruma at
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(_tireLogService.SetTireInert(model));
        }

        public IActionResult GetDebitTireInfo(ETireDto model) => Json(_tireLogService.GetDebitTireInfo(model));//Araç üstünde lastik takılı mı ?

        public IActionResult GetTireHistory(ETireDto model) => Json(_tireLogService.GetTireHistory(model));//lastik işlem geçmişi

        public IActionResult GetTireReport(ETireDto model) => Json(_tireLogService.GetTireReport(model));//depo lastik bilgileri

        public IActionResult TireReturn(ETireDto model)//Depoya iade
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(_tireLogService.TireReturn(model));
        }
    }
}