using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.NoticeVehicle.Notice;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class ComboController : AdminController
    {
        private readonly IGenericRepository<Authorization> _authorizationRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Color> _colorRepository;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleBrandModel> _vehicleBrandRepository;
        private readonly IGenericRepository<VehicleModel> _vehicleModelRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<Tire> _tireRepository;
        private readonly IGenericRepository<TireDebit> _tireDebitRepository;
        private readonly IGenericRepository<Institution> _institutionRepository;
        private readonly IGenericRepository<TenderContact> _tenderContactRepository;
        private readonly IUnitOfWork _uow;

        public ComboController(IUnitOfWork uow)
        {
            _authorizationRepository = uow.GetRepository<Authorization>();
            _userRepository = uow.GetRepository<User>();
            _roleRepository = uow.GetRepository<Role>();
            _colorRepository = uow.GetRepository<Color>();
            _vehicleBrandRepository = uow.GetRepository<VehicleBrandModel>();
            _vehicleModelRepository = uow.GetRepository<VehicleModel>();
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _cityRepository = uow.GetRepository<City>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _unitRepository = uow.GetRepository<Unit>();
            _tireRepository = uow.GetRepository<Tire>();
            _tireDebitRepository = uow.GetRepository<TireDebit>();
            _institutionRepository = uow.GetRepository<Institution>();
            _tenderContactRepository = uow.GetRepository<TenderContact>();
            _uow = uow;
        }

        //Json combobox
        public async Task<IActionResult> GetUserCmbx(string q)
        {
            var list = await Task.FromResult(from u in _userRepository.GetAll()
                                             select new
                                             {
                                                 id = u.Id,
                                                 text = u.Name + " " + u.Surname + (u.MobilePhone != null ? "/" + u.MobilePhone : "")
                                             });

            var result = list.Where(w => w.text.Contains(q)).ToList();
            return Json(result);
        }
        public async Task<IActionResult> GetUserWithEmailCmbx(string q)
        {
            var list = await Task.FromResult(from u in _userRepository.GetAll()
                                             where u.Status
                                             select new
                                             {
                                                 id = u.Id,
                                                 text = u.Name + " " + u.Surname + "/" + u.Email
                                             });

            var result = list.Where(w => w.text.Contains(q)).ToList();
            return Json(result);
        }

        //Aracın üzerinde zimmetli kişi varsa listeler yoksa aracı listeler --> id = UserId
        public async Task<IActionResult> GetVehicleDebitUserCmbx(string q)
        {
            var list = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                             join u in _userRepository.GetAll() on v.LastUserId equals u.Id into uL
                                             from u in uL.DefaultIfEmpty()
                                             where v.Status
                                             select new
                                             {
                                                 id = u.Id,
                                                 text = v.Plate + (u.Name != null ? " (" + u.Name + " " + u.Surname + " " + u.MobilePhone + ")" : "")
                                             });

            var result = list.Where(w => w.text.Contains(q)).Select(s => new { s.id, s.text }).ToList();
            return Json(result);
        }

        //Aracın üzerinde zimmetli kişi varsa listeler yoksa aracı listeler --> id = VehicleId
        public async Task<IActionResult> GetVehicleDebitUserCmbx2(string q)
        {
            var result = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                               join u in _userRepository.GetAll() on v.LastUserId equals u.Id into uL
                                               from u in uL.DefaultIfEmpty()
                                               where v.Plate.Contains(q)
                                               select new ESelect2Dto()
                                               {
                                                   id = v.Id,
                                                   VehicleId = v.Id,
                                                   text = v.Plate + (u.Name != null ? " (" + u.Name + " " + u.Surname + "/" + u.MobilePhone + ")" : "")
                                               });

            // var result = list.Where(w => w.text.Contains(q));


            var addPlate = SamePlateForNewName(result.ToList()); //Aynı plakaların önüne id ekelniyor
            return Json(addPlate.Select(s => new { s.id, s.text }).ToList());
        }

        //Select2 json formatı
        public async Task<IActionResult> GetVehicleCmbx(string q, bool isActive = false)
        {
            var isAdmin = _loginUserInfo.IsAdmin;
            var result = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                               join u in _unitRepository.GetAll() on v.LastUnitId equals u.Id into unitL
                                               from u in unitL.DefaultIfEmpty()
                                               join u2 in _unitRepository.GetAll() on u.ParentId equals u2.Id into unit2L
                                               from u2 in unit2L.DefaultIfEmpty()
                                               where v.Plate.Contains(q)
                                               select new ESelect2Dto()
                                               {
                                                   id = v.Id,
                                                   Status = v.Status,
                                                   CreatedDate = v.CreatedDate,
                                                   text = v.Plate,
                                                   ParentUnitId = u2.Id,
                                                   VehicleId = v.Id,
                                                   UnitId = u.Id
                                               });

            if (isActive)//true değilse tümünü listele
                result = result.Where(w => w.Status);

            if (!_loginUserInfo.IsAdmin)
            {
                if (_loginUserInfo.UnitId == null) //Müdürlük yetkisi var demektir
                    result = result.Where(w => w.ParentUnitId == _loginUserInfo.ParentUnitId);
                else //Proje yetkisi var demektir
                    result = result.Where(w => w.UnitId == _loginUserInfo.UnitId);
            }

            var addPlate = SamePlateForNewName(result.ToList()); //Aynı plakaların önüne id ekelniyor
            return Json(addPlate.Select(s => new { s.id, s.text }).ToList());
        } //yetkili olduğu birime göre

        public async Task<IActionResult> GetAllActiveVehicleCmbx(string q)
        {
            var result = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                               where v.Status && v.Plate.Contains(q)
                                               select new ESelect2Dto()
                                               {
                                                   id = v.Id,
                                                   text = v.Plate,
                                               });

            return Json(result.Select(s => new { s.id, s.text }).ToList());
        } //Aktif tüm araçlar 

        public List<ESelect2Dto> SamePlateForNewName(List<ESelect2Dto> resultPlate)
        {
            var isSamePlate = _vehicleRepository.GetAll().Select(s => new Vehicle() { Plate = s.Plate }).ToList().GroupBy(g => g.Plate)
                .Select(s => new RVehicleFuelDto() { Count = s.Count(), Plate = s.First().Plate })
                .Where(w => w.Count > 1).OrderBy(o => o.VehicleId).ToList();

            foreach (var p in isSamePlate)
            {
                var samePlate = resultPlate.Where(w => w.text.Contains(p.Plate)).ToList();
                for (int i = 0; i < samePlate.Count; i++)
                {
                    var temp = resultPlate.FirstOrDefault(f => f.VehicleId == samePlate[i].VehicleId);
                    if (temp != null)
                        resultPlate.FirstOrDefault(f => f.VehicleId == temp.VehicleId).text += "-" + temp.VehicleId;
                }
            }

            return resultPlate.OrderBy(o => o.VehicleId).ToList();
        }

        //Normal combobox
        public async Task<IActionResult> GetVehicle2Cmbx()
        {
            var list = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                             where v.Status
                                             select new
                                             {
                                                 v.Id,
                                                 Name = v.Plate
                                             });
            return Json(list.ToList());
        }

        public async Task<IActionResult> GetColorCmbx()
        {
            var colors = await Task.FromResult(_colorRepository.GetAll()
                .Select(s => new { s.Id, s.Name }));
            return Json(colors.ToList());
        }

        //Normal combobox
        public async Task<IActionResult> GetRoleCmbx()
        {
            var list = await Task.FromResult(_roleRepository.GetAll()
                .Select(s => new
                {
                    s.Id,
                    s.Name
                }));
            return Json(list.ToList());
        }

        public async Task<IActionResult> GetBrandCmbx()
        {
            var brands = await Task.FromResult(from u in _vehicleBrandRepository.GetAll()
                                               join u2 in _vehicleBrandRepository.GetAll() on u.Id equals u2.ParentId
                                               select new EUnitDto
                                               {
                                                   Id = u2.Id,
                                                   Name = u.Name + " -> " + u2.Name
                                               });
            return Json(brands.ToList());
        }

        public async Task<IActionResult> GetBrandChildCmbx(int parentId)
        {
            var list = await Task.FromResult(_vehicleBrandRepository.GetAll().Where(w => w.Status && w.ParentId == parentId).Select(s => new
            {
                s.Id,
                s.Name
            }));

            if (!_loginUserInfo.IsAdmin && _loginUserInfo.UnitId > 0)
                list = list.Where(w => w.Id == _loginUserInfo.UnitId);

            return Json(list.ToList());
        }

        public async Task<IActionResult> GetUnitChildCmbx(int parentId)
        {
            var list = await Task.FromResult(_unitRepository.GetAll().Where(w => w.Status && w.ParentId == parentId).Select(s => new
            {
                s.Id,
                s.Name
            }));

            if (!_loginUserInfo.IsAdmin && _loginUserInfo.UnitId > 0)
                list = list.Where(w => w.Id == _loginUserInfo.UnitId);

            return Json(list.ToList());
        } //yetkili olduğu birime göre

        //Müdürlük ve proje kırılımlı listeler
        public async Task<IActionResult> GetUnitCmbx()
        {
            var list = await Task.FromResult(from u in _unitRepository.GetAll()
                                             join u2 in _unitRepository.GetAll() on u.Id equals u2.ParentId
                                             where u.Status && u2.Status
                                             select new EUnitDto
                                             {
                                                 Id = u2.Id,
                                                 Name = u.Name + " - " + u2.Name
                                             });

            return Json(list.ToList());
        }

        //Sadece müdürlük listeler
        public async Task<IActionResult> GetUnitJustParentCmbx()
        {
            var list = await Task.FromResult(from u in _unitRepository.GetAll()
                                             where u.ParentId == null && u.Status
                                             select new EUnitDto
                                             {
                                                 Id = u.Id,
                                                 Name = u.Name
                                             });

            if (!_loginUserInfo.IsAdmin)
                list = list.Where(w => w.Id == _loginUserInfo.ParentUnitId);

            return Json(list.ToList());
        } //yetkili olduğu birime göre

        //Tekil Birim veya alt kırılım
        public async Task<IActionResult> GetUnitAndParentCmbx(bool isTenderVisible = false)
        {
            var list = await Task.FromResult((from u in _unitRepository.GetAll()
                                              where u.ParentId == null && u.Status
                                              select new EUnitDto
                                              {
                                                  Id = u.Id,
                                                  Name = u.Name,
                                                  IsTenderVisible = u.IsTenderVisible
                                              }).ToList());

            var list2 = await Task.FromResult((from u in _unitRepository.GetAll()
                                               join u2 in _unitRepository.GetAll() on u.Id equals u2.ParentId
                                               where u.Status && u2.Status
                                               select new EUnitDto
                                               {
                                                   Id = u2.Id,
                                                   Name = u.Name + " - " + u2.Name,
                                                   IsTenderVisible = u2.IsTenderVisible
                                               }).ToList());

            var mergeList = list.Concat(list2).Distinct().OrderBy(o => o.Name);
            if (isTenderVisible)
                return Json(mergeList.Where(w => w.IsTenderVisible == true).ToList());
            else
                return Json(mergeList);
        }

        //Sadece şehir listeler
        public async Task<IActionResult> GetCityCmbx()
        {
            var cities = await Task.FromResult(from u in _cityRepository.GetAll()
                                               where u.ParentId == null && u.Status
                                               select new EUnitDto
                                               {
                                                   Id = u.Id,
                                                   Name = u.Name
                                               });
            return Json(cities.ToList());
        }

        //Şehir ve  bölgeyi kırılımlı listeler
        public async Task<IActionResult> GetCityAndDistrictCmbx()
        {
            var cities = await Task.FromResult(from u in _cityRepository.GetAll()
                                               join u2 in _cityRepository.GetAll() on u.Id equals u2.ParentId
                                               select new EUnitDto
                                               {
                                                   Id = u2.Id,
                                                   Name = u.Name + "-" + u2.Name
                                               });
            return Json(cities.ToList());
        }

        //async olması lazım
        public async Task<IActionResult> GetByLookUpListTypeId(int typeId)
        {
            var list = await _lookUpListRepository.WhereAsync(w => w.Type == typeId);
            return Json(list.OrderBy(o => o.Name).ToList());
        }

        public async Task<IActionResult> GetByLookUpListId(int lookUpId)
        {
            var list = await _lookUpListRepository.WhereAsync(w => w.Id == lookUpId);
            return Json(list.OrderBy(o => o.Name).ToList());
        }

        public IActionResult LookUpList()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var e in Enum.GetValues(typeof(TypeList)))
            {
                var name = e.ToString() switch
                {
                    "CriminialType" => "Trafik Cezası Türü",
                    "RentACarFirm" => "Araç Kiralama Firması",
                    "SupplierACar" => "Bakım/Onarım Tedarikçi Firma",
                    "MaintenancetType" => "İşlem Türü",
                    "FuelStationIdType" => "Benzin İstasyon Adı",
                    "DimensionTypeId" => "Araç Lastik Ebatı (örn: 195 55 16)",
                    "VehicleDeleteTypeId" => "Araç Silme Nedeni",
                    "VehicleMaterialType" => "Araca Zimmetli Malzeme (Yangın tüpü,Zincir vb)",
                    _ => ""
                };
                if (name != "")
                    list.Add(new KeyValuePair<string, int>(name, (int)e));
            }

            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }

        //Kiralık ve mülkiyet araçların tutar tipleri
        public IActionResult GetContractAmountType(int fixtureTypeId)
        {
            var list = new List<KeyValuePair<string, int>>();
            var name = "";
            foreach (var e in Enum.GetValues(typeof(VehicleAmountType)))
                if (fixtureTypeId == (int)FixtureType.ForRent)
                {
                    name = e.ToString() switch
                    {
                        "ArventoMaliyet" => "Arvento Maliyeti-Aylık",
                        "SimKartMaliyet" => "Sim Kart Maliyeti-Aylık",
                        "KiraBedeli" => "Kira Bedeli-Aylık",
                        "ExtraTutar" => "Extra Tutar",
                        _ => ""
                    };
                    if (name != "")
                        list.Add(new KeyValuePair<string, int>(name, (int)e));
                }
                else if (fixtureTypeId == (int)FixtureType.Ownership)
                {
                    name = e.ToString() switch
                    {
                        "ArventoMaliyet" => "Arvento Maliyeti-Aylık",
                        "SimKartMaliyet" => "Sim Kart Maliyeti-Aylık",
                        "AracMaliyet" => "Araç Bedeli",
                        //"KiraBedeli" => "Kira Bedeli",
                        _ => ""
                    };
                    if (name != "")
                        list.Add(new KeyValuePair<string, int>(name, (int)e));
                }

            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }

        //Depolardaki boş lastik sayıları
        public async Task<IActionResult> GetWareHouseEmptyTireCount(ETireDto model)
        {
            var list = await Task.FromResult((from t in _tireRepository.GetAll()
                                              join l in _lookUpListRepository.GetAll() on t.DimensionTypeId equals l.Id
                                              where t.WareHouseId != null && t.State == (int)TireState.InHouse && t.WareHouseId == model.WareHouseId
                                              select new ETireDto()
                                              {
                                                  DimensionTypeId = t.DimensionTypeId,
                                                  DimensionTypeName = l.Name
                                              }).ToList());

            var result = list.GroupBy(g => g.DimensionTypeId).Select(s => new EUnitDto()
            {
                Id = s.First().DimensionTypeId,
                Name = s.First().DimensionTypeName + " (" + s.Count() + " Adet Kullanılabilir)"
            }).ToList();

            return Json(result);
        }
        //Depolardaki boş lastikleri türü
        public async Task<IActionResult> GetWareHouseInTireTypeCount(ETireDto model)
        {
            var list = await Task.FromResult((from t in _tireRepository.GetAll()
                                              join l in _lookUpListRepository.GetAll() on t.TireTypeId equals l.Id
                                              where t.WareHouseId != null && t.State == (int)TireState.InHouse && t.WareHouseId == model.WareHouseId && t.DimensionTypeId == model.DimensionTypeId
                                              select new ETireDto()
                                              {
                                                  TireTypeId = t.TireTypeId,
                                                  TireTypeName = l.Name
                                              }).ToList());

            var result = list.GroupBy(g => g.TireTypeId).Select(s => new EUnitDto()
            {
                Id = s.First().TireTypeId,
                Name = s.First().TireTypeName + " (" + s.Count() + " Adet Kullanılabilir)"
            }).ToList();

            return Json(result);
        }

        public async Task<IActionResult> GetAttachedTirePlateList()
        {
            var plates = await Task.FromResult((from v in _vehicleRepository.GetAll()
                                                join t in _tireRepository.GetAll() on v.Id equals t.VehicleId into tL
                                                from t in tL.DefaultIfEmpty()
                                                join td in _tireDebitRepository.GetAll() on v.Id equals td.VehicleId into tdL
                                                from td in tdL.DefaultIfEmpty()
                                                join l in _lookUpListRepository.GetAll() on t.DimensionTypeId equals l.Id into lL
                                                from l in lL.DefaultIfEmpty()
                                                join l2 in _lookUpListRepository.GetAll() on t.TireTypeId equals l2.Id into l2L
                                                from l2 in l2L.DefaultIfEmpty()
                                                select new EUnitDto()
                                                {
                                                    Id = v.Id,
                                                    Name = t.Id > 0 ? (v.Plate + " (Ebat: " + l.Name + "- <b>Tipi:</b> " + l2.Name + "- Adet: " + td.AttachedTireCount + ")") : v.Plate
                                                }).Distinct().OrderByDescending(o => o.Name.Length).ToList());
            return Json(plates);
        }

        //Ptt tedarikçi firmanın türleri
        public IActionResult HgsTypeList()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var e in Enum.GetValues(typeof(HgsType)))
            {
                var result = _lookUpListRepository.Find((int)e);
                if (result != null)
                    list.Add(new KeyValuePair<string, int>(result.Name, (int)e));
            }
            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }
        public IActionResult GetMessageLogType()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var e in Enum.GetValues(typeof(MessageLogType)))
            {
                var name = e.ToString() switch
                {
                    "EMail" => "E-Mail",
                    "PushNotification" => "Push Notification",
                    "Sms" => "Sms",
                    _ => ""
                };
                if (name != "")
                    list.Add(new KeyValuePair<string, int>(name, (int)e));
            }

            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }

        #region Notice

        public IActionResult GetNoticeType()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var e in Enum.GetValues(typeof(NoticeType)))
            {
                var name = e.ToString() switch
                {
                    "Speed" => "Hız İhlali",
                    //"OutOfHours" => "Mesai Dışı Kullanım",
                    "Duty" => "Görev Analizi",
                    _ => ""
                };
                if (name != "")
                    list.Add(new KeyValuePair<string, int>(name, (int)e));
            }
            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }

        public IActionResult GetNoticeUnitType()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var e in Enum.GetValues(typeof(NoticeType)))
            {
                var name = e.ToString() switch
                {
                    "Speed" => "Hız İhlali",
                    //"OutOfHours" => "Mesai Dışı Kullanım",
                    "Duty" => "Görev Analizi",
                    _ => ""
                };
                if (name != "")
                    list.Add(new KeyValuePair<string, int>(name, (int)e));
            }
            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }
        #endregion

        #region Tender
        public IActionResult LookUpListTender()
        {
            var list = new List<KeyValuePair<string, int>>();
            foreach (var e in Enum.GetValues(typeof(TypeList)))
            {
                var name = e.ToString() switch
                {
                    "TenderStageType" => "Teklif Aşama",
                    "MoneyType" => "Para Birimi",
                    _ => ""
                };
                if (name != "")
                    list.Add(new KeyValuePair<string, int>(name, (int)e));
            }

            return Json(list.Select(s => new { Id = s.Value, Name = s.Key }));
        }
        public async Task<IActionResult> GetInstitutionChildCmbx(EUnitDto filter)
        {
            var list = await GetInstitution();
            if (filter.ParentId > 0)
                list = list.Where(w => w.ParentId == filter.ParentId).ToList();

            if (!_loginUserInfo.IsAdmin && _loginUserInfo.UnitId > 0)
                list = list.Where(w => w.Id == _loginUserInfo.UnitId).ToList();

            return Json(list.ToList());
        }

        public async Task<IActionResult> GetInstitutionAddManagerCmbx(int? id = null, string q = "")//Select2 json için Kurum + Müdürlük
        {
            var inst = GetInstitution(q).Result.Where(w => w.ParentId == null);
            var instWithManager = await GetInstitutionWithManager(q);

            var list = (inst.Concat(instWithManager).ToList())
                .Where(w => w.Name.ToLower().Contains(q))
                .Select(s => new ESelect2Dto()
                {
                    id = s.Id,
                    text = s.Name,
                }).OrderBy(o => o.text).ToList();

            if (!_loginUserInfo.IsAdmin && _loginUserInfo.UnitId > 0)
                list = list.Where(w => w.id == _loginUserInfo.UnitId).ToList();
            if (id > 0)
                list = list.Where(w => w.id == id).ToList();
            return Json(list);
        }

        public async Task<List<EUnitDto>> GetInstitutionWithManager(string q = null) //Müdürlük
        {
            var list = await Task.FromResult(from u in _institutionRepository.GetAll()
                                             join u2 in _institutionRepository.GetAll() on u.Id equals u2.ParentId
                                             where u.Status && u2.Status
                                             select new EUnitDto
                                             {
                                                 Id = u2.Id,
                                                 Name = u.Name + " - " + u2.Name
                                             });

            if (!string.IsNullOrEmpty(q))
                list = list.Where(w => w.Name.ToLower().Contains(q));

            return list.ToList();
        }
        public async Task<List<EUnitDto>> GetInstitution(string q = null) //Tümü
        {
            var list = await Task.FromResult((await _institutionRepository.WhereAsync(w => w.Status == true))
                .Select(s => new EUnitDto()
                {
                    Id = s.Id,
                    ParentId = s.ParentId,
                    Name = s.Name
                }));

            if (!string.IsNullOrEmpty(q))
                list = list.Where(w => w.Name.ToLower().Contains(q));

            return list.ToList();
        }
        public async Task<List<ESelect2Dto>> GetTenderContactCmbx(int? id = null, string q = "")
        {
            var list = await Task.FromResult((await _tenderContactRepository.WhereAsync(w => w.Status == true))
                .Select(s => new ESelect2Dto()
                {
                    id = s.Id,
                    text = s.Name + " " + s.Surname + "/" + s.Phone,
                    Name = s.Name,
                    Surname = s.Surname,
                    Phone = s.Phone,
                    Email = s.Email
                }));

            if (!string.IsNullOrEmpty(q))
                list = list.Where(w => w.text.ToLower().Contains(q));
            if (id > 0)
                list = list.Where(w => w.id == id);
            return list.ToList();
        }
        #endregion
    }
}