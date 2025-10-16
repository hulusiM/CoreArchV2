using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class VehicleController : AdminController
    {
        private readonly ICacheService _cacheService;
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService,
            ICacheService cacheService)
        {
            _vehicleService = vehicleService;
            _cacheService = cacheService;
        }
        public IActionResult Index() => View();
        public IActionResult VehicleGetAll(int? page, EVehicleDto filterModel)
        {
            var result = _vehicleService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Vehicle/VehicleGetAll"));
            return Json(result);
        }
        public async Task<IActionResult> GetByVehicleIdDebitHistory(int vehicleId)
        {
            var list = await Task.FromResult(_vehicleService.GetLastDebitUserHistory(vehicleId));
            if (list.Any() && list[0].DebitState2 != (int)DebitState.Deleted && _loginUserInfo.IsAdmin) //son satıra sil butonu ekle
            {
                list[0].CustomButton = "<a data-toggle='modal' onclick='funcDeleteDebit(" + list[0].VehicleDebitId + ")'><i class='icon-trash text-danger'></i></a>";
                //if (list[0].DebitState2 == (int)DebitState.InService)
                //    list[0].CustomButton += "<a data-toggle='modal' onclick='funcEditInService(" + list[0].VehicleDebitId + ")' style='margin-left: 5px;'><i class='icon-pencil5'></i></a>";
            }
            else if (list.Any() && list[0].DebitState2 == (int)DebitState.Deleted) //Araç silinmişse,silme bilgileri ekleniyor
                list[0].VehicleTransfer = _vehicleService.GetByIdVehicleDebit(vehicleId).VehicleTransfer;

            return Json(list);
        }
        public IActionResult GetAllVehicleList() => Json(_vehicleService.GetAllVehicleList(new EVehicleDto()));
        public IActionResult GetByPlate(string plate)
        {
            var result = _vehicleService.FindPlateList(plate);
            var message = "";
            if (result.FirstOrDefault(w => w.Status == true) != null)
                message = "Bu plakada aktif araç bulunmaktadır.";
            else if (result.FirstOrDefault(w => w.Status == false) != null)
                message = "Bu araç daha önce eklenip silinmiş";

            return Json(message);
        }

        //#region Vehicle Gain
        //public async Task<IActionResult> GetVehicleGain(int vehicleId)
        //{
        //    return Json(await _vehicleService.GetVehicleGain(vehicleId));
        //}
        //#endregion

        #region Mail
        public async Task<IActionResult> SendMailVehiclePhotography(EMailVehiclePhotographyDto model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(await _vehicleService.SendMailVehiclePhotography(model));
        }
        #endregion

        #region VehicleService
        public async Task<IActionResult> VehicleSetService(EVehicleServiceDto model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            var result = _vehicleService.VehicleSetService(model);
            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());

            return Json(result);
        }
        public async Task<IActionResult> VehicleOutService(EVehicleServiceDto model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            var result = _vehicleService.VehicleOutService(model);
            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());

            return Json(result);
        }
        #endregion

        #region Vehicle Table

        public async Task<IActionResult> InsertVehicle(EVehicleFixRentDto tempModel)
        {
            tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            EResultDto result;
            if (tempModel.Id > 0)
                result = _vehicleService.UpdateVehicle(tempModel);
            else
                result = _vehicleService.InsertVehicle(tempModel);

            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());
            return Json(result);
        }
        public IActionResult GetByIdVehicle(int id) => Json(_vehicleService.GetByIdVehicle(id));
        public async Task<IActionResult> BulkCostUpdate(decimal amountPercent) => Json(await _vehicleService.BulkCostUpdate(amountPercent)); //Araç güncel bedel toplu değiştir

        #endregion

        #region VehicleExaminationDate Table

        public IActionResult InsertVehicleExaminationDate(VehicleExaminationDate model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            EResultDto result;
            if (model.Id > 0)
                result = _vehicleService.UpdateVehicleExaminationDate(model);
            else
                result = _vehicleService.InsertVehicleExaminationDate(model);
            return Json(result);
        }
        public IActionResult GetByIdVehicleExaminationDate(int id)
        {
            return Json(_vehicleService.GetByIdVehicleExaminationDate(id));
        }

        #endregion

        #region VehicleDebit Table

        public async Task<IActionResult> InsertVehicleDebit(VehicleDebit model)
        {
            EResultDto result;
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            if (model.Id > 0)
                result = _vehicleService.UpdateVehicleDebit(model);
            else
                result = _vehicleService.InsertVehicleDebit(model);

            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());
            return Json(result);
        }
        public IActionResult GetByIdVehicleDebit(int vehicleId) => Json(_vehicleService.GetByIdVehicleDebit(vehicleId));//int vehicleId
        public IActionResult GetByVehicleDebitId(int vehicleDebitId)
        {
            var debit = _vehicleService.GetByVehicleDebitId(vehicleDebitId);
            var lastDebit = _vehicleService.GetByDebitWithVehicleId(debit.VehicleId);
            if (lastDebit.Id == vehicleDebitId || lastDebit.State != (int)DebitState.InService)//son zimmet üzerinden işlem yapabilir
                return Json(debit);
            else
                return Json(false);
        }
        public async Task<IActionResult> VehicleDebitSetUserNull(VehicleDebit model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            var result = _vehicleService.VehicleDebitSetUserNull(model);

            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());
            return Json(result);
        }
        public async Task<IActionResult> VehicleDebitDelete(int vehicleDebitId)
        {
            var result = _vehicleService.VehicleDebitDelete(vehicleDebitId);
            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());
            return Json(result);
        }

        #endregion

        #region Vehicle File

        public IActionResult GetByVehicleIdFileList(int vehicleId) => Json(_vehicleService.GetByVehicleIdFileList(vehicleId));
        public IActionResult GetByVehicleIdLastLoadImageList(int vehicleId, int typeId) => Json(_vehicleService.GetByVehicleIdLastLoadImageList(vehicleId, typeId));
        public IActionResult VehiclePhotographyInsert(IList<IFormFile> files, int vehicleId, int typeId)
        {
            var createdBy = (int)HttpContext.Session.GetInt32("UserId");
            var model = new EVehiclePhysicalImageLoadDto()
            {
                files = files,
                CreatedBy = createdBy,
                VehicleId = vehicleId,
                TypeId = typeId
            };
            return Json(_vehicleService.VehiclePhotographyInsert(model));
        }
        public IActionResult VehicleFileInsert(IList<IFormFile> files, int vehicleId) => Json(_vehicleService.VehicleFileInsert(files, vehicleId));
        public async Task<IActionResult> VehicleDelete(IList<IFormFile> files, EVehicleTransferFileDto model)
        {
            model.DeleteUserId = (int)HttpContext.Session.GetInt32("UserId");
            var result = _vehicleService.VehicleDelete(files, model);

            await _cacheService.Clear(MemoryCache_.ActiveVehicleList.ToString());
            return Json(result);
        }

        #endregion

        #region VehicleMaterial Table

        public IActionResult InsertVehicleMaterial(int vehicleId, int[] materials)
        {
            var createdBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(_vehicleService.InsertVehicleMaterial(vehicleId, materials, createdBy));
        }
        public IActionResult GetByIdVehicleMaterial(int vehicleId)
        {
            return Json(_vehicleService.GetByIdVehicleMaterial(vehicleId));
        }

        #endregion

        #region VehicleAmount

        public IActionResult InsertVehicleAmount(VehicleAmount model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(_vehicleService.InsertVehicleAmount(model));
        }
        public IActionResult GetByVehicleIdVehicleAmountHistory(int vehicleId, int vehicleAmountTypeId = 0) =>
            Json(_vehicleService.GetByVehicleIdVehicleAmountHistory(vehicleId, vehicleAmountTypeId, _loginUserInfo.IsAdmin));

        #endregion VehicleContract

        #region VehicleContract

        public async Task<IActionResult> InsertVehicleContract(VehicleContract model)
        {
            model.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            var result = new EResultDto();
            if (model.Id > 0)
                result = await _vehicleService.UpdateVehicleContract(model);
            else
                result = await _vehicleService.InsertVehicleContract(model);
            return Json(result);
        }
        public IActionResult DeleteVehicleContract(int vehicleId) => Json(_vehicleService.DeleteVehicleContract(vehicleId));

        //Araca göre sözleşme tarihler arasındaki masrafları listeler
        public async Task<IActionResult> GetByIdVehicleIdContractDateAndAmount(int vehicleId) =>
            Json(await _vehicleService.GetByIdVehicleIdContractDateAndAmount(vehicleId));

        //Sözleşme tutarı siler
        public IActionResult DeleteVehicleAmount(int vehicleContractId, int vehicleAmountId) =>
            Json(_vehicleService.DeleteVehicleAmount(vehicleContractId, vehicleAmountId));
        #endregion

        #region Vehicle Image
        //Bir önceki ay resim yüklemeyenleri listesi
        public async Task<IActionResult> GetNotLoadVehicleImage(EVehicleDto filterModel)
        {
            filterModel.CreatedBy = _loginUserInfo.Id;
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            var list = await _vehicleService.GetNotLoadVehicleImage(filterModel);
            return Json(list);
        }
        #endregion

        public EVehicleDto ForAutVehicleSetUnitId(EVehicleDto filterModel)
        {
            if (_loginUserInfo.IsAdmin) //adminse tüm birimleri listeleyebilir
            {
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
    }
}