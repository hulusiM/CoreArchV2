using CoreArchV2.Core.Enum.NoticeVehicle.Notice;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeUnitDto_;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies.SessionOperations;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;

namespace CoreArchV2.Web.Controllers
{
    public class NoticeController : AdminController
    {

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INoticeService _noticeService;
        private readonly IVehicleService _vehicleService;
        private readonly IUserService _userService;


        public NoticeController(INoticeService noticeService,
            IWebHostEnvironment hostingEnvironment,
            IUserService userService,
            IVehicleService vehicleService)
        {
            _noticeService = noticeService;
            _hostingEnvironment = hostingEnvironment;
            _vehicleService = vehicleService;
            _userService = userService;
        }

        #region Views
        public IActionResult Index() => View();
        public IActionResult NoticeUnit() => View();
        public IActionResult NoticeUnitAnswer() => View();
        #endregion

        #region Notice
        public IActionResult NoticeGetAll(int? page, ENoticeDto filterModel)
        {
            var result = _noticeService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Notice/NoticeGetAll"));
            return Json(result);
        }

        #region Notice Excel import

        public async Task<IActionResult> ReadExcelForNotice(int noticeType) => Json(await ReadExcel(noticeType));

        public async Task<ENoticeReadExcelDto> ReadExcel(int noticeType)
        {
            IFormFile file = Request.Form.Files[0];
            string folderName = "tempFile";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            string fullPath = Path.Combine(newPath, file.FileName);
            var result = new ENoticeReadExcelDto { NoticeList = new List<ENoticeDto>() };
            try
            {
                if (file.Length > 0 && noticeType > 0)
                {
                    string sFileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(newPath))
                        Directory.CreateDirectory(newPath);

                    using (var stream = new FileStream(fullPath, FileMode.Create)) //Okunan excel kaydedildi altta silinecek
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0;
                        ISheet sheet;
                        if (sFileExtension == ".xls")
                        {
                            var hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                            sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                        }
                        else
                        {
                            var hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                            sheet = hssfwb.GetSheetAt(0);
                        }
                        IRow headerRow = sheet.GetRow(0);
                        int cellCount = headerRow.LastCellNum;
                        var list = new List<ENoticeDto>();
                        for (int i = (sheet.FirstRowNum); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;
                            if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                            var arventoNo = row.GetCell(0) != null ? row.GetCell(0).ToString()?.ToUpper().Replace(" ", "") : "";
                            var plate = row.GetCell(1) != null ? row.GetCell(1).ToString()?.ToUpper().Replace(" ", "") : "";
                            var drive = row.GetCell(2) != null ? row.GetCell(2).ToString() : "";
                            DateTime? firstDate = null;
                            DateTime? endDate = null;
                            DateTime? transactionDate = null;

                            decimal totalKm = 0;
                            decimal speed = 0;
                            var address = "";
                            if (noticeType == (int)NoticeType.Speed)
                            {
                                transactionDate = row.GetCell(3) != null ? Convert.ToDateTime(row.GetCell(3).DateCellValue) : (DateTime?)null;
                                speed = row.GetCell(4) != null ? Convert.ToDecimal(row.GetCell(4).ToString()) : 0;
                                if (speed == 0) continue;
                                address = row.GetCell(5).ToString();
                            }
                            else if (noticeType == (int)NoticeType.OutOfHours)
                            {
                                firstDate = row.GetCell(3) != null ? Convert.ToDateTime(row.GetCell(3).DateCellValue) : (DateTime?)null;
                                endDate = row.GetCell(4) != null ? Convert.ToDateTime(row.GetCell(4).DateCellValue) : (DateTime?)null;
                                totalKm = Convert.ToDecimal(row.GetCell(5).ToString());
                                if (totalKm == 0) continue;
                            }

                            if (!string.IsNullOrEmpty(plate))
                            {
                                list.Add(new ENoticeDto()
                                {
                                    ArventoNo = arventoNo,
                                    Plate = plate,
                                    Driver = drive,
                                    FirstRunEngineDate = firstDate,
                                    LastRunEngineDate = endDate,
                                    TotalKm = totalKm,
                                    TransactionDate = transactionDate,
                                    NoticeType = noticeType,
                                    Speed = speed,
                                    Address = address
                                });
                            }
                        }

                        //listeki plakalar sistemde var mı kontrol edilecek
                        var plateList = _vehicleService.GetAllVehicleList();
                        string[] html = new string[2];
                        int counterTrue = 1;
                        int counterFalse = 1;
                        foreach (var pl in list)
                        {
                            var vehicle = plateList.FirstOrDefault(w => w.Plate == pl.Plate);
                            if (vehicle != null)
                            {
                                var erpDebit = _vehicleService.GetByVehicleIdLastDebit(vehicle.VehicleId);
                                pl.Driver2 = erpDebit.DebitNameSurname;
                                pl.ArventoNo2 = erpDebit.ArventoNo;

                                pl.VehicleId = vehicle.VehicleId;
                                result.NoticeList.Add(pl);
                                html[0] += SetExcelRow_True(pl, counterTrue, pl.NoticeType);
                                counterTrue++;
                            }
                            else//sistemde plaka yok
                            {
                                html[1] += SetExcelRow_False(pl, counterFalse, " <b style='color:blue;'>(Plaka sistemde bulunamadı)</b>", pl.NoticeType);
                                counterFalse++;
                            }
                        }

                        result.HtmlString = html;

                        if (System.IO.File.Exists(fullPath))//okunan excel siliniyor
                            System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch (Exception e)
            {
                if (System.IO.File.Exists(fullPath))//okunan excel siliniyor
                    System.IO.File.Delete(fullPath);
            }
            return result;
        }
        public string SetExcelRow_True(ENoticeDto item, int counterTrue, int noticeType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Length = 0;
            sb.AppendLine("<tr>");
            sb.Append("<td>" + counterTrue + "</td>");
            if (!String.Equals(item.ArventoNo.Trim(), item.ArventoNo2.Trim(), StringComparison.CurrentCultureIgnoreCase))
                sb.Append("<td><span class='label bg-orange'>Arvento: (" + item.ArventoNo + ")<br/>Erp: (" + item.ArventoNo2 + ")</span></td>");
            else
                sb.Append("<td>" + item.ArventoNo + "</td>");

            sb.Append("<td><span class='label bg-success-300'>" + item.Plate + " (" + item.VehicleId + ")</span></td>");
            if (!String.Equals(item.Driver, item.Driver2, StringComparison.CurrentCultureIgnoreCase))
                sb.Append("<td><span class='label bg-warning'>Arvento: (" + item.Driver + ")<br/>Erp: (" + item.Driver2 + ")</span></td>");
            else
                sb.Append("<td>" + item.Driver + "</td>");

            if (noticeType == (int)NoticeType.Speed)
            {
                sb.Append("<td><span class='label bg-pink-300'>" + item.TransactionDate.Value.ToString("dd-MM-yyyy HH:mm") + "</span></td>");
                sb.Append("<td><span class='label bg-orange-300'>" + item.Speed + " Kmh</span></td>");
                sb.Append("<td>" + item.Address + "</td>");
            }
            else if (noticeType == (int)NoticeType.OutOfHours)
            {
                sb.Append("<td>" + item.FirstRunEngineDate.Value.ToString("dd-MM-yyyy HH:mm") + "</td>");
                sb.Append("<td>" + item.LastRunEngineDate.Value.ToString("dd-MM-yyyy HH:mm") + "</td>");
                sb.Append("<td>" + item.TotalKm + "</td>");
            }
            sb.AppendLine("</tr>");
            return sb.ToString();
        }
        public string SetExcelRow_False(ENoticeDto item, int counterFalse, string desc, int noticeType)//sistemde yok
        {
            StringBuilder sb = new StringBuilder();
            sb.Length = 0;
            sb.AppendLine("<tr>");
            sb.Append("<td>" + counterFalse + "</td>");
            sb.Append("<td>" + item.ArventoNo + "</td>");
            sb.Append("<td><span class='label bg-success-300'>" + item.Plate + " " + desc + "</span></td>");
            sb.Append("<td>" + item.Driver + "</td>");
            if (noticeType == (int)NoticeType.Speed)
            {
                sb.Append("<td><span class='label bg-pink-300'>" + item.TransactionDate.Value.ToString("dd-MM-yyyy HH:mm") + "</span></td>");
                sb.Append("<td><span class='label bg-orange-300'>" + item.Speed + " Kmh</span></td>");
                sb.Append("<td>" + item.Address + "</td>");
            }
            else if (noticeType == (int)NoticeType.OutOfHours)
            {
                sb.Append("<td><span class='label bg-orange-300'>" + item.FirstRunEngineDate.Value.ToString("dd-MM-yyyy HH:mm") + "</span></td>");
                sb.Append("<td><span class='label bg-orange-300'>" + item.LastRunEngineDate.Value.ToString("dd-MM-yyyy HH:mm") + "</span></td>");
                sb.Append("<td>" + item.TotalKm + "</td>");
            }
            sb.AppendLine("</tr>");
            return sb.ToString();
        }
        public void DeleteExcelFile(string directoryName)//excel dosyaya kaydedip okunuyor,okuma sonrası siliyor. Teyid amaçlı ilgili klasör içi temizleniyor
        {
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(directoryName);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);
            }
            catch (Exception e)
            {
            }
        }

        public async Task<IActionResult> ReadExcelForInsertNotice(int noticeType)
        {
            if (noticeType > 0)
            {
                var model = new ENoticeReadExcelDto()
                {
                    NoticeList = ReadExcel(noticeType).Result.NoticeList,
                    NoticeType = noticeType,
                    CreatedBy = _loginUserInfo.Id,
                    ImportType = (int)ImportType.Excel
                };
                var result = await _noticeService.InsertBulkAsync(model);
                return Json(result);
            }
            else
                return Json("");
        }
        #endregion

        #region Notice Methods
        public IActionResult InsertUpdateNotice(ENoticeDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            if ((model.NoticeType == (int)NoticeType.Speed || model.NoticeType == (int)NoticeType.Duty) && model.TransactionDate > DateTime.Now.Date)
                result.Message = "İşlem tarihi gelecek zaman olamaz";
            else if (model.NoticeType == (int)NoticeType.OutOfHours && (model.StartDate > model.EndDate))
                result.Message = "Mesai dışı ihlalinde baş.tarihi bit.tarihinden sonra olamaz";
            else
            {
                model.CreatedBy = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext").User.Id;
                if (model.Id > 0)
                    result = _noticeService.UpdateNotice(model);
                else
                    result = _noticeService.InsertNotice(model);
            }
            return Json(result);
        }
        public IActionResult ChangePartialBulkExcel(int type)
        {
            var result = type switch
            {
                (int)NoticeType.Speed => PartialView("PartialViews/VehicleNotice/Notice/_Speed", new List<ENoticeDto>()),
                (int)NoticeType.OutOfHours => PartialView("PartialViews/VehicleNotice/Notice/_OutOfHours", new List<ENoticeDto>()),
                _ => null
            };

            return result;
        }
        public async Task<IActionResult> GetByIdNotice(int id) => Json(await _noticeService.GetByIdNoticeAsync(id));
        public IActionResult Delete(int id) => Json(_noticeService.DeleteNotice(id));
        #endregion
        #endregion

        #region NoticeUnit
        public IActionResult NoticeUnitGetAll(int? page, ENoticeDto filterModel)
        {
            if (!_loginUserInfo.IsAdmin)
                filterModel.LoginUserId = _loginUserInfo.Id;
            var result = _noticeService.GetAllUnitWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Notice/NoticeUnitGetAll"));
            return Json(result);
        }
        public Task<IActionResult> GetTypeDateRangeVehicleList(ENoticeUnitDto model)
        {
            var list = GetTypeDateList(model);
            var result = model.NoticeType switch
            {
                (int)NoticeType.Speed => PartialView("PartialViews/VehicleNotice/Notice/Table/_SpeedTable", list),
                (int)NoticeType.OutOfHours => PartialView("PartialViews/VehicleNotice/Notice/Table/_OutOfHoursTable", list),
                (int)NoticeType.Duty => PartialView("PartialViews/VehicleNotice/Notice/Table/_DutyTable", list),
                _ => null
            };
            return Task.FromResult<IActionResult>(result);
        }
        public List<ENoticeDto> GetTypeDateList(ENoticeUnitDto model)
        {
            var noticeUnitVehicleList = _noticeService.GetUnitIdStartEndDateVehicleList(model);
            var groupByUnit = noticeUnitVehicleList.GroupBy(g => g.UnitId)
                .Select(s => new ENoticeDto()
                {
                    EditMode = s.First().EditMode,
                    UnitId = s.First().UnitId,
                    ToUnitName = s.First().ToUnitName,
                    PlateList = s.Where(w => w.UnitId == s.First().UnitId).ToList()
                }).ToList();

            return groupByUnit;
        }
        public IActionResult InsertUpdateNoticeUnit(IList<IFormFile> files, ENoticeUnitDto model)
        {
            var result = new EResultDto();
            if (model.NoticeList.Any(a => a.IsSend))
            {
                model.CreatedBy = _loginUserInfo.Id;
                if (model.Id > 0)
                    result = _noticeService.UpdateNoticeUnit(files, model);
                else
                    result = _noticeService.InsertNoticeUnit(files, model);
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "Talep açabilmek için en az 1 araç seçilmesi gerekiyor";
            }
            return Json(result);
        }
        public async Task<IActionResult> GetByIdNoticeUnit(int id) => Json(await _noticeService.GetByIdNoticeUnitAsync(id));
        public IActionResult DeleteNoticeUnit(ENoticeDto model)
        {
            var result = new EResultDto();
            model.LoginUserId = _loginUserInfo.Id;
            result = _noticeService.DeleteNoticeUnit(model);
            return Json(result);
        }
        #endregion

        #region NoticeUnit Answer
        public IActionResult NoticeUnitAnswerGetAll(int? page, ENoticeDto filterModel)
        {
            filterModel.LoginUserId = _loginUserInfo.Id;
            var result = _noticeService.GetAllUnitAnswerWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Notice/NoticeUnitAnswerGetAll"));
            return Json(result);
        }
        public IActionResult InsertNoticeUnitAnswer(ENoticeUnitDto model)
        {
            var result = new EResultDto() { IsSuccess = false };
            model.LoginUserId = _loginUserInfo.Id;
            if (model.RedirectType == (int)NoticeRedirectType.ContentEdit)
            {
                if (_noticeService.IsAutForNoticeUnit(model.Id, _loginUserInfo.Id))
                    result = _noticeService.InsertNoticeUnitAnswer(model);
                else
                    result.Message = "Sayfayı yenileyip tekrar deneyiniz";
            }
            else if (model.RedirectType == (int)NoticeRedirectType.Analysis)
                result = _noticeService.InsertNoticeUnitRedirectAnswer(model);

            return Json(result);
            //var result = new EResultDto() { IsSuccess = false };
            //if (_noticeService.IsAutForNoticeUnit(model.Id, _loginUserInfo.Id))
            //{
            //    model.LoginUserId = _loginUserInfo.Id;
            //    result = _noticeService.InsertNoticeUnitAnswer(model);
            //}
            //else
            //    result.Message = "Sayfayı yenileyip tekrar deneyiniz";
            //return Json(result);
        }
        public IActionResult GetNoticeUnitAnswerList(int noticeUnitId, int redirectType)
        {
            if (redirectType == (int)NoticeRedirectType.ContentEdit)//birim müdürü
                return NoticeUnitContentEdit(noticeUnitId);
            else if (redirectType == (int)NoticeRedirectType.Analysis)//insan kaynakları
                return NoticeUnitContentAnalysis(noticeUnitId);

            return PartialView(null, null);
        }
        public PartialViewResult NoticeUnitContentEdit(int noticeUnitId)
        {
            var noticeUnit = _noticeService.Find(noticeUnitId);
            var list = GetContentEdit(noticeUnitId);
            if (list.Any())
            {
                var noticeType = noticeUnit.NoticeType;
                var result = noticeType switch
                {
                    (int)NoticeType.Speed => PartialView("PartialViews/VehicleNotice/Notice/Table/_SpeedTable", list),
                    (int)NoticeType.OutOfHours => PartialView("PartialViews/VehicleNotice/Notice/Table/_OutOfHoursTable", list),
                    (int)NoticeType.Duty => PartialView("PartialViews/VehicleNotice/Notice/Table/_DutyTable", list),
                    _ => null
                };
                return result;
            }
            return PartialView(null, null);
        }
        public List<ENoticeDto> GetContentEdit(int noticeUnitId)
        {
            if (_noticeService.IsAutForNoticeUnit(noticeUnitId, _loginUserInfo.Id))
            {
                var noticeUnitVehicleList = _noticeService.GetNoticeUnitAnswerList(noticeUnitId, _loginUserInfo.Id);
                var groupByUnit = noticeUnitVehicleList.GroupBy(g => g.UnitId)
                    .Select(s => new ENoticeDto()
                    {
                        UnitMode = true,
                        NoticeType = s.First().NoticeType,
                        UnitId = s.First().UnitId,
                        ToUnitName = s.First().ToUnitName,
                        PlateList = s.Where(w => w.UnitId == s.First().UnitId).ToList()
                    }).ToList();
                return groupByUnit;
            }
            return null;
        }
        public PartialViewResult NoticeUnitContentAnalysis(int noticeUnitId)
        {
            var noticeUnit = _noticeService.Find(noticeUnitId);
            var list = GetUnitContentList(noticeUnitId);
            if (list.Any())
            {
                var noticeType = noticeUnit.NoticeType;
                var result = noticeType switch
                {
                    (int)NoticeType.Speed => PartialView("PartialViews/VehicleNotice/Notice/Table/_SpeedTable", list),
                    (int)NoticeType.OutOfHours => PartialView("PartialViews/VehicleNotice/Notice/Table/_OutOfHoursTable", list),
                    (int)NoticeType.Duty => PartialView("PartialViews/VehicleNotice/Notice/Table/_DutyTable", list),
                    _ => null
                };
                return result;
            }
            return PartialView(null, null);
        }
        public List<ENoticeDto> GetUnitContentList(int noticeUnitId)
        {
            if (_noticeService.IsAutForNoticeUnit(noticeUnitId, _loginUserInfo.Id))
            {
                var noticeUnitVehicleList =
                    _noticeService.GetNoticeUnitAnswerRedirectList(noticeUnitId, _loginUserInfo.Id);
                var groupByUnit = noticeUnitVehicleList.GroupBy(g => g.UnitId)
                    .Select(s => new ENoticeDto()
                    {
                        RedirectAnswerMode = true,
                        NoticeType = s.First().NoticeType,
                        UnitId = s.First().UnitId,
                        ToUnitName = s.First().ToUnitName,
                        PlateList = s.Where(w => w.UnitId == s.First().UnitId).ToList()
                    }).ToList();
                return groupByUnit;
            }

            return null;
        }
        #endregion

        #region NoticeUnit Redirect
        public IActionResult IsRedirectNoticeUnit(int noticeUnitId) => Json(_noticeService.IsRedirectNoticeUnit(noticeUnitId));
        public IActionResult RedirectVehicleList(int noticeUnitId)
        {
            var list = _noticeService.RedirectVehicleList(noticeUnitId);
            var groupByUnit = list.GroupBy(g => g.UnitId)
                .Select(s => new ENoticeDto()
                {
                    RedirectMode = true,
                    ToUnitName = s.First().ToUnitName,
                    PlateList = s.Where(w => w.UnitId == s.First().UnitId).ToList()
                }).ToList();
            var noticeUnit = _noticeService.Find(noticeUnitId);
            var result = noticeUnit.NoticeType switch
            {
                (int)NoticeType.Speed => PartialView("PartialViews/VehicleNotice/Notice/Table/_SpeedTable", groupByUnit),
                (int)NoticeType.OutOfHours => PartialView("PartialViews/VehicleNotice/Notice/Table/_OutOfHoursTable", groupByUnit),
                (int)NoticeType.Duty => PartialView("PartialViews/VehicleNotice/Notice/Table/_DutyTable", groupByUnit),
                _ => null
            };
            return result;
        }
        public IActionResult InsertRedirectNotice(ENoticeUnitDto model)
        {
            model.LoginUserId = _loginUserInfo.Id;
            return Json(_noticeService.InsertRedirectNotice(model));
        }
        #endregion

        #region Notice History
        public IActionResult GetNoticeUnitHistory(int id, string partialPath)
        {
            if (_noticeService.IsAutForNoticeUnit(id, _loginUserInfo.Id))
            {
                var result = _noticeService.GetNoticeUnitHistory(id, _loginUserInfo.Id, _loginUserInfo.IsAdmin);
                result.ForEach(f => f.LoginUserId = _loginUserInfo.Id);
                return PartialView(partialPath, result);
            }
            else
                return PartialView(partialPath, null);
        }
        public IActionResult GetHistoryWithStateVehicleList(ENoticeUnitDto model)
        {
            var list = new List<ENoticeDto>();
            var noticeUnit = _noticeService.Find(model.Id);
            if (model.State == (int)NoticeState.OpenNotice)
            {
                list = GetTypeDateList(new ENoticeUnitDto()
                {
                    Id = model.Id,
                    NoticeType = noticeUnit.NoticeType,
                    StartDate = noticeUnit.StartDate,
                    EndDate = noticeUnit.EndDate
                });
            }
            else if (model.State == (int)NoticeState.AnswerUnit)
                list = GetContentEdit(model.Id);
            else if (model.State == (int)NoticeState.SendUnit || model.State == (int)NoticeState.Closed)
            {
                list = GetUnitHistoryList(model.Id);
                list.ForEach(f =>
                {
                    f.HistoryMode = true;
                    f.EditMode = false;
                    f.RedirectMode = false;
                    f.UnitMode = false;
                    f.RedirectAnswerMode = false;
                });
            }

            var result = noticeUnit.NoticeType switch
            {
                (int)NoticeType.Speed => PartialView("PartialViews/VehicleNotice/Notice/Table/_SpeedTable", list),
                (int)NoticeType.OutOfHours => PartialView("PartialViews/VehicleNotice/Notice/Table/_OutOfHoursTable", list),
                (int)NoticeType.Duty => PartialView("PartialViews/VehicleNotice/Notice/Table/_DutyTable", list),
                _ => null
            };
            return result;
        }
        public List<ENoticeDto> GetUnitHistoryList(int noticeUnitId)
        {
            if (_noticeService.IsAutForNoticeUnit(noticeUnitId, _loginUserInfo.Id))
            {
                var noticeUnitVehicleList =
                    _noticeService.GetNoticeHistoryResultList(noticeUnitId, _loginUserInfo.Id);
                var groupByUnit = noticeUnitVehicleList.GroupBy(g => g.UnitId)
                    .Select(s => new ENoticeDto()
                    {
                        RedirectAnswerMode = true,
                        NoticeType = s.First().NoticeType,
                        UnitId = s.First().UnitId,
                        ToUnitName = s.First().ToUnitName,
                        PlateList = s.Where(w => w.UnitId == s.First().UnitId).ToList()
                    }).ToList();
                return groupByUnit;
            }

            return null;
        }
        #endregion
    }
}
