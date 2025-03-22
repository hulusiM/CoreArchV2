using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class BrandController : AdminController
    {
        private readonly IBrandService _brandService;


        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BrandGetAll(int? page, EUnitDto filterModel)
        {
            var result = await _brandService.GetAllWithPagedBrand(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Brand/BrandGetAll"));
            return Json(result);
        }

        public IActionResult BrandGetById(int id) => Json(_brandService.GetByIdBrand(id));

        public IActionResult BrandInsertUpdate(EUnitDto model)
        {
            var result = new EResultDto();
            if (model.Id > 0)
                result = _brandService.UpdateBrand(model);
            else
            {
                model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _brandService.InsertBrand(model);
            }
            return Json(result);
        }

        public IActionResult BrandDelete(int id) => Json(_brandService.DeleteBrand(id));

    }
}
