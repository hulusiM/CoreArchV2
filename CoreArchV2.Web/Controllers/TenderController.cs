using AutoMapper;
using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Core.Enum.Tender;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ETenderDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class TenderController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly ITenderService _tenderService;
        private readonly IUtilityService _utilityService;
        private readonly IGenericRepository<TenderDetail> _tenderDetailRepository;
        private readonly IUnitOfWork _uow;

        public TenderController(IMapper mapper,
            IUnitOfWork uow,
            IUtilityService utilityService,
            ITenderService tenderService)
        {
            _uow = uow;
            _mapper = mapper;
            _tenderService = tenderService;
            _utilityService = utilityService;
            _tenderDetailRepository = uow.GetRepository<TenderDetail>();
        }

        public IActionResult Index() => View();
        public IActionResult TenderUnit() => View();
        public IActionResult TenderGetAll(int? page, ETender_Dto filterModel)
        {
            filterModel.CreatedUnitId = GetLoginUserUnitId();
            var result = _tenderService.GetAllWithPaged(page, filterModel, _loginUserInfo.IsAdmin);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Tender/TenderGetAll"));
            return Json(result);
        }

        public IActionResult TenderForUnitGetAll(int? page, ETender_Dto filterModel)
        {
            filterModel.CreatedUnitId = GetLoginUserUnitId();
            var result = _tenderService.GetAllForUnitWithPaged(page, filterModel, _loginUserInfo.IsAdmin);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Tender/TenderForUnitGetAll"));
            return Json(result);
        }

        #region Tender
        public IActionResult InsertUpdate(IList<IFormFile> files, ETender_Dto tempModel)
        {
            var result = new EResultDto();
            if (tempModel.Id > 0)
                result = _tenderService.UpdateTender(files, tempModel);
            else
            {
                tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _tenderService.InsertTender(files, tempModel);
            }
            return Json(result);
        }
        public IActionResult GetById(int id) => Json(_tenderService.GetByIdTender(id));
        public IActionResult Delete(int id) => Json(_tenderService.DeleteTender(id));
        public IActionResult CreateNewSalesNumber() => Json(_tenderService.CreateNewSalesNumber(_loginUserInfo.Id));
        public IActionResult GetByIdTenderHistory(int tenderId)
            => Json(_tenderService.GetByIdTenderHistory(tenderId));
        #endregion

        #region Tender Contact
        public IActionResult InsertUpdateTenderContract(ETenderAllDto tempModel)
        {
            tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            return Json(_tenderService.InsertUpdateTenderContract(tempModel));
        }
        public IActionResult GetByTenderIdContactList(int tenderId)
            => Json(_tenderService.GetByTenderIdContactList(tenderId));
        #endregion

        #region TenderDetail
        public IActionResult GetTenderDetailPriceHistory(int tenderDetailId)
            => Json(_tenderService.GetTenderDetailPriceHistory(tenderDetailId));
        public IActionResult GetTenderDetail(int tenderId)
            => Json(_tenderService.GetTenderDetail(tenderId));
        public IActionResult InsertTenderDetail(ETenderAllDto tempModel)
        {
            var result = new EResultDto() { IsSuccess = false };
            var tender = _tenderService.GetByIdTender(tempModel.TenderId);
            if (!tender.Status)
                result.Message = "Pasif ihale/satış üzerinden işlem yapılamaz";
            else if (tender.State == (int)TenderState_.Draft || tender.State == (int)TenderState_.Revise)
            {
                tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                result = _tenderService.InsertUpdateTenderDetail(tempModel, GetLoginUserUnitId());
            }
            else
                result.Message = "Sadece <b>revize</b> ve <b>iş arttırımı</b> modunda değişiklik yapılabilir";

            return Json(result);
        }
        #endregion

        #region Tender Unit 

        public IActionResult GetTenderDetailForUnit(int tenderId)
            => Json(_tenderService.GetTenderDetailForUnit(tenderId, GetLoginUserUnitId(), _loginUserInfo.IsAdmin));
        public IActionResult GetTenderDetailByTenderDetailId(int tenderDetailId)
            => Json(_tenderService.GetTenderDetailByTenderDetailId(tenderDetailId, GetLoginUserUnitId(), _loginUserInfo.IsAdmin));
        public IActionResult InsertUpdateTenderDetailForUnit(IList<IFormFile> files, ETenderDetailDto tempModel)
        {
            tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
            tempModel.IsAdmin = _loginUserInfo.IsAdmin;
            return Json(_tenderService.InsertUpdateTenderDetailForUnit(files, tempModel, GetLoginUserUnitId()));
        }
        #endregion

        public int GetLoginUserUnitId()
        {
            var unitId = 0;
            if (_loginUserInfo.UnitId == null)
                unitId = _loginUserInfo.ParentUnitId.Value;
            else if (_loginUserInfo.UnitId != null)
                unitId = _loginUserInfo.UnitId.Value;
            return unitId;
        }

        public IActionResult GetTenderStateCmbx(int tenderId)
        {
            var result = new ESelect2ResultDto() { IsSuccess = false };
            var tender = _tenderService.GetByIdTender(tenderId);
            if (tender.Status)
                result.Message = "Pasif ihale/satış üzerinden işlem yapılamaz";
            else
            {
                var tenderDetail = _tenderDetailRepository.Where(w => w.TenderId == tenderId).ToList();
                if (tenderDetail.Any(a => a.SellingCost == null))
                    result.Message = "Satış kalemleri içinde <b>son fiyatı</b> verilmemiş ürünler bulunmaktadır! Kontrol edip tekrar deneyiniz.";
                else
                    result = SetTenderTypeComboBox(tender.State);
            }
            return Json(result);
        }


        public ESelect2ResultDto SetTenderTypeComboBox(int tenderState)
        {
            var result = new ESelect2ResultDto();
            if (tenderState == (int)TenderState_.Draft) //taslak
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.SendInstitution, Name = "Kuruma Gönder" });
            else if (tenderState == (int)TenderState_.SendInstitution)//kuruma gönderildi
            {
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.Revise, Name = "Revize Modu" });
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.NotResult, Name = "Olumsuz Sonuçlandı" });
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.WorkStarted, Name = "İşe Başla" });
            }
            else if (tenderState == (int)TenderState_.Revise)//Revize modu
            {
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.SendInstitution, Name = "Kuruma Gönder" });
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.NotResult, Name = "Olumsuz Sonuçlandı" });
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.WorkStarted, Name = "İşe Başla" });
            }
            else if (tenderState == (int)TenderState_.WorkStarted)//İşe başladı
            {
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.Delivered, Name = "Teslim Edildi" });
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.JobIncrease, Name = "İş Arttırıma Gidildi" });
            }
            else if (tenderState == (int)TenderState_.JobIncrease)//iş arttırımı
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.Delivered, Name = "Teslim Edildi" });
            else if (tenderState == (int)TenderState_.Delivered)//Teslim edildi
                result.ComboList.Add(new EUnitDto() { Id = (int)TenderState_.StartedWarranty, Name = "Garanti Başladı" });

            return result;
        }
    }
}