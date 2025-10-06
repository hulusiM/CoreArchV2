using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class TireLogService : ITireLogService
    {
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly ILogger<TireLogService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<TireFile> _tireFileRepository;
        private readonly IGenericRepository<Tire> _tireRepository;
        private readonly IGenericRepository<TireDebit> _tireDebitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;

        private readonly IWebHostEnvironment _env;


        public TireLogService(IUnitOfWork uow,
            IMapper mapper,
            ILogger<TireLogService> logger,
            IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
            _mapper = mapper;
            _logger = logger;
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _cityRepository = uow.GetRepository<City>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _tireRepository = uow.GetRepository<Tire>();
            _tireDebitRepository = uow.GetRepository<TireDebit>();
            _tireFileRepository = uow.GetRepository<TireFile>();
        }

        public PagedList<ETireDto> GetAllWithPaged(int? page, ETireDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from t in _tireRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                        join ld in _lookUpListRepository.GetAll() on t.DimensionTypeId equals ld.Id
                        join ty in _lookUpListRepository.GetAll() on t.TireTypeId equals ty.Id
                        where t.State == (int)TireState.Debit
                        select new ETireDto()
                        {
                            VehicleId = v.Id,
                            Plate = "<span class='label bg-orange-300 full-width'>" + v.Plate + "</span>",
                            DimensionTypeName = "<span class='label bg-green-300 full-width'>" + ld.Name + "</span>",
                            DimensionTypeId = ld.Id,
                            TireTypeName = ty.Name,
                            TireTypeId = ty.Id,
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                            CustomButton = //"<li title='Kaydı sil ve üstündeki lastikleri takıldığı depoya aktar' class='text-danger-800'><a data-toggle='modal' onclick='funcDeleteTireRecord(" + v.Id + ");'><i class='icon-trash'></i></a></li>" +
                                           "<li title='Lastik Geçmişi' class='text-success-800'><a data-toggle='modal' onclick='funcHistoryTire(" + v.Id + ");'><i class='icon-search4'></i></a></li>"
                        });

            if (filterModel.DimensionTypeId > 0)
                list = list.Where(w => w.DimensionTypeId == filterModel.DimensionTypeId);

            if (filterModel.TireTypeId > 0)
                list = list.Where(w => w.TireTypeId == filterModel.TireTypeId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            var groupList = list.ToList().GroupBy(g => g.VehicleId).Select(s => new ETireDto()
            {
                Plate = s.First().Plate,
                DimensionTypeName = s.First().DimensionTypeName,
                TireTypeName = "<span class='label bg-warning-300 full-width'>" + String.Join(",", list.Where(w => w.VehicleId == s.First().VehicleId).ToList().GroupBy(g => g.TireTypeName).Select(s => s.Key).ToList()) + "</span>",
                PageStartCount = s.First().PageStartCount,
                CustomButton = s.First().CustomButton,
                TireCount = s.Count()
            }).ToList();

            return new PagedList<ETireDto>(groupList, page, PagedCount.GridKayitSayisi);
        }

        public EResultDto Insert(ETireDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var entity = _mapper.Map<Tire>(tempModel);
                entity.State = (int)TireState.InHouse;
                entity.CreatedBy = tempModel.CreatedBy;
                entity.VehicleId = null;
                for (int i = 0; i < tempModel.TireCount; i++)
                {
                    _tireRepository.Insert(entity);
                    _uow.SaveChanges();
                    entity.Id = 0;
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

        public EResultDto Delete(ETireDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var list = GetWareHouseTireEmptyList(tempModel);
                if (list.Count == 0)
                {
                    result.IsSuccess = false;
                    result.Message = "Kriterlere uygun depoda lastik bulunamadı.";
                }
                else if (list.Count() >= tempModel.TireCount)
                {
                    _tireRepository.DeleteRange(list.Take(tempModel.TireCount));
                    _uow.SaveChanges();
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Bu ebatta depoda " + list.Count + " adet bulunmaktadır. En fazla depodaki miktar kadar silebilirsiniz.";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }
            return result;
        }

        public EResultDto TireDebitInsert(ETireDto model)
        {
            var result = new EResultDto();
            try
            {
                var wareHouseEmptyTireList = GetWareHouseTireEmptyList(model);
                if (wareHouseEmptyTireList.Count >= model.TireCount)//Boşta lastik sayısı >= istenen lastik sayısı
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Tire update
                        wareHouseEmptyTireList.Take(model.TireCount).ToList().ForEach(f =>
                    {
                        f.WareHouseId = null;
                        f.State = (int)TireState.Debit;
                        f.VehicleId = model.VehicleId;
                    });
                        _tireRepository.UpdateRange(wareHouseEmptyTireList);

                        //TireDebit insert
                        var entity = _mapper.Map<TireDebit>(model);
                        entity.CreatedBy = model.CreatedBy;
                        entity.AttachedTireCount = model.TireCount;
                        entity.State = (int)TireState.Debit;
                        _tireDebitRepository.Insert(entity);
                        _uow.SaveChanges();
                        scope.Complete();
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Depodaki boşta lastik sayısı uyuşmuyor. En fazla " + wareHouseEmptyTireList.Count + " adet lastik takılabilir.";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }

        //Depoda boşta olan lastikleri listeler
        public List<Tire> GetWareHouseTireEmptyList(ETireDto model)
        {
            return _tireRepository.Where(w => w.WareHouseId == model.WareHouseId &&
                                              w.DimensionTypeId == model.DimensionTypeId &&
                                              w.TireTypeId == model.TireTypeId &&
                                              w.State == (int)TireState.InHouse).ToList();
        }

        //Depoda araca zimmetli lastikleri listeler
        public List<Tire> GetWareHouseTireDebitVehicleList(ETireDto model)
        {
            var list = _tireRepository.Where(w => w.VehicleId == model.VehicleId && w.State == (int)TireState.Debit);
            if (model.DimensionTypeId > 0)
                list = list.Where(w => w.DimensionTypeId == model.DimensionTypeId);

            if (model.TireTypeId > 0)
                list = list.Where(w => w.TireTypeId == model.TireTypeId);

            return list.ToList();
        }

        public bool IsVehicleAttachTire(ETireDto model)
        {
            var tireCount = _tireRepository.Count(w => w.VehicleId == model.VehicleId && w.State == (int)TireState.Debit);
            return tireCount > 0;
        }

        //Araç üstünde lastik var mı?
        public EResultDto GetDebitTireInfo(ETireDto model)
        {
            var result = new EResultDto();
            var temp = (from t in _tireRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on t.VehicleId equals v.Id
                        join l in _lookUpListRepository.GetAll() on t.DimensionTypeId equals l.Id
                        join l2 in _lookUpListRepository.GetAll() on t.TireTypeId equals l2.Id
                        where t.VehicleId == model.VehicleId && t.State == (int)TireState.Debit
                        select new ETireDto()
                        {
                            DimensionTypeId = l.Id,
                            DimensionTypeName = l.Name,
                            TireTypeId = l2.Id,
                            TireTypeName = l2.Name,
                            Plate = v.Plate
                        }).ToList();

            var groupBy = temp.GroupBy(g => g.TireTypeName).Select(s => new ETireDto()
            {
                Plate = s.First().Plate,
                DimensionTypeName = s.First().DimensionTypeName,
                TireTypeId = s.First().TireTypeId,
                TireTypeName = s.First().TireTypeName,
                TireCount = s.Count()
            }).ToList();

            if (groupBy.Count > 0)
            {
                var plate = "";
                var desc = "";
                foreach (var item in groupBy)
                {
                    plate = item.Plate;
                    desc += "<b style='color:brown;'>" + item.DimensionTypeName +
                            "</b> ebatında " + item.TireCount + " adet " + item.TireTypeName + "</b></br>";
                }

                result.Message = "<u>" + plate + "</u> plakalı araç üzerindeki lastikler:</br>";
                result.Message += desc;
            }

            return result;
        }

        public List<ETireDto> GetTireHistory(ETireDto model)
        {
            var list = (from td in _tireDebitRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on td.VehicleId equals v.Id
                        join lw in _lookUpListRepository.GetAll() on td.WareHouseId equals lw.Id
                        join ld in _lookUpListRepository.GetAll() on td.DimensionTypeId equals ld.Id
                        join lt in _lookUpListRepository.GetAll() on td.TireTypeId equals lt.Id
                        where td.VehicleId == model.VehicleId
                        select new ETireDto()
                        {
                            Id = td.Id,
                            CreatedDate = td.CreatedDate,
                            Plate = v.Plate,
                            WareHouseName = lw.Name,
                            DimensionTypeName = ld.Name,
                            TireTypeName = lt.Name,
                            TireCount = td.AttachedTireCount,
                            State = td.State,
                            Description = td.Description
                        }).OrderByDescending(o => o.Id).ToList();

            for (int i = 0; i < list.Count(); i++)
                list[i].StatusName = GetTireStateName(list[i].State);

            return list;
        }

        public string GetTireStateName(int tireTypeId)
        {
            var enumDisplayStatus = (TireState)tireTypeId;
            string stringValue = enumDisplayStatus.ToString();

            return stringValue switch
            {
                "Debit" => "Araca Takıldı",
                "InHouse" => "Depoya Bırakıldı",
                "Deleted" => "Silindi",
                "Return" => "İade Edildi",
                _ => "",
            };
        }

        public EResultDto SetTireInert(ETireDto model)
        {
            var result = new EResultDto();
            try
            {
                if (model.TireChangeType == 1)//depodan atıl işlemi
                {
                    if (model.WareHouseId > 0)
                    {
                        var wareHouseEmptyTireList = GetWareHouseTireEmptyList(model);
                        if (wareHouseEmptyTireList.Count >= model.TireCount) //Adet kadar lastik atıl moda alınıyor
                        {
                            wareHouseEmptyTireList.Take(model.TireCount).ToList().ForEach(f => f.State = (int)TireState.Deleted);
                            _tireRepository.UpdateRange(wareHouseEmptyTireList);
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = "Kriterlere uygun depoda boşta " + wareHouseEmptyTireList.Count + " adet lastik bulunmaktadır.";
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Depo boş geçilemez";
                    }
                }
                else if (model.TireChangeType == 2) //Plakadan atıl işlemi
                {
                    var debitTirelist = GetWareHouseTireDebitVehicleList(model);
                    if (debitTirelist.Count == 0)
                    {
                        result.IsSuccess = false;
                        result.Message = "Araç üstünde bu kullanım tipinde lastik bulunmadı";
                    }
                    else if (debitTirelist.Count >= model.TireCount)
                    {
                        debitTirelist.Take(model.TireCount).ToList().ForEach(f => f.State = (int)TireState.Deleted);
                        _tireRepository.UpdateRange(debitTirelist);
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Bu kullanım tipinde en fazla " + debitTirelist.Count + " adet atıl duruma atılabilir";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Hiçbir işlem yapılmadı";
                }

                _uow.SaveChanges();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }

        public RTireDto GetTireReport(ETireDto model)
        {
            var result = new RTireDto();
            var list = (from t in _tireRepository.GetAll()
                        join lw in _lookUpListRepository.GetAll() on t.WareHouseId equals lw.Id into lwL
                        from lw in lwL.DefaultIfEmpty()
                        join ld in _lookUpListRepository.GetAll() on t.DimensionTypeId equals ld.Id
                        join lt in _lookUpListRepository.GetAll() on t.TireTypeId equals lt.Id
                        select new ETireDto()
                        {
                            WareHouseId = t.WareHouseId,
                            WareHouseName = lw.Name,
                            VehicleId2 = t.VehicleId,
                            DimensionTypeName = ld.Name,
                            DimensionTypeId = ld.Id,
                            TireTypeName = lt.Name,
                            TireTypeId = lt.Id,
                            State = t.State
                        });

            if (list.Count() > 0)
            {
                if (model.WareHouseId > 0)
                    list = list.Where(w => w.WareHouseId == model.WareHouseId);

                if (model.DimensionTypeId > 0)
                    list = list.Where(w => w.DimensionTypeId == model.DimensionTypeId);

                if (model.TireTypeId > 0)
                    list = list.Where(w => w.TireTypeId == model.TireTypeId);

                var inertCount = list.Count(c => c.State == (int)TireState.Deleted);
                var emptyCount = list.Count(c => c.State == (int)TireState.InHouse);
                var debitCount = list.Count(c => c.State == (int)TireState.Debit);
                var allCount = list.Count();

                //Depo bazlı lastik sayıları
                var allList = (from l in list.Where(w => w.State == (int)TireState.InHouse).ToList()
                               group l by new
                               {
                                   l.WareHouseName,
                                   l.WareHouseId,
                                   l.VehicleId2,
                                   l.DimensionTypeId,
                                   l.DimensionTypeName,
                                   l.TireTypeId,
                                   l.TireTypeName
                               }
                    into gcs
                               select new ETireDto()
                               {
                                   WareHouseName = gcs.First().WareHouseName,
                                   DimensionTypeName = gcs.First().DimensionTypeName,
                                   TireTypeName = gcs.First().TireTypeName,
                                   TireCount = gcs.Count()
                               }).ToList();

                var groupByTypeList = list.Where(w => w.State == (int)TireState.InHouse).ToList().GroupBy(g => g.WareHouseId).Select(s => new ETireDto()
                {
                    WareHouseName = s.First().WareHouseName,
                    TireCount = s.Count()
                }).ToList();

                result = new RTireDto()
                {
                    InertCount = inertCount,
                    DebitCount = debitCount,
                    EmptyCount = emptyCount,
                    AllCount = allCount,
                    TireList = groupByTypeList,
                    TireAllList = allList
                };
                return result;
            }
            else
                return result;
        }

        public EResultDto TireReturn(ETireDto model)
        {
            var result = new EResultDto();
            try
            {
                var debitTireList = GetWareHouseTireDebitVehicleList(model);
                if (debitTireList.Count == 0)
                {
                    result.IsSuccess = false;
                    result.Message = "Araç üstünde seçilen kullanım tipinde lastik bulunamadı";
                }
                else if (debitTireList.Count >= model.TireCount)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Tire Update
                        debitTireList.Take(model.TireCount).ToList().ForEach(f =>
                                {
                                    f.WareHouseId = model.TargetWareHouseId;
                                    f.State = (int)TireState.InHouse;
                                    f.VehicleId = null;
                                });

                        //TireDebit insert
                        var entity = _mapper.Map<TireDebit>(model);
                        entity.CreatedBy = model.CreatedBy;
                        entity.AttachedTireCount = model.TireCount;
                        entity.State = (int)TireState.InHouse;
                        entity.WareHouseId = model.TargetWareHouseId;
                        _tireDebitRepository.Insert(entity);

                        _uow.SaveChanges();
                        scope.Complete();
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Bu kullanım tipinde en fazla " + debitTireList.Count + " adet iade edebilirsiniz";
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }
    }
}