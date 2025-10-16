using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class HomeController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cacheService;
        private readonly IReportService _reportService;
        private readonly IVehicleService _vehicleService;

        public HomeController(IMapper mapper,
            IUnitOfWork uow,
            IVehicleService vehicleService,
            ICacheService cacheService,
            IReportService reportService)
        {
            _mapper = mapper;
            _uow = uow;
            _vehicleService = vehicleService;
            _reportService = reportService;
            _cacheService = cacheService;
        }

        #region Views
        public IActionResult Index() => View();
        public IActionResult NotFoundPage() => View();
        public IActionResult NotAuthorityPage() => View();
        #endregion

        public async Task<IActionResult> ActiveVehicleCount(RFilterModelDto filterModel)
        {
            //    string theKey = MemoryCache_.HomePageActiveVehicle;
            //    var result =  _cacheService.GetOrSetAsync(theKey, async () =>
            //    {
            //        if (!_loginUserInfo.IsAdmin)
            //            filterModel = ForAutVehicleSetUnitId(filterModel);

            //        var list = await _reportService.ActiveVehicleCount(filterModel);
            //        return Json(list);
            //    });

            //    return Json(result);

            if (!_loginUserInfo.IsAdmin)
                filterModel = ForAutVehicleSetUnitId(filterModel);

            var list = await _reportService.ActiveVehicleCount(filterModel);
            return Json(list);
        }

        //Eskiden son zimmetlileri listelerdi
        public async Task<IActionResult> GetLastVehicleDebit()
        {
            //Son zimmetlenen araçlar olarak çalışır
            //var filterModel = new RFilterModelDto();
            //if (!_loginUserInfo.IsAdmin)
            //    filterModel = ForAutVehicleSetUnitId(filterModel);

            //var list = await _reportService.GetLastVehicleDebit(filterModel);

            //Servisteki araçları listeler
            var list = await _vehicleService.GetServiceInVehicleList();
            return Json(list);
        }

        //Kullanıcının yetkili olduğu birimi varsa parentUnitId değerini set eder
        public RFilterModelDto ForAutVehicleSetUnitId(RFilterModelDto filterModel)
        {
            if (_loginUserInfo.UnitId == null) //Müdürlük yetkisi var demektir
                filterModel.ParentUnitId = _loginUserInfo.ParentUnitId;
            else //Proje yetkisi var demektir
            {
                filterModel.ParentUnitId = null;
                filterModel.UnitId = _loginUserInfo.UnitId;
            }

            return filterModel;
        }

        #region Home Notification

        public async Task<IActionResult> GetNotificationMessages()
        {
            var filterModel = new RFilterModelDto();
            if (!_loginUserInfo.IsAdmin)
                filterModel = ForAutVehicleSetUnitId(filterModel);

            var result = await _vehicleService.GetNotificationMessages(filterModel);
            return Json(result);
        }

        public async Task<IActionResult> GetTimeUpContractVehicle(RFilterModelDto filterModel)
        {
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            if (!_loginUserInfo.IsAdmin)
                filterModel = ForAutVehicleSetUnitId(filterModel);

            var result = await _vehicleService.GetTimeUpContractVehicle(filterModel);
            return Json(result);
        }

        public async Task<IActionResult> GetTimeUpExaminationVehicle()
        {
            var filterModel = new RFilterModelDto();
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            if (!_loginUserInfo.IsAdmin)
                filterModel = ForAutVehicleSetUnitId(filterModel);

            var result = await Task.Run(() => _vehicleService.GetTimeUpExaminationVehicle(filterModel));

            return Json(result);
        }
        #endregion

    }
}