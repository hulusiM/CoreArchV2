using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.Tender;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ETenderDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.SignalR;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Globalization;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class TenderService : ITenderService
    {
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IGenericRepository<TenderFile> _tenderFileRepository;
        private readonly IGenericRepository<TenderDetailFile> _tenderDetailFileRepository;
        private readonly IGenericRepository<TenderDetail> _tenderDetailRepository;
        private readonly IGenericRepository<Tender> _tenderRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IGenericRepository<Institution> _institutionRepository;
        private readonly IGenericRepository<TenderContact> _tenderContactRepository;
        private readonly IGenericRepository<TenderHistory> _tenderHistoryRepository;
        private readonly IGenericRepository<TenderDetailPriceHistory> _tenderDetailPriceHistoryRepository;
        private readonly IMessageService _messageService;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IHostingEnvironment _env;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;


        public TenderService(IUnitOfWork uow,
            IMessageService messageService,
            IMapper mapper,
            IHubContext<SignalRHub> hubContext,
            IHostingEnvironment env)
        {
            _uow = uow;
            _env = env;
            _mapper = mapper;
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _tenderRepository = uow.GetRepository<Tender>();
            _tenderFileRepository = uow.GetRepository<TenderFile>();
            _tenderDetailFileRepository = uow.GetRepository<TenderDetailFile>();
            _tenderDetailRepository = uow.GetRepository<TenderDetail>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _userRepository = uow.GetRepository<User>();
            _institutionRepository = uow.GetRepository<Institution>();
            _tenderContactRepository = uow.GetRepository<TenderContact>();
            _tenderHistoryRepository = uow.GetRepository<TenderHistory>();
            _tenderDetailPriceHistoryRepository = uow.GetRepository<TenderDetailPriceHistory>();
            _unitRepository = uow.GetRepository<Unit>();
            _messageService = messageService;
            _hubContext = hubContext;
        }

        public PagedList<ETender_Dto> GetAllWithPaged(int? page, ETender_Dto filterModel, bool isAdmin)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from t in _tenderRepository.GetAll()
                            //join lp in _lookUpListRepository.GetAll() on t.ProjectDetailId equals lp.Id
                        select new ETender_Dto
                        {
                            Id = t.Id,
                            Status = t.Status,
                            State = t.State,
                            CreatedUnitId = t.CreatedUnitId,
                            SalesNumber = "<a onclick='funcEditTender(" + t.Id + ");' class='text-bold' style='font-size: 11px;'>" + t.SalesNumber + "</a>",
                            Name = "<span class='label bg-primary-300 full-width'>" + t.Name + "</span>",
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            DeleteButtonActive = true,
                            CustomButton = "<li title='Düzenle' class='text-primary-400'><a data-toggle='modal' onclick='funcEditTender(" + t.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                                           "<li title='Excel' class='text-orange-400'><a data-toggle='modal' onclick='exportExcel(" + t.Id + ");'><i class='icon-file-excel'></i></a></li>" +
                                           "<li title='Durum değiştir' class='text-slate-400'><a data-toggle='modal' onclick='nextStepTender(" + t.Id + ");'><i class='icon-next text-danger'></i></a></li>"
                        });

            if (!isAdmin)
                list = list.Where(w => w.CreatedUnitId == filterModel.CreatedUnitId);

            if (filterModel.ProjectDetailId > 0)
                list = list.Where(w => w.ProjectDetailId == filterModel.ProjectDetailId);

            var result = new PagedList<ETender_Dto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            foreach (var item in result)
            {
                item.TenderStateName = StateTender(item.State);
                if (!item.Status)
                    item.TenderStateName += " (Silinmiş)";
            }
            return result;
        }

        public PagedList<ETender_Dto> GetAllForUnitWithPaged(int? page, ETender_Dto filterModel, bool isAdmin)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from t in _tenderRepository.GetAll()
                        join u in _unitRepository.GetAll() on t.CreatedUnitId equals u.Id
                        join td in _tenderDetailRepository.GetAll() on t.Id equals td.TenderId
                        where t.Status && td.Status
                        select new ETender_Dto
                        {
                            Id = t.Id,
                            State = t.State,
                            CreatedDate = t.CreatedDate,
                            CreatedUnitId = t.CreatedUnitId,
                            UnitName = u.Name,
                            UnitId = td.UnitId,
                            SalesNumber = t.SalesNumber,
                            Name = "<span class='label bg-primary-300 full-width'>" + t.Name + "</span>",
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            CustomButton = "<li class='text-orange-800'><a data-toggle='modal' onclick='funcUnitGetPrice(" + t.Id + ");'><i class='icon-pencil5'></i></a></li>"
                        });

            if (!isAdmin)
                list = list.Where(w => w.UnitId == filterModel.CreatedUnitId);

            if (filterModel.ProjectDetailId > 0)
                list = list.Where(w => w.ProjectDetailId == filterModel.ProjectDetailId);

            //sadece tender kalmalı ki distinct yapılsın
            list = list.Select(s => new ETender_Dto()
            {
                Id = s.Id,
                State = s.State,
                CreatedDate = s.CreatedDate,
                SalesNumber = s.SalesNumber,
                UnitName = s.UnitName,
                Name = s.Name,
                PageStartCount = s.PageStartCount,
                CustomButton = s.CustomButton
            });
            var result = new PagedList<ETender_Dto>(list.Distinct(), page, PagedCount.GridKayitSayisi);
            foreach (var item in result)
            {
                var tenderDetail = _tenderDetailRepository.Where(w => w.TenderId == item.Id && w.Status).ToList();
                if (!isAdmin) //admin değilse sadece kendi satırlarını görsün
                {
                    var createdUnitId = _tenderRepository.Find(item.Id).CreatedUnitId;//kendi oluşturduğu ihale detaylarını ve adminse tümünü görebilir
                    if (createdUnitId != filterModel.CreatedUnitId)
                        tenderDetail = tenderDetail.Where(w => w.UnitId == filterModel.CreatedUnitId).ToList();
                }
                var total = tenderDetail.Count;
                var wait = tenderDetail.Count(w => w.ProductPrice == null);
                var state = item.State != (int)TenderState_.Draft
                    ? ("<span class='label bg-danger-300 full-width'>İşleme Kapalı</span>")
                    : (wait > 0
                        ? "<span class='label bg-orange-300 full-width'>Devam Ediyor</span>"
                        : "<span class='label bg-success-300 full-width'>Tamamlandı</span>");
                item.Description = total + "/" + wait + " Adet";
                item.TenderStateName = state;
                item.WaitRequest = wait;
            }
            return new PagedList<ETender_Dto>(result.OrderByDescending(o => o.State).ThenByDescending(o => o.WaitRequest).ThenByDescending(t => t.CreatedDate).ToList(), page, PagedCount.GridKayitSayisi);
        }

        #region Tender

        public EResultDto InsertTender(IList<IFormFile> files, ETender_Dto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var loginUserUnit = _userRepository.Find(tempModel.CreatedBy);
                var isSalesNumber = _tenderRepository.Any(a => a.SalesNumber == tempModel.SalesNumber);
                if (loginUserUnit.UnitId == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Kullanıcının birimi olmadığından ihale açamaz.";
                }
                else if (!isSalesNumber)
                {
                    var salesNumber = CreateNewSalesNumber(tempModel.CreatedBy);
                    if (salesNumber.IsSuccess)
                    {
                        using (var scope = new TransactionScope())
                        {
                            var model = _mapper.Map<Tender>(tempModel);
                            model.SalesNumber = salesNumber.Message;
                            model.State = (int)TenderState_.Draft;//İlk kayıt aşaması taslak
                            model.CreatedBy = tempModel.CreatedBy;
                            model.CreatedUnitId = loginUserUnit.UnitId.Value;
                            var resultEntity = _tenderRepository.Insert(model);
                            _uow.SaveChanges();

                            if (files.Count > 0)
                            {
                                result = FileTenderInsert(files, resultEntity.Id);
                                if (!result.IsSuccess)
                                    return result;
                            }

                            scope.Complete();
                            result.Id = resultEntity.Id;
                            result.Message = "<b>" + resultEntity.SalesNumber + "</b> numarasıyla teklif kayıt altına alınmıştır";
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Satış numarası oluştururken hata oluştu";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Bu satış numarası zaten var";
                }
            }
            catch (Exception ex)
            {
                var fs = new FileService(_uow, _env);
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/tender/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }


        public EResultDto UpdateTender(IList<IFormFile> files, ETender_Dto entity)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope())
                {
                    var oldEntity = _tenderRepository.Find(entity.Id);
                    var newEntity = _mapper.Map(entity, oldEntity);
                    _tenderRepository.Update(newEntity);
                    _uow.SaveChanges();

                    if (files.Count > 0)
                    {
                        result = FileTenderInsert(files, newEntity.Id);
                        if (!result.IsSuccess)
                            return result;
                    }

                    scope.Complete();
                    result.Id = entity.Id;
                    result.Message = "Güncelleme işlemi başarılı";
                }
            }
            catch (Exception ex)
            {
                var fs = new FileService(_uow, _env);
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/tender/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }

        public ETender_Dto GetByIdTender(int id)
        {
            var result = _tenderRepository.Find(id);
            var entityDto = _mapper.Map<ETender_Dto>(result);

            var files = (from t in _tenderRepository.GetAll()
                         join tf in _tenderFileRepository.GetAll() on t.Id equals tf.TenderId
                         join fu in _fileUploadRepository.GetAll() on tf.FileUploadId equals fu.Id
                         where t.Id == id
                         select new EFileUploadDto
                         {
                             Id = fu.Id,
                             TenderId = t.Id,
                             Name = fu.Name,
                             Extention = fu.Extention,
                             FileSize = fu.FileSize,
                             TenderFileId = tf.Id,
                         }).ToList();

            entityDto.files = files;
            return entityDto;
        }

        public EResultDto DeleteTender(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _tenderRepository.Find(id);
                entity.Status = Convert.ToBoolean(Status.Passive);
                _tenderRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }
            return result;
        }

        public EResultDto CreateNewSalesNumber(int userId)
        {
            var result = new EResultDto();
            try
            {
                var user = _userRepository.Find(userId);
                if (user.UnitId > 0)
                {
                    var lastUnitSalesNumber = _tenderRepository.Where(w => w.CreatedUnitId == user.UnitId).OrderByDescending(o => o.Id).FirstOrDefault();
                    var date = DateTime.Now;
                    if (lastUnitSalesNumber != null)
                    {
                        var lastSalesNumber = lastUnitSalesNumber.SalesNumber;
                        var splitNo = lastSalesNumber.Split('-')[1];
                        var newNo = Convert.ToInt32(splitNo) + 1;
                        result.Message = date.ToString("yy") + "" + date.Month + "." + user.UnitId + "-" + newNo;
                    }
                    else//ilk ihale
                        result.Message = date.ToString("yy") + "" + DateFormat(date.Month) + "." + user.UnitId + "-" + "1";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Kullanıcının birimi olmadığından ihale açamaz.";
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Satış numarası oluşturulamadı.";
            }
            return result;
        }
        #endregion

        #region Tender Contact
        public EResultDto InsertUpdateTenderContract(ETenderAllDto model)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope())
                {
                    //Eski kayıtlar siliniyor
                    var oldContactList = _tenderContactRepository.Where(w => w.TenderId == model.TenderId).ToList();
                    _tenderContactRepository.DeleteRange(oldContactList);

                    //Yeni kayıtlar ekleniyor
                    var tenderId = model.TenderId;
                    foreach (var item in model.ContactList)
                        _tenderContactRepository.Insert(new TenderContact()
                        {
                            CreatedBy = model.CreatedBy,
                            TenderId = tenderId,
                            Name = item.Name,
                            Surname = item.Surname,
                            Phone = item.Phone,
                            Email = item.Email
                        });
                    _uow.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }
            return result;
        }

        public List<TenderContact> GetByTenderIdContactList(int tenderId) =>
            _tenderContactRepository.Where(w => w.Status && w.TenderId == tenderId).ToList();

        public List<ETenderHistoryDto> GetByIdTenderHistory(int tenderId)
        {
            var list = (from th in _tenderHistoryRepository.GetAll()
                        join u in _userRepository.GetAll() on th.CreatedBy equals u.Id
                        join t in _tenderRepository.GetAll() on th.TenderId equals t.Id
                        join l in _lookUpListRepository.GetAll() on t.ProjectDetailId equals l.Id
                        join i in _institutionRepository.GetAll() on th.InstitutionId equals i.Id into iL
                        from i in iL.DefaultIfEmpty()
                        where th.TenderId == tenderId
                        select new ETenderHistoryDto()
                        {
                            InstitutionName = i.Name,
                            ProjectTypeName = l.Name,
                            State = th.State,
                            TransactionDate = th.TransactionDate,
                            NameSurname = u.Name + " " + u.Surname,
                            ObjectTenderDetailModel = th.ObjectTenderDetailModel
                        }).ToList();

            return SetStateTenderList(list);
        }
        #endregion

        #region TenderDetail

        public EResultDto InsertUpdateTenderDetail(ETenderAllDto tempModel, int loginUnitId)
        {
            var result = new EResultDto { IsSuccess = false };
            try
            {
                var tender = _tenderRepository.Find(tempModel.TenderId);
                if (tender != null)
                {
                    var oldTenderDetailList = _tenderDetailRepository.Where(w => w.TenderId == tempModel.TenderId).ToList();
                    var tenderDetailFileList = new List<FileUpload>();
                    using (var scope = new TransactionScope())
                    {
                        tender.FooterInfo = tempModel.Tender.FooterInfo; //tender Update
                        if (oldTenderDetailList.Any())//kuruma gönderildiyse revize log kaydı
                        {
                            var newTenderDetailList = _mapper.Map<List<TenderDetail>>(tempModel.TenderDetailList);
                            if (tender.State != (int)TenderState_.Draft) //Taslak halinde değilse log kaydı at
                            {
                                tender.State = (int)TenderState_.Revise;
                                tender.ChangedCount = (tender.ChangedCount ?? 0) + 1;
                                //TenderHistory
                                var tenderDetailHistory = new TenderHistory()
                                {
                                    CreatedBy = tempModel.CreatedBy,
                                    State = (int)TenderState_.Revise,
                                    TenderId = tempModel.TenderId,
                                    ObjectTenderDetailModel = JsonConvert.SerializeObject(oldTenderDetailList).ToString(),
                                };
                                _tenderHistoryRepository.Insert(tenderDetailHistory);
                            }

                            //TenderDetail edit/delete
                            newTenderDetailList.ForEach(f => { f.TenderId = tempModel.TenderId; f.CreatedBy = tempModel.CreatedBy; });
                            foreach (var item in newTenderDetailList)
                            {
                                var oldValue = oldTenderDetailList.FirstOrDefault(w => w.Id == item.Id);
                                if (oldValue == null) //yeni kayıt
                                {
                                    var tendEntity = _tenderDetailRepository.Insert(item);
                                    _uow.SaveChanges();
                                    item.Id = tendEntity.Id;
                                }
                                else//eski kayıt güncelle
                                {
                                    var ent = _mapper.Map(item, oldValue);
                                    _tenderDetailRepository.Update(ent);
                                    oldTenderDetailList.Remove(oldValue);
                                }
                                InsertTenderDetailPriceHistory(item, loginUnitId);//ilk oluşturan ürün fiyatı girdiyse log at
                            }

                            //listede olmayan kayıtlar siliniyor
                            if (oldTenderDetailList.Count > 0)
                            {
                                TenderDetailFileDeleteLocal(GetTenderDetailFileNameList(oldTenderDetailList));//lokaldeki dosyaları silmek için
                                TenderDetailFileDelete(oldTenderDetailList);//fileUpload tablosundaki kayıtları sil
                                oldTenderDetailList.ForEach(f => f.Status = false);//eski kayıtları pasife çek
                                _tenderDetailRepository.UpdateRange(oldTenderDetailList);
                            }
                        }
                        else//yeni kayıt
                        {
                            //TenderHistory insert
                            var tenderDetailHistory = new TenderHistory()
                            {
                                CreatedBy = tempModel.CreatedBy,
                                State = (int)TenderState_.Draft,
                                TenderId = tempModel.TenderId
                            };
                            _tenderHistoryRepository.Insert(tenderDetailHistory);

                            //TenderDetail insert
                            var entList = _mapper.Map<List<TenderDetail>>(tempModel.TenderDetailList);
                            entList.ForEach(f => { f.CreatedBy = tempModel.CreatedBy; f.TenderId = tempModel.TenderId; });
                            var tendList = _tenderDetailRepository.InsertRange(entList);
                            _uow.SaveChanges();

                            foreach (var item in tendList.Where(w => w.ProductPrice > 0).ToList())//ilk oluşturan ürün fiyatı girdiyse log at
                                InsertTenderDetailPriceHistory(item, loginUnitId);
                        }

                        _tenderRepository.Update(tender);
                        _uow.SaveChanges();
                        scope.Complete();
                        result.IsSuccess = true;
                        result.Message = "Kayıt işlemi başarılı";
                    }
                }
                else
                    result.Message = "İhale/Satış bulunamadı";
            }
            catch (Exception ex)
            {
                result.Message = "Kayıt sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        private void InsertTenderDetailPriceHistory(TenderDetail tenderDetail, int loginUnitId)
        {
            if (tenderDetail.ProductPrice > 0)
            {
                _tenderDetailPriceHistoryRepository.Insert(new TenderDetailPriceHistory()
                {
                    CreatedBy = tenderDetail.CreatedBy,
                    TenderDetailId = tenderDetail.Id,
                    CreatedUnitId = loginUnitId,
                    Price = tenderDetail.ProductPrice.Value
                });
            }
        }

        private void TenderDetailFileDelete(List<TenderDetail> list)
        {
            try
            {
                foreach (var item in list)
                {
                    var tenderDetailFile = _tenderDetailFileRepository.Where(w => w.TenderDetailId == item.Id).FirstOrDefault();
                    if (tenderDetailFile != null)
                    {
                        var fileUpload = _fileUploadRepository.Find(tenderDetailFile.FileUploadId);
                        if (fileUpload != null)
                            _fileUploadRepository.Delete(fileUpload);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private List<FileUpload> GetTenderDetailFileNameList(List<TenderDetail> list)
        {
            var result = new List<FileUpload>();
            foreach (var item in list)
            {
                var tenderFile = _tenderDetailFileRepository.Where(w => w.TenderDetailId == item.Id).FirstOrDefault();
                if (tenderFile != null)
                    result.Add(_fileUploadRepository.Find(tenderFile.FileUploadId));
            }
            return result;
        }


        private void TenderDetailFileDeleteLocal(List<FileUpload> list)
        {
            try
            {
                foreach (var item in list)
                {
                    if (File.Exists(GetPathAndFileName(item.Name, "uploads/tender/")))
                        File.Delete(GetPathAndFileName(item.Name, "uploads/tender/"));
                }
            }
            catch (Exception e) { }
        }

        private string GetPathAndFileName(string filename, string folderName)
        {
            string path = Path.Combine(_env.WebRootPath, folderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + filename;
        }

        private static List<ETenderHistoryDto> SetStateTenderList(List<ETenderHistoryDto> list)
        {
            foreach (var item in list)
            {
                item.StateName = item.State switch
                {
                    (int)TenderState_.Draft => "taslak olarak kaydedilmiştir",
                    (int)TenderState_.SendInstitution => "kuruma gönderildi",
                    (int)TenderState_.Revise => "revize edildi",
                    _ => "Tip Bulunamadı",
                };
            }
            return list;
        }

        private static string StateTender(int state)
        {
            var result = state switch
            {
                (int)TenderState_.Draft => "<span class='label bg-success-300 full-width'>Taslak</span>",
                (int)TenderState_.SendInstitution => "<span class='label bg-success-300 full-width'>Kuruma gönderildi</span>",
                (int)TenderState_.Revise => "<span class='label bg-success-300 full-width'>Revize edildi</span>",
                _ => "<span class='label bg-success-300 full-width'>Tip Bulunamadı</span>",
            };
            return result;
        }

        public ETenderAllDto GetTenderDetail(int tenderId)
        {
            var tender = (from t in _tenderRepository.GetAll()
                          join u in _userRepository.GetAll() on t.CreatedBy equals u.Id
                          where t.Id == tenderId
                          select new ETender_Dto()
                          {
                              Name = t.Name,
                              SalesNumber = t.SalesNumber,
                              TenderDate = t.TenderDate,
                              NameSurname = u.Name + " " + u.Surname,
                              Email = u.Email,
                              FooterInfo = t.FooterInfo
                          }).FirstOrDefault();

            var institution = (from th in _tenderHistoryRepository.GetAll()
                               join i in _institutionRepository.GetAll() on th.InstitutionId equals i.Id into iL
                               from i in iL.DefaultIfEmpty()
                               where th.TenderId == tenderId && th.InstitutionId != null
                               select new ETender_Dto()
                               {
                                   Id = th.Id,
                                   InstitutionName = i.Name
                               }).OrderByDescending(o => o.Id).FirstOrDefault();

            if (institution != null && tender != null)
                tender.InstitutionName = institution.InstitutionName;

            var tenderDetail = (from td in _tenderDetailRepository.GetAll()
                                join unit in _lookUpListRepository.GetAll() on td.UnitTypeId equals unit.Id
                                where td.TenderId == tenderId && td.Status
                                select new ETenderDetailDto()
                                {
                                    Id = td.Id,
                                    IsPrint = td.IsPrint,
                                    ProductName = td.ProductName,
                                    StockCode = td.StockCode,
                                    Piece = td.Piece,
                                    UnitTypeId = td.UnitTypeId,
                                    ProductPrice = td.ProductPrice,
                                    TotalProductPrice = td.Piece * (td.ProductPrice ?? (decimal)0),
                                    UnitId = td.UnitId,
                                    SellingCost = td.SellingCost,
                                    JobIncrease = td.JobIncrease,
                                    UnitTypeName = unit.Name,
                                    //CustomButton = _tenderDetailFileRepository.Count(w => w.TenderDetailId == td.Id) > 0 ? "<button onclick='funcSetTenderDetailFolder(" + td.Id + ")' type='submit'><i class='icon-folder-search' style='color: #fccc77;'></i></button>" : "",
                                }).ToList();

            var contactList = _tenderContactRepository
                .Where(w => w.TenderId == tenderId).Select(s => new ESelect2Dto()
                {
                    Name = s.Name,
                    Surname = " ...."
                }).ToList();

            return new ETenderAllDto()
            {
                Tender = tender,
                TotalAmountTenderDetail = tenderDetail.Sum(s => s.TotalProductPrice),
                TenderDetailList = tenderDetail,
                ContactList = contactList
            };
        }

        public List<ETenderDetailDto> GetTenderDetailPriceHistory(int tenderDetailId)
        {
            var list = (from th in _tenderDetailPriceHistoryRepository.GetAll()
                        join u in _userRepository.GetAll() on th.CreatedBy equals u.Id
                        join td in _tenderDetailRepository.GetAll() on th.TenderDetailId equals td.Id
                        where th.TenderDetailId == tenderDetailId
                        select new ETenderDetailDto()
                        {
                            ProductName = td.ProductName,
                            NameSurname = u.Name + " " + u.Surname,
                            CreatedDate = th.CreatedDate,
                            ProductPrice = th.Price
                        }).OrderByDescending(o => o.CreatedDate).ToList();
            return list;
        }
        #endregion

        #region Tender Unit
        public List<ETenderDetailDto> GetTenderDetailForUnit(int tenderId, int loginUnitId, bool isAdmin)
        {
            var list = (from td in _tenderDetailRepository.GetAll()
                        join u in _unitRepository.GetAll() on td.UnitId equals u.Id
                        join unit in _lookUpListRepository.GetAll() on td.UnitTypeId equals unit.Id
                        where td.TenderId == tenderId && td.Status
                        select new ETenderDetailDto()
                        {
                            Id = td.Id,
                            UnitId = td.UnitId,
                            ProductName = td.ProductName,
                            UnitName = u.Name,
                            StockCode = td.StockCode,
                            Piece = td.Piece,
                            ProductPrice = td.ProductPrice,
                            TotalProductPrice = td.Piece * (td.ProductPrice ?? (decimal)0),
                            UnitTypeName = unit.Name,
                            CustomButton = "<a onclick='funcOpenTenderDetailModal(" + td.Id + ")'><i class='icon-comment-discussion position-left'></i></a>"
                        });

            if (!isAdmin) //admin değilse sadece kendi satırlarını görsün
            {
                var createdUnitId = _tenderRepository.Find(tenderId).CreatedUnitId;//kendi oluşturduğu ihale detaylarını ve adminse tümünü görebilir
                if (createdUnitId != loginUnitId)
                    list = list.Where(w => w.UnitId == loginUnitId);
            }

            return list.OrderBy(o => o.ProductPrice).ToList().Select(s => new ETenderDetailDto()
            {
                Id = s.Id,
                ProductName = s.ProductName,
                StockCode = s.StockCode,
                Piece = s.Piece,
                UnitName = s.UnitName,
                ProductPrice = s.ProductPrice,
                ProductPriceInfo = (s.ProductPrice != null && s.ProductPrice > 0) ? s.ProductPrice.Value.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR")) : "<span class='label bg-orange-300 full-width faa-flash animated faa-slow'>Bekliyor</span>",
                TotalProductPrice = s.TotalProductPrice,
                UnitTypeName = s.UnitTypeName,
                CustomButton = s.CustomButton
            }).ToList();
        }

        public ETenderDetailDto GetTenderDetailByTenderDetailId(int tenderDetailId, int loginUnitId, bool isAdmin)
        {
            var entityDto = new ETenderDetailDto();
            var result = (from td in _tenderDetailRepository.GetAll()
                          join l in _lookUpListRepository.GetAll() on td.UnitTypeId equals l.Id
                          where td.Id == tenderDetailId && td.Status //&& td.UnitId == loginUnitId
                          select new ETenderDetailDto()
                          {
                              Id = td.Id,
                              TenderId = td.TenderId,
                              ProductName = td.ProductName,
                              StockCode = td.StockCode,
                              Piece = td.Piece,
                              ProductPrice = td.ProductPrice,
                              UnitTypeName = l.Name,
                              UnitId = td.UnitId,
                              TotalProductPrice = td.Piece * (td.ProductPrice ?? (decimal)0),
                              ProductDescription = td.ProductDescription
                          }).FirstOrDefault();

            if (result != null)
            {
                var tenderCreated = _tenderRepository.Find(result.TenderId);
                if (!isAdmin && tenderCreated.CreatedUnitId != loginUnitId)
                    result = result.UnitId == loginUnitId ? result : null;

                if (result != null)//üstteki adımda yetkisi yoksa null olabilir
                {
                    entityDto = _mapper.Map<ETenderDetailDto>(result);
                    entityDto.files = (from td in _tenderDetailRepository.GetAll()
                                       join tdf in _tenderDetailFileRepository.GetAll() on td.Id equals tdf.TenderDetailId
                                       join fu in _fileUploadRepository.GetAll() on tdf.FileUploadId equals fu.Id
                                       where td.Id == tenderDetailId
                                       select new EFileUploadDto
                                       {
                                           Id = fu.Id,
                                           Name = fu.Name,
                                           Extention = fu.Extention,
                                           FileSize = fu.FileSize,
                                       }).ToList();
                }
            }
            return entityDto;
        }


        public EResultDto InsertUpdateTenderDetailForUnit(IList<IFormFile> files, ETenderDetailDto tempModel, int loginUnitId)
        {
            var result = new EResultDto { IsSuccess = false };
            try
            {
                var entity = _tenderDetailRepository.Find(tempModel.Id);
                var tender = _tenderRepository.Find(entity.TenderId);

                if (tender.State != (int)TenderState_.Draft)
                    result.Message = "Teklif/Satış taslak aşamasında olmadığından işlem yapılamaz";
                if (entity.UnitId == null)
                    result.Message = "Birime atanmayan üründe işlem yapılamaz";
                else if (entity.UnitId != loginUnitId && !tempModel.IsAdmin)
                    result.Message = "Bu işlemi yapmaya yetkiniz bulunmamaktadır.";
                else if (tempModel.ProductPrice == null)
                    result.Message = "Fiyat alanı boş olamaz";
                else
                {
                    using (var scope = new TransactionScope())
                    {
                        var priceHistoryEntity = new TenderDetailPriceHistory()
                        {
                            CreatedBy = tempModel.CreatedBy,
                            TenderDetailId = entity.Id,
                            CreatedUnitId = loginUnitId,
                            Price = tempModel.ProductPrice.Value
                        };
                        _tenderDetailPriceHistoryRepository.Insert(priceHistoryEntity);

                        entity.ProductPrice = tempModel.ProductPrice;
                        _tenderDetailRepository.Update(entity);

                        if (files.Count > 0)
                        {
                            result = FileTenderDetailInsert(files, entity.Id);
                            if (!result.IsSuccess)
                                return new EResultDto { IsSuccess = false };
                        }

                        _uow.SaveChanges();
                        scope.Complete();
                        result.IsSuccess = true;
                        result.Id = entity.TenderId;
                        result.Message = "Kayıt işlemi başarılı";
                    }
                }
            }
            catch (Exception e)
            {
                var fs = new FileService(_uow, _env);
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/tender/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }


        private EResultDto FileTenderDetailInsert(IList<IFormFile> files, int tenderDetailId)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                result = fs.FileUploadInsertTender(files);
                if (result.IsSuccess)
                {
                    foreach (var item in result.Ids)
                    {
                        var entity = _tenderDetailFileRepository.Insert(new TenderDetailFile
                        {
                            FileUploadId = item,
                            TenderDetailId = tenderDetailId
                        });
                    }

                    _uow.SaveChanges();
                    result.IsSuccess = true;
                    result.Id = tenderDetailId;
                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Dosya yüklemede hata oluştu";
                }
            }
            catch (Exception)
            {
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/tender/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }

        #endregion



        private EResultDto FileTenderInsert(IList<IFormFile> files, int tenderId)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                result = fs.FileUploadInsertTender(files);
                if (result.IsSuccess)
                {
                    foreach (var item in result.Ids)
                    {
                        var entity = _tenderFileRepository.Insert(new TenderFile
                        {
                            FileUploadId = item,
                            TenderId = tenderId
                        });
                    }

                    _uow.SaveChanges();
                    result.Id = tenderId;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Dosya yüklemede hata oluştu";
                }
            }
            catch (Exception)
            {
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/tender/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }

        private static string DateFormat(int month)
        {
            if (month < 10)
                return "0" + month;
            return month.ToString();
        }
    }
}