using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class SpeedController : AdminController
    {
        private readonly INoticeService _noticeService;

        public SpeedController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        public IActionResult Index() => View();

        public IActionResult SpeedGetAll(int? page, ENoticeDto filterModel)
        {
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            filterModel.CreatedBy = _loginUserInfo.Id;
            var result = _noticeService.GetAllSpeedWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Speed/SpeedGetAll"));
            return Json(result);
        }
    }
}
