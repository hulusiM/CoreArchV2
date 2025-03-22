using AutoMapper;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;

namespace CoreArchV2.Services.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<VehicleBrandModel> _brandModelRepository;


        public BrandService(IUnitOfWork uow,
            IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _brandModelRepository = uow.GetRepository<VehicleBrandModel>();
        }

        public async Task<PagedList<EUnitDto>> GetAllWithPagedBrand(int? page, EUnitDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = await Task.FromResult(from u in _brandModelRepository.GetAll()
                                             where u.ParentId == null && u.Status
                                             select new EUnitDto()
                                             {
                                                 Id = u.Id,
                                                 Name = "<b style='color:Red;'>" + u.Name + "</b>",
                                                 PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                                                 CustomButton = "<li title='Düzenle' class='text-primary-400'><a onclick='getByIdBrand(" +
                                                                u.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                                                                "<li title='sil' class='text-danger-800'><a data-toggle='modal' onclick='deleteBrand(" +
                                                                u.Id + ");'><i class='icon-trash'></i></a></li>"
                                             });

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            var lastResult = new PagedList<EUnitDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }
        public EResultDto UpdateBrand(EUnitDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                tempModel.Name = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Name);
                var name = _brandModelRepository.GetAll().FirstOrDefault(w => w.Status && w.Name == tempModel.Name && w.Id != tempModel.Id);
                if (name != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Bu isimle kayıt bulunmaktadır!";
                }
                else
                {
                    var oldEntiy = _brandModelRepository.Find(tempModel.Id);
                    var entity = _mapper.Map(tempModel, oldEntiy);
                    _brandModelRepository.Update(entity);
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

        public EResultDto BrandNameControl(EUnitDto tempModel)
        {
            var result = new EResultDto();
            if (_brandModelRepository.GetAll().Any(w => w.Status && w.Name == tempModel.Name))
            {
                result.IsSuccess = false;
                result.Message = "Bu isimle kayıt bulunmaktadır!";
            }

            return result;
        }

        public EResultDto InsertBrand(EUnitDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                tempModel.Name = ExternalProcess.SetPascalCaseNameWithSpace(tempModel.Name);
                result = BrandNameControl(tempModel);
                if (result.IsSuccess)
                {
                    var entity = new VehicleBrandModel()
                    {
                        Name = tempModel.Name,
                        ParentId = tempModel.ParentId,//dolu gelirse model,boş gelirse marka
                        Status = true
                    };

                    _brandModelRepository.Insert(entity);
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

        public EResultDto DeleteBrand(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _brandModelRepository.Find(id);
                entity.Status = false;

                if (entity.ParentId == null) //alt kırılımları pasif yapılıyor
                {
                    var project = _brandModelRepository.GetAll().Where(w => w.ParentId == entity.Id).ToList();
                    project.ForEach(f => f.Status = false);
                    _brandModelRepository.UpdateRange(project);
                }

                _brandModelRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
            }

            return result;
        }

        public EUnitDto GetByIdBrand(int id)
        {
            var result = _mapper.Map<EUnitDto>(_brandModelRepository.Find(id));
            return result;
        }
    }
}
