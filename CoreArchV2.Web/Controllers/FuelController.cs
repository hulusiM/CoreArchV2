using AutoMapper;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;

namespace CoreArchV2.Web.Controllers
{
    public class FuelController : AdminController
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IFuelLogService _fuelLogService;
        private readonly IVehicleService _vehicleService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;


        public FuelController(IMapper mapper,
            IFuelLogService fuelLogService,
            IVehicleService vehicleService,
            IWebHostEnvironment hostingEnvironment,
            IUnitOfWork uow)
        {
            _mapper = mapper;
            _uow = uow;
            _hostingEnvironment = hostingEnvironment;
            _fuelLogService = fuelLogService;
            _vehicleService = vehicleService;
        }

        public IActionResult Index() => View();

        public IActionResult FuelLogGetAll(int? page, EFuelLogDto filterModel)
        {
            var result = _fuelLogService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Fuel/FuelLogGetAll"));
            return Json(result);
        }

        public async Task<IActionResult> InsertUpdate(EFuelLogDto tempModel)
        {
            var result = new EResultDto();
            if (tempModel.TotalAmount > 0)
            {
                if (tempModel.Id > 0)
                    result = await _fuelLogService.UpdateAsync(tempModel);
                else
                {
                    tempModel.CreatedBy = (int)HttpContext.Session.GetInt32("UserId");
                    result = await _fuelLogService.InsertAsync(tempModel);
                }
            }
            else
            {
                result.Message = "Tutar kısmı boş geçilemez";
                result.IsSuccess = false;
            }

            return Json(result);
        }

        public IActionResult Delete(int id) => Json(_fuelLogService.Delete(id));

        public IActionResult GetById(int id) => Json(_fuelLogService.GetById(id));

        public IActionResult GetPublishFuel(FuelPublisher publisher) => Json(_fuelLogService.GetPublishFuel(publisher));

        [HttpPost]
        public IActionResult SetPublishFuel(EFuelLogDto model) => Json(_fuelLogService.SetPublishFuel(model));

        //Toplu yakıt girişi verileri dataTable'da gösteriliyor

        public IActionResult ReadExcelForFuel(DateTime fuelDate) => Json(ReadExcel(fuelDate, true).Result);

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

        public async Task<EFuelReadExcelDto> ReadExcel(DateTime fuelDate, bool isHtmlPrint = false)
        {
            IFormFile file = Request.Form.Files[0];
            string folderName = "tempFile";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            string fullPath = Path.Combine(newPath, file.FileName);
            DeleteExcelFile(newPath);
            var result = new EFuelReadExcelDto();
            result.FuelList = new List<EFuelLogDto>();
            try
            {
                ISheet sheet;
                if (file.Length > 0)
                {
                    string sFileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!Directory.Exists(newPath))
                        Directory.CreateDirectory(newPath);

                    using (var stream = new FileStream(fullPath, FileMode.Create)) //Okunan excel kaydedildi altta silinecek
                    {
                        file.CopyTo(stream);
                        stream.Position = 0;
                        if (sFileExtension == ".xls")
                        {
                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                            sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                        }
                        else
                        {
                            XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                            sheet = hssfwb.GetSheetAt(0);
                        }
                        IRow headerRow = sheet.GetRow(0);
                        int cellCount = headerRow.LastCellNum;
                        var list = new List<EFuelLogDto>();
                        for (int i = (sheet.FirstRowNum); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;
                            if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                            var plate = row.GetCell(0) != null ? row.GetCell(0).ToString().ToUpper().Replace(" ", "") : "";
                            var amount = row.GetCell(1) != null ? Convert.ToDecimal(row.GetCell(1).ToString().Replace(" ", "")) : 0;
                            if (!string.IsNullOrEmpty(plate) && amount > 0)
                            {
                                //Yorum satırı tarih düzenlemesi için
                                //var changeDivText = row.GetCell(2).ToString();
                                //if (changeDivText.Contains("/"))
                                //    changeDivText = row.GetCell(2).ToString().Replace("/", ".").ToString();
                                //var date = row.GetCell(2) != null ? Convert.ToDateTime(changeDivText) : DateTime.MinValue;

                                //var arventoKmS = row.GetCell(2) == null ? "" : row.GetCell(2).ToString();
                                //decimal arventoKm = 0;
                                //if (!string.IsNullOrEmpty(arventoKmS) && arventoKmS != null)
                                //    arventoKm = Convert.ToDecimal(arventoKmS.Replace(" ", ""));

                                list.Add(new EFuelLogDto()
                                {
                                    Plate = plate,
                                    TransactionDate = fuelDate,
                                    TotalAmount = amount,
                                    //Km = arventoKm
                                });
                            }
                        }

                        //bir plaka birden fazla yakıt aldıysa
                        var groupByDate = list.GroupBy(g => new
                        {
                            g.Plate,
                            //g.TransactionDate
                        }).Select(s => new EFuelLogDto()
                        {
                            Plate = s.First().Plate,
                            TransactionDate = s.First().TransactionDate,
                            TotalAmount = s.Sum(s => s.TotalAmount),
                            //Km = s.Sum(s => s.Km),
                            ListCount = s.Count()
                        }).ToList();

                        //listeki plakalar sistemde var mı kontrol edilecek
                        var plateList = _vehicleService.GetAllVehicleList();
                        string[] html = new string[2];
                        int counter_true = 1;
                        int counter_false = 1;
                        foreach (var pl in groupByDate)
                        {
                            var vehicle = plateList.FirstOrDefault(w => w.Plate == pl.Plate);
                            if (vehicle != null)
                            {
                                var vehicleDebit = await _vehicleService.GetVehicleDebitRangeByPlate(pl.Plate, pl.TransactionDate); //plaka ve yakıt tarihinde zimmeti var mı 
                                if (vehicleDebit == null)//girilen yakıt zimmet aralığında değil
                                {
                                    html[1] += SetExcelRow_False(pl, counter_false, " <b style='color:red;'>(Yakıt tarihi zimmet aralığında değil)</b>");
                                    counter_false++;
                                }
                                else//eklenebilir
                                {
                                    pl.VehicleId = vehicleDebit.VehicleId;
                                    result.FuelList.Add(pl);
                                    html[0] += SetExcelRow_True(pl, counter_true);
                                    counter_true++;
                                }
                            }
                            else//sistemde plaka yok
                            {
                                html[1] += SetExcelRow_False(pl, counter_false, " <b style='color:blue;'>(Plaka sistemde bulunamadı)</b>");
                                counter_false++;
                            }
                        }

                        html[0] += "<tr><td class='text-right' colspan='5'><span id='totalAmount_True' class='label bg-orange-300'><b style='color: black;'>Toplam: " + (result.FuelList.Sum(s => s.TotalAmount)).ToString("#,##0.00") + " ₺</b></span></td></tr>";
                        html[1] += "<tr><td class='text-right' colspan='5'><span id='totalAmount_False' class='label bg-orange-300'><b style='color: black;'>Toplam: " + (groupByDate.Sum(s => s.TotalAmount) - result.FuelList.Sum(s => s.TotalAmount)).ToString("#,##0.00") + " ₺</b></span></td></tr>";
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

                return result;
            }
            return result;
        }

        public string SetExcelRow_True(EFuelLogDto item, int counter_true)
        {
            StringBuilder sb = new StringBuilder();
            sb.Length = 0;
            sb.AppendLine("<tr>");
            sb.Append("<td>" + counter_true + "</td>");
            sb.Append("<td style='font-weight: bold;'>" + (item.ListCount > 1 ? (item.Plate + " (" + item.VehicleId + ") -<span class='badge bg-orange-300'>" + item.ListCount + " kere yakıt alınmış</span>") : (item.Plate + " (" + item.VehicleId + ")")) + "</td>");
            sb.Append("<td><span class='label bg-orange-300 full-width'>" + item.TotalAmount.ToString("#,##0.00") + " ₺</span></td>");
            sb.AppendLine("</tr>");
            return sb.ToString();
        }

        public string SetExcelRow_False(EFuelLogDto item, int counter_false, string desc)//sistemde yok
        {
            StringBuilder sb = new StringBuilder();
            sb.Length = 0;
            sb.AppendLine("<tr>");
            sb.Append("<td>" + counter_false + "</td>");
            sb.Append("<td style='font-weight: bold;'>" + item.Plate + " " + desc + "</td>");
            sb.Append("<td><span class='label bg-orange-300 full-width'>" + item.TotalAmount.ToString("#,##0.00") + " ₺</span></td>");
            sb.AppendLine("</tr>");

            return sb.ToString();
        }


        public async Task<IActionResult> ReadExcelForInsertFuel(int fuelStationId, decimal discountPercent, DateTime fuelDate)
        {
            var model = new EFuelReadExcelDto()
            {
                FuelList = ReadExcel(fuelDate, false).Result.FuelList,
                UserName = _loginUserInfo.FullName,
                DiscountPercent = discountPercent,
                FuelStationId = fuelStationId,
                CreatedBy = _loginUserInfo.Id
            };
            var result = await _fuelLogService.InsertBulkAsync(model);
            return Json(result);
        }

    }
}