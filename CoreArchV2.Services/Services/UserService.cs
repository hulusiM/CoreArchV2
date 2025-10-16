using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Licence.Dto;
using CoreArchV2.Core.Entity.Licence.Entity;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Util.Hash;
using CoreArchV2.Data;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<Device> _deviceRepository;
        private readonly LicenceDbContext _dbLicenceContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUnitOfWork uow,
            IMapper mapper,
            LicenceDbContext dbLicenceContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _mapper = mapper;
            _dbLicenceContext = dbLicenceContext;
            _userRepository = uow.GetRepository<User>();
            _unitRepository = uow.GetRepository<Unit>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _messageRepository = uow.GetRepository<Message>();
            _deviceRepository = uow.GetRepository<Device>();
            _httpContextAccessor = httpContextAccessor;
        }
        public PagedList<EUserDto> GetAllWithPaged(int? page, EUserDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = from m in _userRepository.GetAll()
                       join unit in _unitRepository.GetAll() on m.UnitId equals unit.Id into unitL
                       from unit in unitL.DefaultIfEmpty()
                       join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                       from unit2 in unit2L.DefaultIfEmpty()
                           //where m.Status == Convert.ToBoolean(Status.Active)
                       select new EUserDto
                       {
                           Id = m.Id,
                           Status = m.Status,
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           CreatedDate = m.CreatedDate,
                           CreatedBy = m.CreatedBy,
                           CreatedIp = m.CreatedIp,
                           Name = m.Name,
                           Surname = m.Surname,
                           PassportNo = m.PassportNo,
                           BirthDate = m.BirthDate,
                           Gender = m.Gender,
                           MobilePhone = m.MobilePhone,
                           Email = m.Email,
                           MailSenderInfo = m.IsSendMail ? "<span class='label bg-green-300'>Açık</span>" : "<span class='label bg-primary'>Kapalı</span>",
                           MailSenderVehicleOpInfo = m.IsSendMailVehicleOpReport.GetValueOrDefault(false) ? "<span class='label bg-green-300'>Açık</span>" : "<span class='label bg-primary'>Kapalı</span>",
                           CityId = m.CityId,
                           UnitName = unit2.Id > 0 ? (unit2.Name + "/" + unit.Name) : unit.Name,
                           UnitId = unit.Id,
                           ParentUnitId = unit2.Id,
                           Flag = m.Flag,
                           StatusName = m.Status == Convert.ToBoolean(Status.Active)
                               ? "<span style='/* width: 45%; */' class='label bg-green-300'>Aktif</span>"
                               : "<span class='label bg-danger-800'>Pasif</span>",
                           CustomButton =
                               "<li title='Kullanıcı düzenle' class='text-primary-400'><a data-toggle='modal' onclick='funcEditUser(" +
                               m.Id + ");'><i class='icon-pencil5'></i></a></li>",
                           DeleteButtonActive = true,
                           FullName = m.Name + " " + m.Surname + "/ <a  href='" + m.MobilePhone.Replace("+9", "") + "'>" +
                                      m.MobilePhone + "</a>"
                       };

            if (filterModel.Flag > 0)
                list = list.Where(w => w.Flag == filterModel.Flag);

            if (filterModel.UnitId > 0)
                list = list.Where(w => w.UnitId == filterModel.UnitId);

            if (filterModel.ParentUnitId > 0 && filterModel.Flag == (int)Flag.Manager)
                list = list.Where(w => w.UnitId == filterModel.ParentUnitId);
            else if (filterModel.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == filterModel.ParentUnitId);

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);


            var lastResult = new PagedList<EUserDto>(list.OrderBy(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var list = _userRepository.GetAllAsync();
            return await list;
        }
        public async Task<EUserDto> GetByIdAsync(int id)
        {
            var result = await Task.FromResult((from m in _userRepository.GetAll()
                                                where m.Id == id
                                                select new EUserDto
                                                {
                                                    Id = m.Id,
                                                    Status = m.Status,
                                                    CreatedDate = m.CreatedDate,
                                                    CreatedBy = m.CreatedBy,
                                                    CreatedIp = m.CreatedIp,
                                                    Name = m.Name,
                                                    Surname = m.Surname,
                                                    UnitId = m.UnitId,
                                                    Image = m.Image != null ? Convert.ToBase64String(m.Image) : "",
                                                    PassportNo = m.PassportNo,
                                                    BirthDate = m.BirthDate,
                                                    Gender = m.Gender,
                                                    MobilePhone = m.MobilePhone,
                                                    Email = m.Email,
                                                    IsSendMail = m.IsSendMail,
                                                    IsSendMailVehicleOpReport = m.IsSendMailVehicleOpReport.GetValueOrDefault(false),
                                                    CityId = m.CityId,
                                                    Flag = m.Flag,
                                                    IsAdmin = m.IsAdmin,
                                                    ArventoUserName = m.ArventoUserName,
                                                    ArventoPassword = m.ArventoPassword
                                                }).FirstOrDefault());
            return result;
        }
        public EResultDto Insert(EUserDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                if (tempModel.MobilePhone != null && _userRepository.Any(a => a.Status && a.MobilePhone == tempModel.MobilePhone))
                {
                    result.IsSuccess = false;
                    result.Message = "Bu telefon numarasıyla kayıt mevcuttur";
                }
                //else if (tempModel.Email != null && _userRepository.Any(a => a.Status && a.Email == tempModel.Email))
                //{
                //    result.IsSuccess = false;
                //    result.Message = "Bu email adresiyle kayıt mevcuttur";
                //}
                else
                {
                    var model = _mapper.Map<User>(tempModel);
                    model.CreatedBy = tempModel.CreatedBy;
                    model.Flag = tempModel.Flag;// tempModel.IsAdmin == false ? (int)Flag.User : (int)Flag.Admin;
                    model.IsAdmin = tempModel.IsAdmin;
                    model.Password = tempModel.Password != null ? OneWayHash.Create(tempModel.Password.TrimStart().TrimEnd()) : null;
                    _userRepository.Insert(model);
                    _uow.SaveChanges();
                    result.IsSuccess = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu";
                return result;
            }
        }
        public EResultDto Update(EUserDto tempModel)
        {
            var result = new EResultDto() { IsSuccess = false };
            try
            {
                var oldEntity = _userRepository.Find(tempModel.Id);
                bool isMail = false;
                bool isPhone = false;

                if (!string.IsNullOrEmpty(tempModel.MobilePhone))
                    isPhone = _userRepository.Any(w => w.Status && w.Id != tempModel.Id && w.MobilePhone == tempModel.MobilePhone);

                //if (!isPhone && !string.IsNullOrEmpty(tempModel.Email))
                //    isMail = _userRepository.Any(w => w.Status && w.Id != tempModel.Id && w.Email == tempModel.Email);

                if (isPhone)
                    result.Message = "Bu telefon numarasıyla kayıt mevcuttur";
                //else if (isMail)
                //    result.Message = "Bu email adresiyle kayıt mevcuttur";
                else
                {
                    var entity = _mapper.Map<EUserDto, User>(tempModel, oldEntity);
                    entity.Flag = tempModel.Flag;// tempModel.IsAdmin == false ? (int)Flag.User : (int)Flag.Admin;
                    entity.IsAdmin = tempModel.IsAdmin;

                    if (!string.IsNullOrEmpty(tempModel.Password))
                        entity.Password = OneWayHash.Create(tempModel.Password.TrimStart().TrimEnd());

                    var oldRecord = _userRepository.FirstOrDefaultNoTracking(f => f.Id == tempModel.Id);
                    if (oldRecord.Status && !tempModel.Status)//kaydı pasife alınca rol var mı kontrol et
                    {
                        var userRole = _userRoleRepository.FirstOrDefault(f => f.Status && f.UserId == tempModel.Id);
                        if (userRole != null)
                        {
                            result.IsSuccess = false;
                            result.Message = "Kullanıcı üzerinde rol bulunmaktadır. Rolü silip tekrar deneyiniz.";
                            return result;
                        }
                    }

                    _userRepository.Update(entity);
                    _uow.SaveChanges();
                    result.IsSuccess = true;
                }
            }
            catch (Exception) { result.Message = "Düzenleme sırasında hata oluştu"; }

            return result;
        }
        public EResultDto Delete(int id)
        {
            var result = new EResultDto();
            try
            {
                var userRole = _userRoleRepository.FirstOrDefault(f => f.Status && f.UserId == id);
                if (userRole != null)
                {
                    result.IsSuccess = false;
                    result.Message = "Kullanıcı üzerinde rol bulunmaktadır";
                }
                else
                {
                    var entity = _userRepository.Find(id);
                    entity.Status = Convert.ToBoolean(Status.Passive);
                    entity.ArventoUserName = null;
                    entity.ArventoPassword = null;
                    _userRepository.Update(entity);
                    _uow.SaveChanges();
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu";
            }

            return result;
        }
        public async Task<EUserDto> FindByUsernameAndPass(EUserDto model)
        {
            var passHash = OneWayHash.Create(model.Password);
            var user = await (from u in _userRepository.GetAll()
                        join unit in _unitRepository.GetAll() on u.UnitId equals unit.Id into unitL
                        from unit in unitL.DefaultIfEmpty()
                        where u.Status && u.MobilePhone == model.MobilePhone.Replace(" ", "") //&& u.Password == HashMD5.Create(model.Password)
                        select new EUserDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Surname = u.Surname,
                            NameSurname = u.Name + " " + u.Surname,
                            Password = u.Password,
                            NewPassword = u.NewPassword,
                            MobilePhone = u.MobilePhone,
                            IsNewPassword = u.IsNewPassword,
                            FullName = u.Name + " " + u.Surname,
                            UnitName = unit.Name,
                            Image = u.Image != null ? Convert.ToBase64String(u.Image) : "",
                            IsAdmin = u.IsAdmin,// u.Flag == (int)Flag.Admin ? true : false,
                            Flag = model.Flag,
                            UnitId = u.UnitId
                        }).FirstOrDefaultAsync();

            if (user != null)
            {
                var entity = _userRepository.Find(user.Id);
                if (user.Password != passHash && user.NewPassword != passHash)
                    return null;
                if (user.Password == passHash && user.IsNewPassword)//eski şifre uyuşuyorsa ve şifre talebi varsa
                {
                    entity.NewPassword = null;
                    entity.IsNewPassword = false;
                    _userRepository.Update(entity);
                    _uow.SaveChanges();
                }
                else if (user.IsNewPassword && user.Password != passHash)//yeni şifre talebi var ve şifre uyuşuyorsa
                {
                    entity.Password = entity.NewPassword;
                    entity.NewPassword = null;
                    entity.IsNewPassword = false;
                    _userRepository.Update(entity);
                    _uow.SaveChanges();
                }

                user.Password = "";
                user.NewPassword = "";
                //Kullanıcının bağlı olduğu birimin parentId değer var mı ? Sayfalarda filtreleme yaparken müdürlük veya proje ayrışması yapılması için
                var userUnit = _unitRepository.Find(user.UnitId.Value);
                if (user.UnitId != null && userUnit.ParentId != null) //Proje bazında yetkisi var
                {
                    user.UnitId = user.UnitId;
                    user.ParentUnitId = userUnit.ParentId;
                }
                else //Müdürlük bazlı yetkisi var demektir
                {
                    user.ParentUnitId = user.UnitId;
                    user.UnitId = null;
                }
            }

            return user;
        }
        public User Find(int id)
        {
            return _userRepository.Find(id);
        }
        public User FindByMail(string mail) => _userRepository.FirstOrDefault(f => f.Email == mail);
        public User FindByPhone(string phone) => _userRepository.FirstOrDefault(f => f.MobilePhone == phone);
        public List<EAuthorizationDto> GetWithUserRoleById(int userId)
        {
            var result = (from u in _userRepository.GetAll()
                          join ur in _userRoleRepository.GetAll() on u.Id equals ur.UserId
                          where u.Id == userId
                          select new EAuthorizationDto
                          {
                              UserId = u.Id,
                              Name = u.Name
                          }).ToList();

            return result;
        }
        //Message tablosuna kayıt atar
        public void InsertMessageTable(Message model)
        {
            try
            {
                var userList = _userRoleRepository.GetAll().Where(w => w.Status == true).ToList();
                foreach (var item in userList)
                {
                    model.UserId = item.UserId;
                    model.UnRead = false;
                    _messageRepository.Insert(model);
                }
                _uow.SaveChanges();
            }
            catch (Exception e)
            {
            }
        }
        public async Task<EResultDto> InsertDeviceAync(Device entity)
        {
            var result = new EResultDto();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var oldDeviceList = await Task.FromResult(_deviceRepository.Where(w => w.UserId == entity.UserId && w.Status).ToList());
                    oldDeviceList.ForEach(f => f.Status = false);
                    _deviceRepository.UpdateRange(oldDeviceList);

                    entity.CreatedBy = 1;
                    entity.CreatedDate = DateTime.Now;
                    await _deviceRepository.InsertAsync(entity);
                    await _uow.SaveChangesAsync();

                    scope.Complete();
                    result.Id = entity.Id;
                    result.IsSuccess = true;
                    result.Message = "Kayıt başarıyla eklenmiştir";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return result;
        }

        #region lisans

        public PagedList<LicenceKeyDto> GetAllLicenceWithPaged(int? page, EUserDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = _dbLicenceContext.LicenceKey.Select(m => new LicenceKeyDto
            {
                Id = m.Id,
                StatusName = m.IsActive
                               ? "<span class='label bg-green-300'>Aktif</span>"
                               : "<span class='label bg-danger-800'>Pasif</span>",
                LockName = !m.IsLock
                               ? "<span class='label bg-green-300'>Kilitli Değil</span>"
                               : "<span class='label bg-danger-800'>Kilitli</span>",
                PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                FirmName = m.FirmName,
                FirmLongName = m.FirmLongName,
                MaxUserCount = m.MaxUserCount,
                ActiveUserCount = m.ActiveUserCount,
                MaxVehicleCount = m.MaxVehicleCount,
                ActiveVehicleCount = m.ActiveVehicleCount,
                LockDate = m.LockDate,
                ErrorMessage = m.ErrorMessage,
                CustomButton =
                               "<li title='Lisans düzenle' class='text-primary-400'><a data-toggle='modal' onclick='funcEditLicence(" + m.Id + ");'><i class='icon-pencil5'></i></a></li>" +
                                           "<li title='Lisans sil' class='text-danger'><a onclick='funcDeleteLicence(" + m.Id + ");'><i class='icon-trash'></i></a></li>"
            }).AsQueryable();

            var lastResult = new PagedList<LicenceKeyDto>(list.OrderBy(o => o.EndDate), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }

        public EResultDto LicenceInsert(LicenceKeyDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var entity = _mapper.Map<LicenceKey>(tempModel);
                entity.CreatedDate = DateTime.Now;
                entity.FirmKey = Guid.NewGuid().ToString();
                entity.ActiveVehicleCount = 0;
                entity.ActiveUserCount = 0;
                entity.CreatedBy = tempModel.UpdatedBy;
                _dbLicenceContext.LicenceKey.Add(entity);
                _dbLicenceContext.SaveChanges();
                result.Message = "Kayıt Başarılı";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }
        public EResultDto LicenceUpdate(LicenceKeyDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var oldEntity = _dbLicenceContext.LicenceKey.Find(tempModel.Id);
                var oldLog = JsonConvert.SerializeObject(oldEntity);

                var entity = _mapper.Map<LicenceKeyDto, LicenceKey>(tempModel, oldEntity);
                _dbLicenceContext.LicenceKey.Update(entity);

                var log = new WebLog()
                {
                    UpdatedBy = tempModel.UpdatedBy,
                    CreatedDate = DateTime.Now,
                    OldLog = oldLog,
                    NewLog = JsonConvert.SerializeObject(entity)
                };
                _dbLicenceContext.WebLog.Add(log);
                _dbLicenceContext.SaveChanges();
                result.Message = "Güncelleme Başarılı";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public EResultDto LicenceDeleteUser(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _dbLicenceContext.LicenceKey.FirstOrDefault(f => f.IsActive && f.Id == id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    _dbLicenceContext.LicenceKey.Update(entity);
                    _dbLicenceContext.SaveChanges();
                    result.Message = "İşlem Başarılı";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Lisans bulunamadı";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }

        public LicenceKeyDto LicenceGetById(int id)
        {
            var entity = _dbLicenceContext.LicenceKey.FirstOrDefault(f => f.Id == id);
            var result = _mapper.Map<LicenceKeyDto>(entity);
            return result;
        }
        #endregion
    }
}