using AutoMapper;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class FileController : AdminController
    {
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;



        public FileController(IMapper mapper,
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IFileService fileService,
            IMaintenanceService maintenanceService)
        {
            _mapper = mapper;
            _uow = uow;
            _env = env;
            _fileService = fileService;
            _maintenanceService = maintenanceService;
            _fileUploadRepository = uow.GetRepository<FileUpload>();
        }

        /// <summary>
        ///     FileUpload tablosunu siler. Silinecek her tabloya Cascade bağlanması gerekir!!!
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IActionResult FileDeleteLogistics(int key)
        {
            if (_loginUserInfo.IsAdmin)
            {
                var fileUploadId = key;
                return Json(_fileService.FileDeleteLogistics(fileUploadId));
            }
            else
                return Json(false);
        }

        public IActionResult FileDeletePhysicalImage(int key)
        {
            if (_loginUserInfo.IsAdmin)
            {
                var fileUploadId = key;
                return Json(_fileService.FileDeletePhysicalImage(fileUploadId));
            }
            else
                return Json(false);
        }


        public IActionResult FileDeleteTender(int key)
        {
            if (_loginUserInfo.IsAdmin)
            {
                var fileUploadId = key;
                return Json(_fileService.FileDeleteTender(fileUploadId));
            }
            else
                return Json(false);
        }

        [HttpGet]

        public async Task<IActionResult> FileDownloadLogistics(int key)
        {
            var fileName = await _fileUploadRepository.FindAsync(key);
            string path = Path.Combine(_env.WebRootPath, "uploads/logistics/") + fileName.Name;
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, "application/octet-stream", fileName.Name);
        }

        [HttpGet]

        public async Task<IActionResult> FileDownloadUserPhysicalImage(int key)
        {
            var fileName = await _fileUploadRepository.FindAsync(key);
            string path = Path.Combine(_env.WebRootPath, "uploads/physicalimage/") + fileName.Name;
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, "application/octet-stream", fileName.Name);
        }

        public IActionResult FileDeleteNoticeUnit(int key)
        {
            if (_loginUserInfo.IsAdmin)
            {
                var fileUploadId = key;
                return Json(_fileService.FileDeleteNoticeUnit(fileUploadId));
            }
            else
                return Json(false);
        }

        [HttpGet]

        public async Task<IActionResult> FileDownloadNoticeUnit(int key)
        {
            var fileName = await _fileUploadRepository.FindAsync(key);
            string path = Path.Combine(_env.WebRootPath, "uploads/Notice/NoticeUnit/") + fileName.Name;
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, "application/octet-stream", fileName.Name);
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"}
            };
        }
    }
}