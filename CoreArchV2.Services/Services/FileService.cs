using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class FileService : IFileService
    {
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IGenericRepository<VehiclePhysicalImageFile> _vehiclePhysicalImageFileGenericRepository;
        private readonly IGenericRepository<VehiclePhysicalImage> _vehiclePhysicalImageGenericRepository;
        private readonly IUnitOfWork _uow;
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        private readonly IHostingEnvironment _env;


        public FileService(IUnitOfWork uow,
            IHostingEnvironment env)
        {
            _uow = uow;
            _env = env;
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _vehiclePhysicalImageFileGenericRepository = uow.GetRepository<VehiclePhysicalImageFile>();
            _vehiclePhysicalImageGenericRepository = uow.GetRepository<VehiclePhysicalImage>();
        }

        public string FormatSize(long bytes)
        {
            var counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }

            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        //Transaction anında yüklenen dosyaları siler

        public void FileUploadWithTransactionDelete(int[] ids, string[] idNames, string folderName)
        {
            try
            {
                if (ids.Length > 0)
                    foreach (var item in ids)//db'ye kaydetmişse
                    {
                        var fileUpload = _fileUploadRepository.Find(item);
                        if (fileUpload != null)
                        {
                            if (File.Exists(GetPathAndFileName(fileUpload.Name, folderName)))
                                try
                                {
                                    File.Delete(GetPathAndFileName(fileUpload.Name, folderName));
                                }
                                catch (Exception ex) { }
                            _fileUploadRepository.Delete(fileUpload);
                            _uow.SaveChanges();
                        }
                    }

                if (idNames.Length > 0)
                    foreach (var item in idNames)//db'ye kaydetmeden klasöre kaydetmişse
                    {
                        if (File.Exists(GetPathAndFileName(item, folderName)))
                            try
                            {
                                File.Delete(GetPathAndFileName(item, folderName));
                            }
                            catch (Exception ex) { }
                    }
            }
            catch (Exception)
            {
            }
        }

        private string GetPathAndFileName(string filename, string folderName)
        {
            string path = Path.Combine(_env.WebRootPath, folderName);
            //var path = _env.WebRootPath + "\\uploads\\logistics\\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + filename;
        }

        #region Logistics


        public EResultDto FileDeleteLogistics(int fileUploadId)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope())
                {
                    //FileUpload Table delete
                    var fileUpload = _fileUploadRepository.FindForInsertUpdateDelete(fileUploadId);
                    _fileUploadRepository.Delete(fileUpload);
                    _uow.SaveChanges();//Bir sorun olursa alt satıra geçme hata fırlat

                    //local file delete
                    if (File.Exists(GetPathAndFileName(fileUpload.Name, "uploads/logistics/")))
                        File.Delete(GetPathAndFileName(fileUpload.Name, "uploads/logistics/"));

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }
            return result;
        }



        public EResultDto FileDeletePhysicalImage(int fileUploadId)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope())
                {
                    var vehiclePhysicalImageId = _vehiclePhysicalImageFileGenericRepository.FirstOrDefault(w => w.FileUploadId == fileUploadId).VehiclePhysicalImageId;

                    //FileUpload Table delete
                    var fileUpload = _fileUploadRepository.FindForInsertUpdateDelete(fileUploadId);
                    _fileUploadRepository.Delete(fileUpload);
                    _uow.SaveChanges();//Bir sorun olursa alt satıra geçme hata fırlat

                    //local file delete
                    if (File.Exists(GetPathAndFileName(fileUpload.Name, "uploads/physicalimage/")))
                        File.Delete(GetPathAndFileName(fileUpload.Name, "uploads/physicalimage/"));

                    var subFiles = _vehiclePhysicalImageFileGenericRepository.Where(w => w.VehiclePhysicalImageId == vehiclePhysicalImageId).ToList();
                    var file = _vehiclePhysicalImageGenericRepository.FirstOrDefault(w => w.Id == vehiclePhysicalImageId);

                    if (subFiles.Count == 0)
                    {
                        _vehiclePhysicalImageGenericRepository.Delete(file);
                        _uow.SaveChanges();
                    }

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }
            return result;
        }



        public EResultDto FileUploadInsertLogistics(IList<IFormFile> files)
        {
            var result = new EResultDto();
            result.Ids = new int[files.Count];
            result.IdNames = new string[files.Count];
            try
            {
                var count = 0;
                foreach (var source in files)
                {
                    var originFileName = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.Replace(" ", "").Trim('"');
                    originFileName = Regex.Replace(originFileName, "[@&'(\\s)<>#]", "");
                    var filename = Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "") + "-" + originFileName;
                    var extention = Path.GetExtension(source.FileName).Replace(".", string.Empty).ToLower();
                    var fileUpload = _fileUploadRepository.Insert(new FileUpload
                    { Name = filename, Extention = extention, FileSize = source.Length });
                    _uow.SaveChanges();
                    using (var output = File.Create(GetPathAndFileName(filename, "uploads/logistics/")))
                    {
                        source.CopyTo(output);
                    }

                    result.Ids[count] = fileUpload.Id;
                    result.IdNames[count] = filename;
                    count++;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Dosya yüklemede hata oluştu!";
                FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/logistics/"); //Hata aldıysa yüklenen dosyaları sil
            }

            return result;
        }


        public EResultDto FileUploadInsertVehiclePhysicalImage(IList<IFormFile> files, string fileName)
        {
            var result = new EResultDto();

            //var formFileList = Base64ToFormFile(files);
            result.Ids = new int[files.Count];
            result.IdNames = new string[files.Count];
            try
            {
                var count = 0;
                foreach (var source in files)
                {
                    var originFileName = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.Replace(" ", "").Trim('"');
                    originFileName = Regex.Replace(originFileName, "[@&'(\\s)<>#]", "");
                    var extention = System.IO.Path.GetExtension(originFileName);
                    fileName += extention;

                    var fileUpload = _fileUploadRepository.Insert(new FileUpload
                    { Name = fileName, Extention = extention, FileSize = source.Length });
                    _uow.SaveChanges();

                    using (var output = File.Create(GetPathAndFileName(fileName, "uploads/physicalimage/")))
                    {
                        source.CopyTo(output);
                    }

                    result.Ids[count] = fileUpload.Id;
                    result.IdNames[count] = fileName;
                    count++;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Dosya yüklemede hata oluştu!";
                FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/physicalimage/"); //Hata aldıysa yüklenen dosyaları sil
            }

            return result;
        }

        public IFormFile Base64ToFormFile(string equipmentFiles, string fileName)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(equipmentFiles);
            var stream = new MemoryStream(bytes);

            var file = new FormFile(stream, 0, bytes.Length, fileName, fileName);
            return file;
        }

        #endregion

        #region Tender


        public EResultDto FileDeleteTender(int fileUploadId)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope())
                {
                    //FileUpload Table delete
                    var fileUpload = _fileUploadRepository.FindForInsertUpdateDelete(fileUploadId);
                    _fileUploadRepository.Delete(fileUpload);
                    _uow.SaveChanges();//Bir sorun olursa alt satıra geçme hata fırlat

                    //local file delete
                    if (File.Exists(GetPathAndFileName(fileUpload.Name, "uploads/tender/")))
                        File.Delete(GetPathAndFileName(fileUpload.Name, "uploads/tender/"));

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }
            return result;
        }

        public EResultDto FileUploadInsertTender(IList<IFormFile> files)
        {
            var result = new EResultDto();
            result.Ids = new int[files.Count];
            result.IdNames = new string[files.Count];
            try
            {
                var count = 0;
                foreach (var source in files)
                {
                    var originFileName = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.Replace(" ", "").Trim('"');
                    originFileName = Regex.Replace(originFileName, "[@&'(\\s)<>#]", "");
                    var filename = Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "") + "-" + originFileName;
                    var extention = Path.GetExtension(source.FileName).Replace(".", string.Empty).ToLower();
                    var fileUpload = _fileUploadRepository.Insert(new FileUpload
                    { Name = filename, Extention = extention, FileSize = source.Length });
                    _uow.SaveChanges();
                    using (var output = File.Create(GetPathAndFileName(filename, "uploads/tender/")))
                    {
                        source.CopyTo(output);
                    }

                    result.Ids[count] = fileUpload.Id;
                    result.IdNames[count] = filename;
                    count++;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Dosya yüklemede hata oluştu!";
                FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/tender/"); //Hata aldıysa yüklenen dosyaları sil
            }
            return result;
        }

        #endregion

        #region Notice

        public EResultDto FileUploadInsertNoticeUnit(IList<IFormFile> files)
        {
            var result = new EResultDto();
            result.Ids = new int[files.Count];
            result.IdNames = new string[files.Count];
            try
            {
                var count = 0;
                foreach (var source in files)
                {
                    var originFileName = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.Replace(" ", "").Trim('"');
                    originFileName = Regex.Replace(originFileName, "[@&'(\\s)<>#]", "");
                    var filename = Guid.NewGuid().ToString().Replace("-", "").Replace(" ", "") + "-" + originFileName;
                    var extention = Path.GetExtension(source.FileName).Replace(".", string.Empty).ToLower();
                    var fileUpload = _fileUploadRepository.Insert(new FileUpload
                    { Name = filename, Extention = extention, FileSize = source.Length });
                    _uow.SaveChanges();
                    using (var output = File.Create(GetPathAndFileName(filename, "uploads/Notice/NoticeUnit/")))
                    {
                        source.CopyTo(output);
                    }

                    result.Ids[count] = fileUpload.Id;
                    result.IdNames[count] = filename;
                    count++;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Dosya yüklemede hata oluştu!";
                FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/Notice/NoticeUnit/"); //Hata aldıysa yüklenen dosyaları sil
            }

            return result;
        }

        public EResultDto FileDeleteNoticeUnit(int fileUploadId)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope())
                {
                    //FileUpload Table delete
                    var fileUpload = _fileUploadRepository.FindForInsertUpdateDelete(fileUploadId);
                    _fileUploadRepository.Delete(fileUpload);
                    _uow.SaveChanges();//Bir sorun olursa alt satıra geçme hata fırlat

                    //local file delete
                    if (File.Exists(GetPathAndFileName(fileUpload.Name, "uploads/Notice/NoticeUnit/")))
                        File.Delete(GetPathAndFileName(fileUpload.Name, "uploads/Notice/NoticeUnit/"));

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }
            return result;
        }

        #endregion


    }
}