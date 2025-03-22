using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class OutOfHourController : AdminController
    {
        private readonly IOutOfHourService _outOfHourService;
        public OutOfHourController(IOutOfHourService outOfHourService)
        {
            _outOfHourService = outOfHourService;
        }

        #region Mesai içi/dışı sayfası
        public IActionResult Index() => View();
        public IActionResult OutOfHourGetAll(int? page, EOutOfHourDto filterModel)
        {
            filterModel.CreatedBy = _loginUserInfo.Id;
            filterModel = ForAutVehicleSetUnitId(filterModel);
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            var result = _outOfHourService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/OutOfHour/OutOfHourGetAll"));
            return Json(result);
        }
        public EOutOfHourDto ForAutVehicleSetUnitId(EOutOfHourDto filterModel)
        {
            if (_loginUserInfo.IsAdmin) //adminse tüm birimleri listeleyebilir
            {
                filterModel.IsAdmin = _loginUserInfo.IsAdmin;
                filterModel.UnitId = filterModel.UnitId;
                filterModel.ParentUnitId = filterModel.ParentUnitId;
            }
            else //değilse sadece yetkili oldu birimi listeleyebilir
            {
                filterModel.UnitId = _loginUserInfo.UnitId == null ? filterModel.UnitId : _loginUserInfo.UnitId;
                filterModel.ParentUnitId = _loginUserInfo.ParentUnitId;
            }
            return filterModel;
        }
        public async Task<IActionResult> GetOutOfHourHistoryMap(int vecOperationId)
        {
            var list = await _outOfHourService.GetOutOfHourMap(vecOperationId);
            return Json(list);
        }
        #endregion


        #region Parametre sayfası
        public IActionResult ParamOutOfHourIndex() => View();
        public IActionResult ParamOutOfHourGetAll(int? page, EOutOfHourDto filterModel)
        {
            var result = _outOfHourService.GetAllParamOutOfHourWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/OutOfHour/ParamOutOfHourGetAll"));
            return Json(result);
        }
        public IActionResult ParamOutOfHourDelete(int id)
        {
            return Json(_outOfHourService.ParamOutOfHourDelete(id));
        }
        public IActionResult ParamOutOfHourSave(int unitId)
        {
            var entity = new VehicleOperatingReportParam()
            {
                CreatedBy = _loginUserInfo.Id,
                CreatedDate = System.DateTime.Now,
                UnitId = unitId
            };
            return Json(_outOfHourService.ParamOutOfHourSave(entity));
        }
        public IActionResult ComboParamOutOfUnitList() => Json(_outOfHourService.ComboParamOutOfUnitList());
        #endregion
    }
}
