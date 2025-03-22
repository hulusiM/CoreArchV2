using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;

namespace CoreArchV2.Services.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Color> _colorRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleAmount> _vehicleAmountRepository;
        private readonly IGenericRepository<VehicleBrandModel> _vehicleBrandModelRepository;
        private readonly IGenericRepository<VehicleCity> _vehicleCityRepository;
        private readonly IGenericRepository<VehicleContract> _vehicleContractRepository;
        private readonly IGenericRepository<VehicleDebit> _vehicleDebitRepository;
        private readonly IGenericRepository<VehicleExaminationDate> _vehicleExaminationDateRepository;
        private readonly IGenericRepository<VehicleFile> _vehicleFileRepository;
        private readonly IGenericRepository<VehicleMaterial> _vehicleMaterialRepository;
        private readonly IGenericRepository<VehicleRent> _vehicleRentRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehicleTransferFile> _vehicleTransferFileRepository;
        private readonly IGenericRepository<VehicleTransferLog> _vehicleTransferLogRepository;
        private readonly IGenericRepository<Institution> _institutionRepository;


        public UtilityService(IUnitOfWork uow,
            IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _vehicleDebitRepository = uow.GetRepository<VehicleDebit>();
            _vehicleExaminationDateRepository = uow.GetRepository<VehicleExaminationDate>();
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _cityRepository = uow.GetRepository<City>();
            _vehicleRentRepository = uow.GetRepository<VehicleRent>();
            _vehicleFileRepository = uow.GetRepository<VehicleFile>();
            _unitRepository = uow.GetRepository<Unit>();
            _vehicleCityRepository = uow.GetRepository<VehicleCity>();
            _vehicleTransferLogRepository = uow.GetRepository<VehicleTransferLog>();
            _vehicleTransferFileRepository = uow.GetRepository<VehicleTransferFile>();
            _vehicleBrandModelRepository = uow.GetRepository<VehicleBrandModel>();
            _colorRepository = uow.GetRepository<Color>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _messageRepository = uow.GetRepository<Message>();
            _vehicleMaterialRepository = uow.GetRepository<VehicleMaterial>();
            _vehicleAmountRepository = uow.GetRepository<VehicleAmount>();
            _vehicleContractRepository = uow.GetRepository<VehicleContract>();
            _institutionRepository = uow.GetRepository<Institution>();
        }

        #region Logistics Unit
        public async Task<PagedList<EUnitDto>> GetAllWithPagedUnit(int? page, EUnitDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = await Task.FromResult(from u in _unitRepository.GetAll()
                                             where u.ParentId == null && u.Status == true
                                             select new EUnitDto()
                                             {
                                                 Id = u.Id,
                                                 Name = "<b style='color:Red;'>" + u.Name + "</b>",
                                                 PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                                                 Code = u.Code,
                                                 CustomButton = "<li title='Düzenle' class='text-primary-400'><a onclick='getByIdUnit(" +
                                                                u.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                                                                "<li title='sil' class='text-danger-800'><a data-toggle='modal' onclick='deleteUnit(" +
                                                                u.Id + ");'><i class='icon-trash'></i></a></li>"
                                             });

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            var lastResult = new PagedList<EUnitDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }
        public EResultDto UpdateUnit(EUnitDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                tempModel.Name = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Name);
                tempModel.Code = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Code);

                var name = _unitRepository.GetAll().FirstOrDefault(w => w.Status && w.Name == tempModel.Name && w.Id != tempModel.Id);
                var code = _unitRepository.GetAll().FirstOrDefault(w => w.Status && w.Code == tempModel.Code && w.Id != tempModel.Id);
                if (name != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu isimle kayıt bulunmaktadır!";
                }
                else if (code != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu kısa kodla kayıt bulunmaktadır!";
                }
                else
                {
                    tempModel.Name = tempModel.Name.TrimEnd().TrimStart();
                    tempModel.Code = tempModel.Code.TrimEnd().TrimStart();
                    var oldEntiy = _unitRepository.Find(tempModel.Id);
                    var entity = _mapper.Map(tempModel, oldEntiy);
                    _unitRepository.Update(entity);
                    _uow.SaveChanges();
                    result.Id = entity.Id;
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Güncelleme sırasında hata oluştu!";
            }
            return result;
        }
        public EResultDto UnitCodeNameControl(EUnitDto tempModel)
        {
            var result = new EResultDto();
            var entity = _unitRepository.FirstOrDefault(w => w.Name.ToLower().TrimEnd().TrimStart() == tempModel.Name.ToLower().TrimEnd().TrimStart());
            if (entity != null)
            {
                result.IsSuccess = false;
                result.Message = entity.Status == true ? "Bu isimle aktif kayıt mevcuttur" : "Bu isimle daha önce kayıt edilip silinmiş kayıt mevcuttur,adminle iletişime geçiniz";
            }
            else if (_unitRepository.GetAll().ToList().Any(w => w.Code.ToLower().TrimEnd().TrimStart() == tempModel.Code.ToLower().TrimEnd().TrimStart()))
            {
                result.IsSuccess = false;
                result.Message = "Bu kısa kodla kayıt bulunmaktadır!";
            }
            return result;
        }
        public EResultDto InsertUnit(EUnitDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                tempModel.Name = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Name);
                tempModel.Code = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Code);
                result = UnitCodeNameControl(tempModel);
                if (result.IsSuccess)
                {
                    var entity = new Unit()
                    {
                        Name = tempModel.Name.TrimEnd().TrimStart(),
                        Code = tempModel.Code.TrimEnd().TrimStart(),
                        ParentId = tempModel.ParentId,//dolu gelirse proje,boş gelirse müdürlük
                        Status = true
                    };

                    _unitRepository.Insert(entity);
                    _uow.SaveChanges();
                    result.Id = entity.Id;
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }
        public EResultDto DeleteUnit(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _unitRepository.Find(id);
                entity.Status = false;

                if (entity.ParentId == null) //alt kırılımları pasif yapılıyor
                {
                    var project = _unitRepository.GetAll().Where(w => w.ParentId == entity.Id).ToList();
                    project.ForEach(f => f.Status = false);
                    _unitRepository.UpdateRange(project);
                }

                _unitRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }

            return result;
        }
        public EUnitDto GetByIdUnit(int id) //=> _mapper.Map<EUnitDto>(_unitRepository.Find(id));
        {
            var result = _mapper.Map<EUnitDto>(_unitRepository.Find(id));
            return result;
        }
        #endregion


        #region Institution Unit
        public async Task<PagedList<EUnitDto>> GetAllWithPagedInstitution(int? page, EUnitDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = await Task.FromResult(from u in _institutionRepository.GetAll()
                                             where u.ParentId == null && u.Status
                                             select new EUnitDto()
                                             {
                                                 Id = u.Id,
                                                 Name = "<b style='color:Red;'>" + u.Name + "</b>",
                                                 PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                                                 Code = u.Code,
                                                 CustomButton = "<li title='Düzenle' class='text-primary-400'><a onclick='getByIdUnit(" +
                                                                u.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                                                                "<li title='sil' class='text-danger-800'><a data-toggle='modal' onclick='deleteUnit(" +
                                                                u.Id + ");'><i class='icon-trash'></i></a></li>"
                                             });

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            var lastResult = new PagedList<EUnitDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }
        public EResultDto UpdateInstitution(EUnitDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                tempModel.Name = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Name);
                tempModel.Code = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Code);
                var name = _institutionRepository.GetAll().FirstOrDefault(w => w.Status && w.Name == tempModel.Name && w.Id != tempModel.Id);
                var code = _institutionRepository.GetAll().FirstOrDefault(w => w.Status && w.Code == tempModel.Code && w.Id != tempModel.Id);
                if (name != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu isimle kayıt bulunmaktadır!";
                }
                else if (code != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu kısa kodla kayıt bulunmaktadır!";
                }
                else
                {
                    var oldEntiy = _institutionRepository.Find(tempModel.Id);
                    var entity = _mapper.Map(tempModel, oldEntiy);
                    _institutionRepository.Update(entity);
                    _uow.SaveChanges();
                    result.Id = entity.Id;
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Güncelleme sırasında hata oluştu!";
            }
            return result;
        }
        public EResultDto InstitutionCodeNameControl(EUnitDto tempModel)
        {
            var result = new EResultDto();
            if (_institutionRepository.GetAll().Any(w => w.Status && w.Name == tempModel.Name))
            {
                result.IsSuccess = false;
                result.Message = "Bu isimle kayıt bulunmaktadır!";
            }
            else if (_institutionRepository.GetAll().Any(w => w.Code == tempModel.Code))
            {
                result.IsSuccess = false;
                result.Message = "Bu kısa kodla kayıt bulunmaktadır!";
            }
            return result;
        }
        public EResultDto InsertInstitution(EUnitDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                tempModel.Name = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Name);
                tempModel.Code = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Code);
                result = InstitutionCodeNameControl(tempModel);
                if (result.IsSuccess)
                {
                    var entity = new Institution()
                    {
                        Name = tempModel.Name,
                        Code = tempModel.Code,
                        ParentId = tempModel.ParentId,//dolu gelirse proje,boş gelirse müdürlük
                        Status = true
                    };


                    _institutionRepository.Insert(entity);
                    _uow.SaveChanges();
                    result.Id = entity.Id;
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
            }
            return result;
        }
        public EResultDto DeleteInstitution(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _institutionRepository.Find(id);
                entity.Status = false;

                if (entity.ParentId == null) //alt kırılımları pasif yapılıyor
                {
                    var project = _institutionRepository.GetAll().Where(w => w.ParentId == entity.Id).ToList();
                    project.ForEach(f => f.Status = false);
                    _institutionRepository.UpdateRange(project);
                }

                _institutionRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }

            return result;
        }
        public EUnitDto GetByIdInstitution(int id)
        {
            var result = _mapper.Map<EUnitDto>(_institutionRepository.Find(id));
            return result;
        }
        #endregion
    }
}
