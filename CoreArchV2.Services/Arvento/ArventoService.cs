using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.NoticeVehicle.Notice;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.NoticeVehicle.Notice;
using CoreArchV2.Core.Enum.TripVehicle;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Xml;


namespace CoreArchV2.Services.Arvento
{
    public class ArventoService : IArventoService
    {
        string rootUrl = "http://ws.arvento.com/v1/report.asmx/";
        string userName = string.Empty;
        string password = string.Empty;

        private readonly IUnitOfWork _uow;
        private readonly IMailService _mailService;
        private readonly IReportService _reportService;
        private readonly IMobileService _mobileService;
        private readonly ArventoSetting _arventoSetting;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<Notice> _noticeRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<TaskScheduler_> _jobRepository;
        private readonly IGenericRepository<Parameter> _parameterRepository;
        private readonly IGenericRepository<VehicleOperatingReportParam> _vehicleOpParam;
        private readonly IGenericRepository<VehicleCoordinate> _vehicleCoordinateRepository;
        private readonly IGenericRepository<VehicleOperatingReport> _vehicleOperationReportRepository;
        public ArventoService(IUnitOfWork uow,
            IReportService reportService,
            IMobileService mobileService,
            IOptions<ArventoSetting> arventoSetting,
            IMailService mailService)
        {
            _uow = uow;
            _mailService = mailService;
            _reportService = reportService;
            _mobileService = mobileService;
            _arventoSetting = arventoSetting.Value;
            userName = _arventoSetting.UserName;
            password = _arventoSetting.Password;
            _unitRepository = uow.GetRepository<Unit>();
            _tripRepository = uow.GetRepository<Trip>();
            _cityRepository = uow.GetRepository<City>();
            _userRepository = uow.GetRepository<User>();
            _noticeRepository = uow.GetRepository<Notice>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _jobRepository = uow.GetRepository<TaskScheduler_>();
            _parameterRepository = uow.GetRepository<Parameter>();
            _vehicleOpParam = uow.GetRepository<VehicleOperatingReportParam>();
            _vehicleCoordinateRepository = uow.GetRepository<VehicleCoordinate>();
            _vehicleOperationReportRepository = uow.GetRepository<VehicleOperatingReport>();
        }

        #region Arvento Metotlar
        //Node göre plaka listeler
        public EPlateFromNodeDto GetLicensePlateFromNode(string node)
        {
            var url = rootUrl + "GetLicensePlateFromNode";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            var data = new
            {
                Username = userName,
                PIN1 = password,
                PIN2 = password,
                Node = node
            };

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(data));
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    var serializer = new JsonSerializer();
                    var result = serializer.Deserialize<EPlateFromNodeDto>(jsonTextReader);
                    return result;
                }
            }
        }

        //Verilen tarihe göre plakanın koordinatlarını listeler
        public List<EGeneralReport2Dto> GeneralReport(DateTime start, DateTime end, string node)
        {
            try
            {
                var url = rootUrl + $"GeneralReport2?Username={userName}&PIN1={password}&PIN2={password}&" +
             "StartDate=" + start.ToString("MMddyyyyHHmmss") +
             "&EndDate=" + end.ToString("MMddyyyyHHmmss") +
             "&Node=" + node +
             "&Group=&Compress=&chkLocation=1&chkSpeed=1&chkPause=1&chkMotion=1&chkRegion=1&txtSpeedMin=&txtSpeedMax=&chkTemperatureSensor1=1&chkTemperatureSensorPer1=1&chkTemperatureSensorAlm1=1&chkTemperatureSensor2=1&chkTemperatureSensorPer2=1&chkTemperatureSensorAlm2=1&txtTemperatureMin=&txtTemperatureMax=&chkEmergency=1&chkDoor=1&chkPauseTime=1&chkContactAlarm=1&chkIdlingTime=1&chkIdlingAlarm=1&chkFuelLevel=1&chkPower=1&chkDriverIdentification=1&chkPossibleAccident=1&chkAcceleration=1&chkVehicleMovedWithoutDriverCard=1&MinuteDif=UTC+02:00&Language=0";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                List<EGeneralReport2Dto> list = new List<EGeneralReport2Dto>();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(result);
                    foreach (XmlNode child in doc.ChildNodes)
                    {
                        foreach (XmlNode child2 in child.ChildNodes)
                        {
                            foreach (XmlNode child3 in child2.ChildNodes)
                            {
                                if (child3.Name == "NewDataSet")
                                {
                                    foreach (XmlNode item in child3.ChildNodes)
                                    {
                                        if (item.SelectSingleNode("Enlem") != null && item.SelectSingleNode("Boylam") != null)
                                        {
                                            var model = new EGeneralReport2Dto();
                                            if (item.SelectSingleNode("Kayıt_x0020_No") != null && item.SelectSingleNode("Kayıt_x0020_No").FirstChild != null)
                                                model.KayitNo = Convert.ToInt32(item.SelectSingleNode("Kayıt_x0020_No").FirstChild.Value);
                                            if (item.SelectSingleNode("Cihaz_x0020_No") != null && item.SelectSingleNode("Cihaz_x0020_No").FirstChild != null)
                                                model.CihazNo = item.SelectSingleNode("Cihaz_x0020_No").FirstChild.Value;
                                            if (item.SelectSingleNode("Plaka") != null && item.SelectSingleNode("Plaka").FirstChild != null)
                                                model.Plaka = item.SelectSingleNode("Plaka").FirstChild.Value.Replace(" ", "");
                                            if (item.SelectSingleNode("Sürücü") != null && item.SelectSingleNode("Sürücü").FirstChild != null)
                                                model.Surucu = item.SelectSingleNode("Sürücü").FirstChild.Value;
                                            if (item.SelectSingleNode("Tarih_x002F_Saat") != null && item.SelectSingleNode("Tarih_x002F_Saat").FirstChild != null)
                                                model.Tarih = Convert.ToDateTime(Convert.ToDateTime(item.SelectSingleNode("Tarih_x002F_Saat").FirstChild.Value).ToString("dd.MM.yyyy HH:mm"));
                                            if (item.SelectSingleNode("Tür") != null && item.SelectSingleNode("Tür").FirstChild != null)
                                                model.Tur = item.SelectSingleNode("Tür").FirstChild.Value;
                                            if (item.SelectSingleNode("Hız_x0020_km_x002F_s") != null && item.SelectSingleNode("Hız_x0020_km_x002F_s").FirstChild != null)
                                                model.Hiz = item.SelectSingleNode("Hız_x0020_km_x002F_s").FirstChild.Value;
                                            if (item.SelectSingleNode("Adres") != null && item.SelectSingleNode("Adres").FirstChild != null)
                                                model.Adres = item.SelectSingleNode("Adres").FirstChild.Value;

                                            model.Enlem = item.SelectSingleNode("Enlem").FirstChild.Value;
                                            model.Boylam = item.SelectSingleNode("Boylam").FirstChild.Value;

                                            if (item.SelectSingleNode("Duraklama_x0020_Süresi") != null && item.SelectSingleNode("Duraklama_x0020_Süresi").FirstChild != null)
                                                model.DuraklamaSuresi = item.SelectSingleNode("Duraklama_x0020_Süresi").FirstChild.Value;
                                            list.Add(model);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var groupCoordinate = list.GroupBy(g => new { g.Enlem, g.Boylam }).ToList().Select(s => new EGeneralReport2Dto()
                {
                    Enlem = s.Key.Enlem,
                    Boylam = s.Key.Boylam,
                    Plaka = s.FirstOrDefault().Plaka,
                    Surucu = s.FirstOrDefault().Surucu,
                    Tarih = s.OrderBy(o => o.Tarih).FirstOrDefault().Tarih,
                    Hiz = s.FirstOrDefault().Hiz,
                    DuraklamaSuresi = s.OrderByDescending(o => o.KayitNo).First().DuraklamaSuresi
                }).ToList().GroupBy(a => a.Tarih).ToList().Select(b => new EGeneralReport2Dto()
                {
                    Enlem = b.First().Enlem,
                    Boylam = b.First().Boylam,
                    Plaka = b.First().Plaka,
                    Surucu = b.First().Surucu,
                    Tarih = b.First().Tarih,
                    Hiz = b.First().Hiz,
                    DuraklamaSuresi = b.First().DuraklamaSuresi
                }).ToList();

                return groupCoordinate;
            }
            catch (Exception)
            {
                var ss = 5;
            }
            return new List<EGeneralReport2Dto>();
        }

        //Verilen günler arasındaki koordinatları çeker !!Manuel kullanım için
        public void InsertPlateCoordinateRange()
        {
            try
            {
                var vehicleList = _vehicleRepository.Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo)).ToList();
                var startDate1 = new DateTime(2022, 07, 17);
                var endDate1 = DateTime.Now.Date;

                int days = (int)(endDate1 - startDate1).TotalDays;
                for (int i = 0; i < days; i++)
                {
                    var startDate2 = startDate1.AddDays(i);
                    var endDate2 = startDate1.AddDays(i + 1);
                    foreach (var item in vehicleList)
                    {
                        var start = DateTime.Now;
                        var coordinateList = GeneralReport(startDate2, endDate2, item.ArventoNo);
                        if (coordinateList.Any())
                        {
                            coordinateList = coordinateList.OrderBy(o => o.Tarih).ToList();
                            var list = new List<VehicleCoordinate>();

                            foreach (var co in coordinateList)
                            {
                                list.Add(new VehicleCoordinate()
                                {
                                    VehicleId = item.Id,
                                    Latitude = co.Enlem,
                                    Longitude = co.Boylam,
                                    Speed = co.Hiz,
                                    Driver = co.Surucu,
                                    LocalDate = co.Tarih
                                });
                            }
                            var entities = list.OrderBy(o => o.LocalDate).ToList();
                            _vehicleCoordinateRepository.InsertRange(entities);
                            _uow.SaveChanges();

                            var end = DateTime.Now;
                            int diffSeconds = (int)(end - start).TotalSeconds;//arvento 30 sn'de bir istek kabul ediyor
                            if (diffSeconds < 30)
                                Thread.Sleep((30 - diffSeconds) * 1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(30000);
            }
        }

        public async Task<VehicleOperatingReport> VehicleOperatingReport(string node, DateTime start, DateTime end)
        {
            var model = new VehicleOperatingReport();
            try
            {
                var url = rootUrl + $"VehicleOperatingReport?Username={userName}&PIN1={password}&PIN2={password}&" +
                 "StartDate=" + start.ToString("MMddyyyyHHmmss") +
                 "&EndDate=" + end.ToString("MMddyyyyHHmmss") +
                 "&Node=" + node +
                 "&Group=&Compress=&Locale=&Language=0&ShowDayByDay=true&ShowLastLocationInformation=true&ShowDistance=true&ShowStandStill=true&ShowIdling=true&ShowIgnition=true&ShowMaxSpeed=true&ShowAlarmCounts=true&ShowAlarmInformation=true&ShowMotionDuration=true";

                using var client = new HttpClient();
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                XmlDocument docX = new XmlDocument();
                docX.LoadXml(result);

                var item = docX.GetElementsByTagName("Calisma")[0];
                if (item != null)
                {
                    model.Tarih = Convert.ToDateTime(item.SelectSingleNode("Tarih")?.FirstChild.Value);
                    model.ArventoNo = item.SelectSingleNode("Cihaz_x0020_No")?.FirstChild.Value;
                    model.ArventoPlaka = item.SelectSingleNode("Plaka")?.FirstChild.Value.Replace(" ", "").ToUpper();
                    model.MesafeKm = item.SelectSingleNode("Mesafe_x0020_km")?.FirstChild.Value;
                    model.DuraklamaSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Duraklama_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                    model.DuraklamaSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Duraklama_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                    model.DuraklamaSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Duraklama_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                    model.RolantiSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Rölanti_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                    model.RolantiSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Rölanti_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                    model.RolantiSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Rölanti_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                    model.HareketSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Hareket_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                    model.HareketSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Hareket_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                    model.HareketSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Hareket_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                    model.KontakAcikKalmaSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Kontak_x0020_Açık_x0020_Kalma_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                    model.KontakAcikKalmaSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Kontak_x0020_Açık_x0020_Kalma_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                    model.KontakAcikKalmaSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Kontak_x0020_Açık_x0020_Kalma_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                    model.MaxHiz = item.SelectSingleNode("Maksimum_x0020_Hız_x0020_km_x002F_s")?.FirstChild.Value;
                    model.AracSonDurumBilgileri = item.SelectSingleNode("Araç_x0020_Son_x0020_Durum_x0020_Bilgileri")?.FirstChild.Value;
                    model.HizAlarm = item.SelectSingleNode("Hız_x0020_Alarmı")?.FirstChild.Value;
                    model.SehirIciHizAlarm = item.SelectSingleNode("Şehir_x0020_İçi_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                    model.SehirDisiHizAlarm = item.SelectSingleNode("Şehir_x0020_Dışı_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                    model.OtoyolHizAlarm = item.SelectSingleNode("Otoyol_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                    model.RolantiAlarm = item.SelectSingleNode("Rölanti_x0020_Alarmı")?.FirstChild.Value;
                    model.DuraklamaAlarm = item.SelectSingleNode("Duraklama_x0020_Alarmı")?.FirstChild.Value;
                    model.HareketAlarm = item.SelectSingleNode("Hareket_x0020_Alarmı")?.FirstChild.Value;
                    model.KontakAcildiAlarm = item.SelectSingleNode("Kontak_x0020_Açıldı_x0020_Alarmı")?.FirstChild.Value;
                    model.KontakKapandiAlarm = item.SelectSingleNode("Kontak_x0020_Kapandı_x0020_Alarmı")?.FirstChild.Value;
                    model.AniHizlanmaAlarm = item.SelectSingleNode("Ani_x0020_Hızlanma_x0020_Alarmı")?.FirstChild.Value;
                    model.AniYavaslamaAlarm = item.SelectSingleNode("Ani_x0020_Yavaşlama_x0020_Alarmı")?.FirstChild.Value;
                    model.MotorDevirAsimiAlarm = item.SelectSingleNode("Motor_x0020_Devir_x0020_Aşımı_x0020_Alarmı")?.FirstChild.Value;
                    model.IlkKontakAcildi = item.SelectSingleNode("İlk_x0020_Kontak_x0020_Açıldı_x0020_Alarmı")?.FirstChild.Value;
                    model.SonKontakKapandi = item.SelectSingleNode("Son_x0020_Kontak_x0020_Kapandı_x0020_Alarmı")?.FirstChild.Value;
                    model.SurucuTanimaBirimi = item.SelectSingleNode("Sürücü_x0020_Tanıma_x0020_Birimi")?.FirstChild.Value;
                    model.SonHizAlarm = item.SelectSingleNode("Son_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                    model.SonDuraklamaAlarm = item.SelectSingleNode("Son_x0020_Duraklama_x0020_Alarmı")?.FirstChild.Value;
                }

            }
            catch (Exception) { }

            return model;
        }

        //Birden fazla çalışma gelirse. (Not: Tarih aralığı 2 gün olursa birden fazla calisma xml dönüyor)
        public async Task<VehicleOperatingReport> VehicleOperatingReportMultipleCalisma(string node, DateTime start, DateTime end)
        {
            var list = new List<VehicleOperatingReport>();
            try
            {
                var url = rootUrl + $"VehicleOperatingReport?Username={userName}&PIN1={password}&PIN2={password}&" +
                 "StartDate=" + start.ToString("MMddyyyyHHmmss") +
                 "&EndDate=" + end.ToString("MMddyyyyHHmmss") +
                 "&Node=" + node +
                 "&Group=&Compress=&Locale=&Language=0&ShowDayByDay=true&ShowLastLocationInformation=true&ShowDistance=true&ShowStandStill=true&ShowIdling=true&ShowIgnition=true&ShowMaxSpeed=true&ShowAlarmCounts=true&ShowAlarmInformation=true&ShowMotionDuration=true";

                using var client = new HttpClient();
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                XmlDocument docX = new XmlDocument();
                docX.LoadXml(result);

                var tagList = docX.GetElementsByTagName("Calisma");
                for (int i = 0; i < tagList.Count; i++)
                {
                    var item = tagList[i];
                    if (item != null)
                    {
                        var model = new VehicleOperatingReport();
                        model.Tarih = Convert.ToDateTime(item.SelectSingleNode("Tarih")?.FirstChild.Value);
                        model.ArventoNo = item.SelectSingleNode("Cihaz_x0020_No")?.FirstChild.Value;
                        model.ArventoPlaka = item.SelectSingleNode("Plaka")?.FirstChild.Value.Replace(" ", "").ToUpper();
                        model.MesafeKm = item.SelectSingleNode("Mesafe_x0020_km")?.FirstChild.Value;
                        model.DuraklamaSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Duraklama_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                        model.DuraklamaSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Duraklama_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                        model.DuraklamaSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Duraklama_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                        model.RolantiSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Rölanti_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                        model.RolantiSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Rölanti_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                        model.RolantiSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Rölanti_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                        model.HareketSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Hareket_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                        model.HareketSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Hareket_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                        model.HareketSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Hareket_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                        model.KontakAcikKalmaSuresiSaat = Convert.ToInt32(item.SelectSingleNode("Kontak_x0020_Açık_x0020_Kalma_x0020_Süresi_x0020_sa")?.FirstChild.Value);
                        model.KontakAcikKalmaSuresiDakika = Convert.ToInt32(item.SelectSingleNode("Kontak_x0020_Açık_x0020_Kalma_x0020_Süresi_x0020_dak")?.FirstChild.Value);
                        model.KontakAcikKalmaSuresiSaniye = Convert.ToInt32(item.SelectSingleNode("Kontak_x0020_Açık_x0020_Kalma_x0020_Süresi_x0020_sn")?.FirstChild.Value);
                        model.MaxHiz = item.SelectSingleNode("Maksimum_x0020_Hız_x0020_km_x002F_s")?.FirstChild.Value;
                        model.AracSonDurumBilgileri = item.SelectSingleNode("Araç_x0020_Son_x0020_Durum_x0020_Bilgileri")?.FirstChild.Value;
                        model.HizAlarm = item.SelectSingleNode("Hız_x0020_Alarmı")?.FirstChild.Value;
                        model.SehirIciHizAlarm = item.SelectSingleNode("Şehir_x0020_İçi_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                        model.SehirDisiHizAlarm = item.SelectSingleNode("Şehir_x0020_Dışı_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                        model.OtoyolHizAlarm = item.SelectSingleNode("Otoyol_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                        model.RolantiAlarm = item.SelectSingleNode("Rölanti_x0020_Alarmı")?.FirstChild.Value;
                        model.DuraklamaAlarm = item.SelectSingleNode("Duraklama_x0020_Alarmı")?.FirstChild.Value;
                        model.HareketAlarm = item.SelectSingleNode("Hareket_x0020_Alarmı")?.FirstChild.Value;
                        model.KontakAcildiAlarm = item.SelectSingleNode("Kontak_x0020_Açıldı_x0020_Alarmı")?.FirstChild.Value;
                        model.KontakKapandiAlarm = item.SelectSingleNode("Kontak_x0020_Kapandı_x0020_Alarmı")?.FirstChild.Value;
                        model.AniHizlanmaAlarm = item.SelectSingleNode("Ani_x0020_Hızlanma_x0020_Alarmı")?.FirstChild.Value;
                        model.AniYavaslamaAlarm = item.SelectSingleNode("Ani_x0020_Yavaşlama_x0020_Alarmı")?.FirstChild.Value;
                        model.MotorDevirAsimiAlarm = item.SelectSingleNode("Motor_x0020_Devir_x0020_Aşımı_x0020_Alarmı")?.FirstChild.Value;
                        model.IlkKontakAcildi = item.SelectSingleNode("İlk_x0020_Kontak_x0020_Açıldı_x0020_Alarmı")?.FirstChild.Value;
                        model.SonKontakKapandi = item.SelectSingleNode("Son_x0020_Kontak_x0020_Kapandı_x0020_Alarmı")?.FirstChild.Value;
                        //model.SurucuTanimaBirimi = item.SelectSingleNode("Sürücü_x0020_Tanıma_x0020_Birimi")?.FirstChild.Value;
                        //model.SonHizAlarm = item.SelectSingleNode("Son_x0020_Hız_x0020_Alarmı")?.FirstChild.Value;
                        //model.SonDuraklamaAlarm = item.SelectSingleNode("Son_x0020_Duraklama_x0020_Alarmı")?.FirstChild.Value;

                        list.Add(model);
                    }
                }
            }
            catch (Exception) { }

            if (list.Any())
            {
                var groupByList = list.GroupBy(g => g.ArventoNo).ToList();
                var entity = groupByList
                    .Select(s => new VehicleOperatingReport()
                    {
                        MesafeKm = s.Sum(s => Convert.ToDecimal(s.MesafeKm?.Replace(".", ","))).ToString(),
                        MaxHiz = s.Max(s => Convert.ToDecimal(s.MaxHiz)).ToString(),
                        IlkKontakAcildi = s.FirstOrDefault()?.IlkKontakAcildi != null ? s.Min(s => Convert.ToDateTime(s.IlkKontakAcildi)).ToString("dd.MM.yyyy hh:mm") : null,
                        SonKontakKapandi = s.FirstOrDefault()?.SonKontakKapandi != null ? s.Max(s => Convert.ToDateTime(s.SonKontakKapandi)).ToString("dd.MM.yyyy hh:mm") : null,

                        Tarih = s.FirstOrDefault().Tarih,
                        ArventoNo = s.FirstOrDefault().ArventoNo,
                        ArventoPlaka = s.FirstOrDefault().ArventoPlaka.Replace(" ", "").ToUpper(),
                        DuraklamaSuresiSaat = s.Sum(s => Convert.ToInt32(s.DuraklamaSuresiSaat)),
                        DuraklamaSuresiDakika = s.Sum(s => Convert.ToInt32(s.DuraklamaSuresiDakika)),
                        DuraklamaSuresiSaniye = s.Sum(s => Convert.ToInt32(s.DuraklamaSuresiSaniye)),
                        RolantiSuresiSaat = s.Sum(s => Convert.ToInt32(s.RolantiSuresiSaat)),
                        RolantiSuresiDakika = s.Sum(s => Convert.ToInt32(s.RolantiSuresiDakika)),
                        RolantiSuresiSaniye = s.Sum(s => Convert.ToInt32(s.RolantiSuresiSaniye)),
                        HareketSuresiSaat = s.Sum(s => Convert.ToInt32(s.HareketSuresiSaat)),
                        HareketSuresiDakika = s.Sum(s => Convert.ToInt32(s.HareketSuresiDakika)),
                        HareketSuresiSaniye = s.Sum(s => Convert.ToInt32(s.HareketSuresiSaniye)),
                        KontakAcikKalmaSuresiSaat = s.Sum(s => Convert.ToInt32(s.KontakAcikKalmaSuresiSaat)),
                        KontakAcikKalmaSuresiDakika = s.Sum(s => Convert.ToInt32(s.KontakAcikKalmaSuresiDakika)),
                        KontakAcikKalmaSuresiSaniye = s.Sum(s => Convert.ToInt32(s.KontakAcikKalmaSuresiSaniye)),
                        AracSonDurumBilgileri = s.FirstOrDefault().AracSonDurumBilgileri,
                        HizAlarm = s.Sum(s => Convert.ToInt32(s.HizAlarm)).ToString(),
                        SehirIciHizAlarm = s.Sum(s => Convert.ToInt32(s.SehirIciHizAlarm)).ToString(),
                        SehirDisiHizAlarm = s.Sum(s => Convert.ToInt32(s.SehirDisiHizAlarm)).ToString(),
                        OtoyolHizAlarm = s.Sum(s => Convert.ToInt32(s.OtoyolHizAlarm)).ToString(),
                        RolantiAlarm = s.Sum(s => Convert.ToInt32(s.RolantiAlarm)).ToString(),
                        DuraklamaAlarm = s.Sum(s => Convert.ToInt32(s.DuraklamaAlarm)).ToString(),
                        HareketAlarm = s.Sum(s => Convert.ToInt32(s.HareketAlarm)).ToString(),
                        KontakAcildiAlarm = s.Sum(s => Convert.ToInt32(s.KontakAcildiAlarm)).ToString(),
                        KontakKapandiAlarm = s.Sum(s => Convert.ToInt32(s.KontakKapandiAlarm)).ToString(),
                        AniHizlanmaAlarm = s.Sum(s => Convert.ToInt32(s.AniHizlanmaAlarm)).ToString(),
                        AniYavaslamaAlarm = s.Sum(s => Convert.ToInt32(s.AniYavaslamaAlarm)).ToString(),
                        MotorDevirAsimiAlarm = s.Sum(s => Convert.ToInt32(s.MotorDevirAsimiAlarm)).ToString(),
                    }).FirstOrDefault();

                var second = 60;
                #region 
                if (entity.DuraklamaSuresiSaniye >= second)
                {
                    var saniye = entity.DuraklamaSuresiSaniye;
                    entity.DuraklamaSuresiSaniye = saniye % second;
                    entity.DuraklamaSuresiDakika += saniye / second;
                }

                if (entity.DuraklamaSuresiDakika >= 60)
                {
                    var dakika = entity.DuraklamaSuresiDakika;
                    entity.DuraklamaSuresiDakika = dakika % second;
                    entity.DuraklamaSuresiSaat += dakika / second;
                }
                #endregion

                #region 
                if (entity.RolantiSuresiSaniye >= second)
                {
                    var saniye = entity.RolantiSuresiSaniye;
                    entity.RolantiSuresiSaniye = saniye % second;
                    entity.RolantiSuresiDakika += saniye / second;
                }

                if (entity.RolantiSuresiDakika >= 60)
                {
                    var dakika = entity.RolantiSuresiDakika;
                    entity.RolantiSuresiDakika = dakika % second;
                    entity.RolantiSuresiSaat += dakika / second;
                }
                #endregion

                #region 
                if (entity.HareketSuresiSaniye >= second)
                {
                    var saniye = entity.HareketSuresiSaniye;
                    entity.HareketSuresiSaniye = saniye % second;
                    entity.HareketSuresiDakika += saniye / second;
                }

                if (entity.HareketSuresiDakika >= 60)
                {
                    var dakika = entity.HareketSuresiDakika;
                    entity.HareketSuresiDakika = dakika % second;
                    entity.HareketSuresiSaat += dakika / second;
                }
                #endregion

                #region 
                if (entity.KontakAcikKalmaSuresiSaniye >= second)
                {
                    var saniye = entity.KontakAcikKalmaSuresiSaniye;
                    entity.KontakAcikKalmaSuresiSaniye = saniye % second;
                    entity.KontakAcikKalmaSuresiDakika += saniye / second;
                }

                if (entity.KontakAcikKalmaSuresiDakika >= 60)
                {
                    var dakika = entity.KontakAcikKalmaSuresiDakika;
                    entity.KontakAcikKalmaSuresiDakika = dakika % second;
                    entity.KontakAcikKalmaSuresiSaat += dakika / second;
                }
                #endregion


                return entity;
            }

            return null;
        }
        #endregion

        #region Mesai Dışı
        public async Task ArventoMesaiDisiKullanimRaporu()
        {
            if (new ModeDetector().IsDebug)
                return;

            var start = DateTime.Now;
            if (start.Hour != 7 || start.DayOfWeek == DayOfWeek.Saturday)//Cumartesi çalışmayacak
                return;
            try
            {
                //if (IsJobRun())
                //    return;

                //JobSetTrueFalse("true");

                #region Mesai Dışı Kullanım
                var dateNow = DateTime.Now;
                var startDate = dateNow;
                var endDate = dateNow;
                var startDate2 = dateNow;
                var endDate2 = dateNow;
                var unique = "";

                var startDateEntity = startDate;
                var endDateEntity = endDate;
                if (dateNow.DayOfWeek == DayOfWeek.Monday) //Pazartesi günü gecesi dahil ediliyor. (00:00-07:00 arası)
                {
                    startDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 07, 0, 0);
                    dateNow = dateNow.AddDays(-1);
                    unique = dateNow.ToString("ddMMyyyy");
                    startDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 59, 59);

                    startDateEntity = startDate;
                    endDateEntity = endDate2;
                }
                else if (dateNow.DayOfWeek == DayOfWeek.Sunday) //Cuma aksamı dahil ediliyor. (19:00-23:59 arası)
                {
                    var friday = dateNow.AddDays(-2);
                    startDate = new DateTime(friday.Year, friday.Month, friday.Day, 19, 0, 0);
                    endDate = new DateTime(friday.Year, friday.Month, friday.Day, 23, 59, 59);
                    dateNow = dateNow.AddDays(-1);
                    unique = dateNow.ToString("ddMMyyyy");
                    startDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 59, 59);

                    startDateEntity = startDate;
                    endDateEntity = endDate2;
                }
                else
                {
                    startDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 07, 0, 0);
                    dateNow = dateNow.AddDays(-1);
                    startDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 19, 0, 0);
                    endDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 59, 59);
                    unique = dateNow.ToString("ddMMyyyy");

                    startDateEntity = startDate;
                    endDateEntity = endDate2;
                }


                var vehicleList = _vehicleRepository.Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo)).ToList();
                var datalist = new List<VehicleOperatingReport>();
                dateNow = DateTime.Now;
                foreach (var item in vehicleList)
                {
                    try
                    {
                        var data = new VehicleOperatingReport();

                        var templist = new List<VehicleOperatingReport>();

                        var firstRow = await VehicleOperatingReport(item.ArventoNo, startDate, endDate);
                        if (firstRow != null && firstRow.MesafeKm != null && firstRow.MesafeKm != "0")
                            templist.Add(firstRow);

                        start = DateTime.Now;
                        SearchSecondCheck(start);

                        var secondRow = await VehicleOperatingReport(item.ArventoNo, startDate2, endDate2);
                        if (secondRow != null && secondRow.MesafeKm != null && secondRow.MesafeKm != "0")
                            templist.Add(secondRow);

                        start = DateTime.Now;
                        if (templist.Any())
                        {
                            data = await GetGroupBySingle(templist);
                            if (data != null)
                            {
                                if (firstRow.MesafeKm != null && firstRow.MesafeKm != "0" && secondRow.MesafeKm != null && secondRow.MesafeKm != "0")
                                {
                                    data.IlkKontakAcildi = firstRow.IlkKontakAcildi;
                                    data.SonKontakKapandi = secondRow.SonKontakKapandi;
                                }
                                else if ((firstRow.MesafeKm == null || firstRow.MesafeKm == "0") && (secondRow.MesafeKm != null && secondRow.MesafeKm != "0"))
                                {
                                    data.IlkKontakAcildi = secondRow.IlkKontakAcildi;
                                    data.SonKontakKapandi = secondRow.SonKontakKapandi;
                                }
                                else if ((firstRow.MesafeKm != null && firstRow.MesafeKm != "0") && (secondRow.MesafeKm == null || secondRow.MesafeKm == "0"))
                                {
                                    data.IlkKontakAcildi = firstRow.IlkKontakAcildi;
                                    data.SonKontakKapandi = firstRow.SonKontakKapandi;
                                }
                            }
                        }

                        if (data != null && !string.IsNullOrEmpty(data.ArventoNo) && data.MesafeKm != null && data.MesafeKm != "0")
                        {
                            data.StartDate = startDateEntity;
                            data.EndDate = endDateEntity;
                            data.VehicleId = item.Id;
                            data.ZimmetliPlaka = item.Plate;

                            data.LastDebitUserId = item.LastUserId;
                            data.LastDebitCityId = item.LastCityId;
                            data.LastDebitStatus = item.LastStatus;
                            data.LastUnitId = item.LastUnitId;

                            data.LastDebitKm = item.LastKm;
                            data.Type = 0;
                            data.UniqueLine = unique;
                            data.TypeName = "Mesai Dışı Kullanım";

                            #region Arvento no kontrol
                            if (data.ArventoPlaka != data.ZimmetliPlaka)
                            {
                                item.ArventoNo = data.ArventoNo;
                                _vehicleRepository.Update(item);
                            }
                            #endregion

                            await _vehicleOperationReportRepository.InsertAsync(data);
                            await _uow.SaveChangesAsync();
                        }

                        SearchSecondCheck(start);
                    }
                    catch (Exception)
                    {
                        SearchSecondCheck(start);
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                //JobSetTrueFalse("false");
                MailSender("ArventoMesaiDisiKullanimRaporu Hk.", "Araç raporu çekilirken hata oluştu. Method name: ArventoMesaiDisiKullanimRaporu");
            }

            //JobSetTrueFalse("false");
        }

        public async Task<VehicleOperatingReport> GetGroupBySingle(List<VehicleOperatingReport> list)
        {
            try
            {
                if (list.Any())
                {
                    var groupByList = list.GroupBy(g => g.ArventoNo).ToList();
                    var entity = groupByList
                        .Select(s => new VehicleOperatingReport()
                        {
                            MesafeKm = s.Sum(s => Convert.ToDecimal(s?.MesafeKm?.Replace(".", ","))).ToString(),
                            MaxHiz = s.Max(s => Convert.ToDecimal(s?.MaxHiz)).ToString(),
                            //IlkKontakAcildi = s.FirstOrDefault()?.IlkKontakAcildi != null ? s.Min(s => Convert.ToDateTime(s.IlkKontakAcildi)).ToString("dd.MM.yyyy hh:mm") : null,
                            //SonKontakKapandi = s.FirstOrDefault()?.SonKontakKapandi != null ? s.Max(s => Convert.ToDateTime(s.SonKontakKapandi)).ToString("dd.MM.yyyy hh:mm") : null,

                            Tarih = s.FirstOrDefault()?.Tarih,
                            ArventoNo = s.FirstOrDefault()?.ArventoNo,
                            ArventoPlaka = s.FirstOrDefault()?.ArventoPlaka.Replace(" ", "").ToUpper(),
                            DuraklamaSuresiSaat = s.Sum(s => Convert.ToInt32(s?.DuraklamaSuresiSaat)),
                            DuraklamaSuresiDakika = s.Sum(s => Convert.ToInt32(s?.DuraklamaSuresiDakika)),
                            DuraklamaSuresiSaniye = s.Sum(s => Convert.ToInt32(s?.DuraklamaSuresiSaniye)),
                            RolantiSuresiSaat = s.Sum(s => Convert.ToInt32(s?.RolantiSuresiSaat)),
                            RolantiSuresiDakika = s.Sum(s => Convert.ToInt32(s?.RolantiSuresiDakika)),
                            RolantiSuresiSaniye = s.Sum(s => Convert.ToInt32(s?.RolantiSuresiSaniye)),
                            HareketSuresiSaat = s.Sum(s => Convert.ToInt32(s?.HareketSuresiSaat)),
                            HareketSuresiDakika = s.Sum(s => Convert.ToInt32(s?.HareketSuresiDakika)),
                            HareketSuresiSaniye = s.Sum(s => Convert.ToInt32(s?.HareketSuresiSaniye)),
                            KontakAcikKalmaSuresiSaat = s.Sum(s => Convert.ToInt32(s?.KontakAcikKalmaSuresiSaat)),
                            KontakAcikKalmaSuresiDakika = s.Sum(s => Convert.ToInt32(s?.KontakAcikKalmaSuresiDakika)),
                            KontakAcikKalmaSuresiSaniye = s.Sum(s => Convert.ToInt32(s?.KontakAcikKalmaSuresiSaniye)),
                            AracSonDurumBilgileri = s.FirstOrDefault()?.AracSonDurumBilgileri,
                            HizAlarm = s.Sum(s => Convert.ToInt32(s?.HizAlarm)).ToString(),
                            SehirIciHizAlarm = s.Sum(s => Convert.ToInt32(s?.SehirIciHizAlarm)).ToString(),
                            SehirDisiHizAlarm = s.Sum(s => Convert.ToInt32(s?.SehirDisiHizAlarm)).ToString(),
                            OtoyolHizAlarm = s.Sum(s => Convert.ToInt32(s?.OtoyolHizAlarm)).ToString(),
                            RolantiAlarm = s.Sum(s => Convert.ToInt32(s?.RolantiAlarm)).ToString(),
                            DuraklamaAlarm = s.Sum(s => Convert.ToInt32(s?.DuraklamaAlarm)).ToString(),
                            HareketAlarm = s.Sum(s => Convert.ToInt32(s?.HareketAlarm)).ToString(),
                            KontakAcildiAlarm = s.Sum(s => Convert.ToInt32(s?.KontakAcildiAlarm)).ToString(),
                            KontakKapandiAlarm = s.Sum(s => Convert.ToInt32(s?.KontakKapandiAlarm)).ToString(),
                            AniHizlanmaAlarm = s.Sum(s => Convert.ToInt32(s?.AniHizlanmaAlarm)).ToString(),
                            AniYavaslamaAlarm = s.Sum(s => Convert.ToInt32(s?.AniYavaslamaAlarm)).ToString(),
                            MotorDevirAsimiAlarm = s.Sum(s => Convert.ToInt32(s?.MotorDevirAsimiAlarm)).ToString(),
                        }).FirstOrDefault();

                    var second = 60;
                    #region 
                    if (entity.DuraklamaSuresiSaniye >= second)
                    {
                        var saniye = entity.DuraklamaSuresiSaniye;
                        entity.DuraklamaSuresiSaniye = saniye % second;
                        entity.DuraklamaSuresiDakika += saniye / second;
                    }

                    if (entity.DuraklamaSuresiDakika >= second)
                    {
                        var dakika = entity.DuraklamaSuresiDakika;
                        entity.DuraklamaSuresiDakika = dakika % second;
                        entity.DuraklamaSuresiSaat += dakika / second;
                    }
                    #endregion

                    #region 
                    if (entity.RolantiSuresiSaniye >= second)
                    {
                        var saniye = entity.RolantiSuresiSaniye;
                        entity.RolantiSuresiSaniye = saniye % second;
                        entity.RolantiSuresiDakika += saniye / second;
                    }

                    if (entity.RolantiSuresiDakika >= second)
                    {
                        var dakika = entity.RolantiSuresiDakika;
                        entity.RolantiSuresiDakika = dakika % second;
                        entity.RolantiSuresiSaat += dakika / second;
                    }
                    #endregion

                    #region 
                    if (entity.HareketSuresiSaniye >= second)
                    {
                        var saniye = entity.HareketSuresiSaniye;
                        entity.HareketSuresiSaniye = saniye % second;
                        entity.HareketSuresiDakika += saniye / second;
                    }

                    if (entity.HareketSuresiDakika >= second)
                    {
                        var dakika = entity.HareketSuresiDakika;
                        entity.HareketSuresiDakika = dakika % second;
                        entity.HareketSuresiSaat += dakika / second;
                    }
                    #endregion

                    #region 
                    if (entity.KontakAcikKalmaSuresiSaniye >= second)
                    {
                        var saniye = entity.KontakAcikKalmaSuresiSaniye;
                        entity.KontakAcikKalmaSuresiSaniye = saniye % second;
                        entity.KontakAcikKalmaSuresiDakika += saniye / second;
                    }

                    if (entity.KontakAcikKalmaSuresiDakika >= second)
                    {
                        var dakika = entity.KontakAcikKalmaSuresiDakika;
                        entity.KontakAcikKalmaSuresiDakika = dakika % second;
                        entity.KontakAcikKalmaSuresiSaat += dakika / second;
                    }
                    #endregion


                    return entity;
                }

                return null;
            }
            catch (Exception)
            { }
            return null;
        }
        #endregion

        #region Mesai İçi
        public async Task ArventoMesaiIciKullanimRaporu()
        {
            if (new ModeDetector().IsDebug)
                return;

            var dateNow = DateTime.Now;
            try
            {
                if (dateNow.Hour != 19)
                    return;

                //if (IsJobRun())
                //    return;

                if (dateNow.DayOfWeek == DayOfWeek.Saturday || dateNow.DayOfWeek == DayOfWeek.Sunday)//Haftasonu mesai dışına giriyor
                    return;

                //JobSetTrueFalse("true");
                #region Mesai İçi Kullanım

                var startDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 7, 0, 0); //07:00
                var endDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 19, 0, 0); //19:00

                var vehicleList = _vehicleRepository.Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo)).ToList();
                var datalist = new List<VehicleOperatingReport>();
                foreach (var item in vehicleList)
                {
                    try
                    {
                        var data = await VehicleOperatingReport(item.ArventoNo, startDate, endDate);
                        var start = DateTime.Now;
                        if (!string.IsNullOrEmpty(data.ArventoNo) && data.MesafeKm != null && data.MesafeKm != "0")
                        {
                            data.StartDate = startDate;
                            data.EndDate = endDate;
                            data.VehicleId = item.Id;
                            data.ZimmetliPlaka = item.Plate;

                            data.LastDebitUserId = item.LastUserId;
                            data.LastDebitCityId = item.LastCityId;
                            data.LastDebitStatus = item.LastStatus;
                            data.LastUnitId = item.LastUnitId;
                            data.LastDebitKm = item.LastKm;

                            data.Type = 1;
                            data.UniqueLine = dateNow.ToString("ddMMyyyy");
                            data.TypeName = "Mesai İçi Kullanım";

                            #region Arvento no kontrol
                            if (data.ArventoPlaka != data.ZimmetliPlaka)
                            {
                                item.ArventoNo = data.ArventoNo;
                                _vehicleRepository.Update(item);
                            }
                            #endregion

                            await _vehicleOperationReportRepository.InsertAsync(data);
                            await _uow.SaveChangesAsync();
                        }

                        var end = DateTime.Now;
                        int diffSeconds = (int)(end - start).TotalSeconds;//arvento 30 sn'de bir istek kabul ediyor
                        if (diffSeconds < 30)
                            Thread.Sleep((30 - diffSeconds) * 1000);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(30 * 1000);
                    }
                }

                //JobSetTrueFalse("false");

                #endregion
            }
            catch (Exception)
            {
                //JobSetTrueFalse("false");
                MailSender("ArventoMesaiIciKullanimRaporu Hk.", "Araç raporu çekilirken hata oluştu. Method name: ArventoMesaiIciKullanimRaporu");
            }
        }
        #endregion

        #region Arvento job çalışıyor mu ?
        public bool IsJobRun()
        {
            try
            {
                var param = _parameterRepository.FirstOrDefaultNoTracking(f => f.KeyP == "IsArventoJobRun");

                if (param == null)
                    return false;
                else if (Convert.ToBoolean(param.ValueP))
                    return true;
                else if (!Convert.ToBoolean(param.ValueP))
                    return false;
            }
            catch (Exception) { }

            return false;
        }

        public void JobSetTrueFalse(string val)
        {
            try
            {
                var param = _parameterRepository.FirstOrDefault(f => f.KeyP == "IsArventoJobRun");
                if (param != null)
                {
                    param.ValueP = val;
                    _parameterRepository.Update(param);
                    _uow.SaveChanges();
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Mesai İçi/Dışı Kullanım Raporu Mail Gönder
        public async Task AracKullanimRaporuMailGonder()
        {
            if (new ModeDetector().IsDebug)
                return;

            var dateLastDay = DateTime.Now.AddDays(-1);

            try
            {
                if (dateLastDay.Hour != 12)
                    return;

                var list = await _vehicleOperationReportRepository.WhereAsync(w => w.Status && !w.IsSendMail && w.UniqueLine == dateLastDay.ToString("ddMMyyyy"));
                if (!list.Any())
                    return;

                var dateNow = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                var mailTemplate = await _parameterRepository.FirstOrDefaultAsync(f => f.KeyP == ParameterEnum.GorevDisiKullanimMailTemplate.ToString());

                if (mailTemplate == null)
                    return;

                var template_ = mailTemplate.ValueP;

                #region Set Start/EndDate
                var startDate_ic = list.FirstOrDefault(f => f.Type == 1);
                var startDate_dis = list.FirstOrDefault(f => f.Type == 0);

                template_ = template_.Replace("{dateNow}", dateNow);
                if (startDate_ic != null)
                    template_ = template_.Replace("{Mesai_Ici_Tarihi}", startDate_ic.StartDate?.ToString("dd MMMM HH:mm") + "<br>" + startDate_ic.EndDate?.ToString("dd MMMM HH:mm"));
                else
                    template_ = template_.Replace("{Mesai_Ici_Tarihi}", "-");


                if (startDate_dis != null)
                    template_ = template_.Replace("{Mesai_Disi_Tarihi}", startDate_dis.StartDate?.ToString("dd MMMM HH:mm") + "<br>" + startDate_dis.EndDate?.ToString("dd MMMM HH:mm"));
                else
                    template_ = template_.Replace("{Mesai_Disi_Tarihi}", "-");

                #endregion

                var gropedPlate = list.GroupBy(g => g.VehicleId).ToList();//Tüm birimler için burası açılacak

                #region Şimdilik milked araçlarına bildirim gidecek. Region komple silinecek
                //var milkedAraclari = await GetVehicleListByUnitId(11);//todo: silinecek
                //var vehicleIds = milkedAraclari.Select(s => s.Id).ToList();//todo: silinecek
                //var gropedPlate = list.Where(w => vehicleIds.Contains(w.VehicleId)).GroupBy(g => g.VehicleId).ToList();//todo: silinecek
                #endregion


                var disabledProjectMailList = GetDisabledProjectMailList(); //Mail gönderilmeyecek birimler
                var allUnit = GetAllUnit(); //Tüm araçlar çekiliyor
                foreach (var item in gropedPlate)
                {
                    var unitVehicle = allUnit.FirstOrDefault(f => f.Id == item.FirstOrDefault().LastUnitId);
                    if (unitVehicle == null)
                        continue;

                    var notSendMailMudurluk = disabledProjectMailList.FirstOrDefault(f => f.Id == unitVehicle.ParentId);
                    if (notSendMailMudurluk != null)//Müdürlüğü kontrol ediliyor
                        continue;

                    var notSendMailProject = disabledProjectMailList.FirstOrDefault(f => f.Id == unitVehicle.Id);
                    if (notSendMailProject != null)//proje kontrol ediliyor
                        continue;

                    try
                    {
                        #region Parametreler
                        var vehicleId = item.Key;
                        var KontakAcmaKapamaSaati_ic = "✘";
                        var ArventoKm_ic = "✘";
                        var GorevAcildiMi_ic = "✘";
                        var GorevBasBitIl_ic = "✘";
                        var GorevSuresi_ic = "✘";
                        var GorevdeYaptigiKm_ic = "✘";

                        var KontakAcmaKapamaSaati_dis = "✘";
                        var ArventoKm_dis = "✘";
                        var GorevAcildiMi_dis = "✘";
                        var GorevBasBitIl_dis = "✘";
                        var GorevSuresi_dis = "✘";
                        var GorevdeYaptigiKm_dis = "✘";

                        var mesaiIciMaxHiz = "✘";
                        var mesaiDisiMaxHiz = "✘";
                        #endregion

                        #region Mesai İçi Parametresi
                        var mesaiIci = item.FirstOrDefault(f => f.Type == 1);
                        if (mesaiIci != null)
                        {
                            if (mesaiIci != null && (!string.IsNullOrEmpty(mesaiIci.IlkKontakAcildi) || !string.IsNullOrEmpty(mesaiIci.SonKontakKapandi)))
                                KontakAcmaKapamaSaati_ic = (mesaiIci.IlkKontakAcildi ?? "✘") + "<br>" + (mesaiIci.SonKontakKapandi ?? "✘");

                            if (!string.IsNullOrEmpty(mesaiIci.MesafeKm))
                                ArventoKm_ic = mesaiIci.MesafeKm + " Km";

                            var mesaiIciGorev = _tripRepository.Where(f => f.Status && f.VehicleId == vehicleId && mesaiIci.StartDate <= f.StartDate && f.StartDate <= mesaiIci.EndDate).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                            if (mesaiIciGorev != null)
                            {
                                TimeSpan openClosedDate = mesaiIciGorev.State == (int)TripState.EndTrip ? (mesaiIciGorev.EndDate.Value - mesaiIciGorev.StartDate) : (DateTime.Now - mesaiIciGorev.StartDate);
                                var days = (openClosedDate.Days + " Gün, " + openClosedDate.Hours + " Saat, " + openClosedDate.Minutes + " Dk");
                                GorevSuresi_ic = mesaiIciGorev.State == (int)TripState.EndTrip ? (days + " (Görev Kapatıldı)") : (days + " (Görev Açık)");

                                GorevBasBitIl_ic = GetCityName(mesaiIciGorev.StartCityId) + "<br>" + (mesaiIciGorev.State == (int)TripState.EndTrip ? GetCityName(mesaiIciGorev.EndCityId.Value) : "Devam Ediyor");
                                GorevAcildiMi_ic = "<div style='color:green;'>✔ Evet</div>";
                                GorevdeYaptigiKm_ic = mesaiIciGorev.Type == (int)TripType.Mission ? "(Görev)-" : "<div style='color:red;'>(Görev Dışı)</div>-";
                                GorevdeYaptigiKm_ic += mesaiIciGorev.State == (int)TripState.EndTrip ? (Convert.ToInt32(mesaiIciGorev.EndKm.Value - mesaiIciGorev.StartKm) + " Km") : "Devam Ediyor";
                                mesaiIciMaxHiz = mesaiIci.MaxHiz + " Km/h";
                            }
                            else
                                GorevAcildiMi_ic = "<div style='color:red;'>✘</div>";
                        }
                        #endregion

                        #region Mesai Dışı Parametresi
                        var mesaiDisi = item.FirstOrDefault(f => f.Type == 0);
                        if (mesaiDisi != null)
                        {
                            if (mesaiDisi != null && (!string.IsNullOrEmpty(mesaiDisi.IlkKontakAcildi) || !string.IsNullOrEmpty(mesaiDisi.SonKontakKapandi)))
                                KontakAcmaKapamaSaati_dis = (mesaiDisi.IlkKontakAcildi ?? "✘") + "<br>" + (mesaiDisi.SonKontakKapandi ?? "✘");

                            if (!string.IsNullOrEmpty(mesaiDisi.MesafeKm))
                                ArventoKm_dis = mesaiDisi.MesafeKm + " Km";

                            var mesaiDisiGorev = _tripRepository.Where(f => f.Status && f.VehicleId == vehicleId && mesaiDisi.StartDate <= f.StartDate && f.StartDate <= mesaiDisi.EndDate).OrderByDescending(o => o.Id).Take(1).FirstOrDefault();
                            if (mesaiDisiGorev != null)
                            {
                                TimeSpan openClosedDate = mesaiDisiGorev.State == (int)TripState.EndTrip ? (mesaiDisiGorev.EndDate.Value - mesaiDisiGorev.StartDate) : (DateTime.Now - mesaiDisiGorev.StartDate);
                                var days = (openClosedDate.Days + " Gün, " + openClosedDate.Hours + " Saat, " + openClosedDate.Minutes + " Dk");
                                GorevSuresi_dis = mesaiDisiGorev.State == (int)TripState.EndTrip ? (days + " (Görev Kapatıldı)") : (days + " (Görev Açık)");

                                GorevBasBitIl_dis = GetCityName(mesaiDisiGorev.StartCityId) + "<br>" + (mesaiDisiGorev.State == (int)TripState.EndTrip ? GetCityName(mesaiDisiGorev.EndCityId.Value) : "Devam Ediyor");
                                GorevAcildiMi_dis = "<div style='color:green;'>✔ Evet</div>";
                                GorevdeYaptigiKm_dis = mesaiDisiGorev.Type == (int)TripType.Mission ? "(Görev)-" : "(Görev Dışı)-";
                                GorevdeYaptigiKm_dis += mesaiDisiGorev.State == (int)TripState.EndTrip ? (Convert.ToInt32(mesaiDisiGorev.EndKm.Value - mesaiDisiGorev.StartKm) + " Km") : "Devam Ediyor";
                                mesaiDisiMaxHiz = mesaiDisi.MaxHiz + " Km/h";
                            }
                            else
                                GorevAcildiMi_dis = "<div style='color:red;'>✘</div>";
                        }
                        #endregion

                        //--Mesai içi/dışı aynı kullanıcıysa
                        if (mesaiIci != null && mesaiDisi != null && mesaiIci.LastDebitUserId == mesaiDisi.LastDebitUserId)
                        {
                            #region Replace
                            var template = template_;
                            //template = template.Replace("{Mesai_Ici_Tarihi}", mesaiIci.StartDate?.ToString("dd MMMM HH:mm", culture) + "<br>" + mesaiIci.EndDate?.ToString("dd MMMM HH:mm", culture));
                            //template = template.Replace("{Mesai_Disi_Tarihi}", mesaiDisi.StartDate?.ToString("dd MMMM HH:mm", culture) + "<br>" + mesaiDisi.EndDate?.ToString("dd MMMM HH:mm", culture));

                            template = template.Replace("{KontakAcmaKapamaSaati_ic}", KontakAcmaKapamaSaati_ic);
                            template = template.Replace("{ArventoKm_ic}", ArventoKm_ic);
                            template = template.Replace("{GorevAcildiMi_ic}", GorevAcildiMi_ic);
                            template = template.Replace("{GorevBasBitIl_ic}", GorevBasBitIl_ic);
                            template = template.Replace("{GorevSuresi_ic}", GorevSuresi_ic);
                            template = template.Replace("{GorevdeYaptigiKm_ic}", GorevdeYaptigiKm_ic);
                            template = template.Replace("{Max_hiz_ic}", mesaiIciMaxHiz);

                            template = template.Replace("{KontakAcmaKapamaSaati_dis}", KontakAcmaKapamaSaati_dis);
                            template = template.Replace("{ArventoKm_dis}", ArventoKm_dis);
                            template = template.Replace("{GorevAcildiMi_dis}", GorevAcildiMi_dis);
                            template = template.Replace("{GorevBasBitIl_dis}", GorevBasBitIl_dis);
                            template = template.Replace("{GorevSuresi_dis}", GorevSuresi_dis);
                            template = template.Replace("{GorevdeYaptigiKm_dis}", GorevdeYaptigiKm_dis);
                            template = template.Replace("{Max_hiz_dis}", mesaiDisiMaxHiz);
                            #endregion

                            try
                            {
                                if (mesaiIci.LastDebitUserId > 0)
                                {
                                    var result = await SendMailFromTrip(mesaiIci.LastDebitUserId.Value, template, item.FirstOrDefault()?.ArventoPlaka);
                                    if (result.IsSuccess)
                                    {
                                        var entites = item.ToList();
                                        entites.ForEach(f => f.IsSendMail = true);
                                        _vehicleOperationReportRepository.UpdateRange(entites);
                                        await _uow.SaveChangesAsync();
                                    }
                                }
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            if (mesaiIci != null && mesaiIci.LastDebitUserId > 0)
                            {
                                #region Mesai içi
                                var template_ic = template_;
                                //template_ic = template_ic.Replace("{Mesai_Ici_Tarihi}", mesaiIci.StartDate?.ToString("dd MMMM HH:mm", culture) + "<br>" + mesaiIci.EndDate?.ToString("dd MMMM HH:mm", culture));
                                template_ic = template_ic.Replace("{KontakAcmaKapamaSaati_ic}", KontakAcmaKapamaSaati_ic);
                                template_ic = template_ic.Replace("{ArventoKm_ic}", ArventoKm_ic);
                                template_ic = template_ic.Replace("{GorevAcildiMi_ic}", GorevAcildiMi_ic);
                                template_ic = template_ic.Replace("{GorevBasBitIl_ic}", GorevBasBitIl_ic);
                                template_ic = template_ic.Replace("{GorevSuresi_ic}", GorevSuresi_ic);
                                template_ic = template_ic.Replace("{GorevdeYaptigiKm_ic}", GorevdeYaptigiKm_ic);
                                template_ic = template_ic.Replace("{Max_hiz_ic}", mesaiIciMaxHiz);

                                template_ic = template_ic.Replace("{KontakAcmaKapamaSaati_dis}", "-");
                                template_ic = template_ic.Replace("{ArventoKm_dis}", "-");
                                template_ic = template_ic.Replace("{GorevAcildiMi_dis}", "-");
                                template_ic = template_ic.Replace("{GorevBasBitIl_dis}", "-");
                                template_ic = template_ic.Replace("{GorevSuresi_dis}", "-");
                                template_ic = template_ic.Replace("{GorevdeYaptigiKm_dis}", "-");
                                template_ic = template_ic.Replace("{Max_hiz_dis}", "-");

                                try
                                {
                                    if (mesaiIci.LastDebitUserId > 0)
                                    {
                                        var result = await SendMailFromTrip(mesaiIci.LastDebitUserId.Value, template_ic, item.FirstOrDefault()?.ArventoPlaka);

                                        if (result.IsSuccess)
                                        {
                                            var entites = item.Where(w => w.Type == 1).ToList();
                                            entites.ForEach(f => f.IsSendMail = true);
                                            _vehicleOperationReportRepository.UpdateRange(entites);
                                            await _uow.SaveChangesAsync();
                                        }
                                    }
                                }
                                catch (Exception) { }
                                #endregion
                            }

                            if (mesaiDisi != null && mesaiDisi.LastDebitUserId > 0)
                            {
                                #region Mesai dışı
                                var template_dis = template_;
                                //template_dis = template_dis.Replace("{Mesai_Disi_Tarihi}", mesaiDisi.StartDate?.ToString("dd MMMM HH:mm", culture) + "<br>" + mesaiDisi.EndDate?.ToString("dd MMMM HH:mm", culture));

                                template_dis = template_dis.Replace("{KontakAcmaKapamaSaati_ic}", "-");
                                template_dis = template_dis.Replace("{ArventoKm_ic}", "-");
                                template_dis = template_dis.Replace("{GorevAcildiMi_ic}", "-");
                                template_dis = template_dis.Replace("{GorevBasBitIl_ic}", "-");
                                template_dis = template_dis.Replace("{GorevSuresi_ic}", "-");
                                template_dis = template_dis.Replace("{GorevdeYaptigiKm_ic}", "-");
                                template_dis = template_dis.Replace("{Max_hiz_ic}", "-");

                                template_dis = template_dis.Replace("{KontakAcmaKapamaSaati_dis}", KontakAcmaKapamaSaati_dis);
                                template_dis = template_dis.Replace("{ArventoKm_dis}", ArventoKm_dis);
                                template_dis = template_dis.Replace("{GorevAcildiMi_dis}", GorevAcildiMi_dis);
                                template_dis = template_dis.Replace("{GorevBasBitIl_dis}", GorevBasBitIl_dis);
                                template_dis = template_dis.Replace("{GorevSuresi_dis}", GorevSuresi_dis);
                                template_dis = template_dis.Replace("{GorevdeYaptigiKm_dis}", GorevdeYaptigiKm_dis);
                                template_dis = template_dis.Replace("{Max_hiz_dis}", mesaiDisiMaxHiz);

                                try
                                {
                                    if (mesaiDisi.LastDebitUserId > 0)
                                    {
                                        var result = await SendMailFromTrip(mesaiDisi.LastDebitUserId.Value, template_dis, item.FirstOrDefault()?.ArventoPlaka);
                                        if (result.IsSuccess)
                                        {
                                            var entites = item.Where(w => w.Type == 0).ToList();
                                            entites.ForEach(f => f.IsSendMail = true);
                                            _vehicleOperationReportRepository.UpdateRange(entites);
                                            await _uow.SaveChangesAsync();
                                        }
                                    }
                                }
                                catch (Exception) { }
                                #endregion
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }
        public List<Unit> GetDisabledProjectMailList()
        {
            var list = (from vo in _vehicleOpParam.GetAll()
                        join un in _unitRepository.GetAll() on vo.UnitId equals un.Id into unitL
                        from un in unitL.DefaultIfEmpty()
                        join un2 in _unitRepository.GetAll() on un.ParentId equals un2.Id into unit2L
                        from un2 in unit2L.DefaultIfEmpty()
                        where vo.Status
                        select new Unit
                        {
                            Id = un.Id,
                            ParentId = un2.Id,
                            Name = un2.Name + "/" + un.Name,
                        }).ToList();

            return list;
        }
        public List<EUnitDto> GetAllUnit()
        {
            var list = (from u in _unitRepository.GetAll()
                        join u2 in _unitRepository.GetAll() on u.ParentId equals u2.Id into unit2L
                        from u2 in unit2L.DefaultIfEmpty()
                        select new EUnitDto
                        {
                            Id = u.Id,
                            ParentId = u.ParentId,
                            Name = u.Name + "/" + u2.Name
                        }).ToList();

            return list;
        }
        public async Task<EResultDto> SendMailFromTrip(int userId, string template, string plate)
        {
            var result = new EResultDto();
            result.IsSuccess = false;
            try
            {
                string mailCc = "";
                var driveUser = await _userRepository.FindAsync(userId);

                if (driveUser != null && (!driveUser.IsSendMail || !driveUser.IsSendMailVehicleOpReport.GetValueOrDefault(false)))
                    return result;

                if (driveUser.Flag != (int)Flag.Manager && driveUser.UnitId > 0)
                {
                    var manager = _userRepository.Where(f => f.Status && f.UnitId == driveUser.UnitId && f.IsSendMail && f.IsSendMailVehicleOpReport == true && f.Flag == (int)Flag.Manager).ToList();

                    if (manager.Any())
                        mailCc = String.Join(";", manager.Select(s => s.Email).Distinct().ToList());
                }

                if (string.IsNullOrEmpty(mailCc))
                    mailCc = "mehmetpehlivan@basaranteknoloji.net";//todo:silinecek
                else
                    mailCc = "mehmetpehlivan@basaranteknoloji.net;" + mailCc;

                var date = DateTime.Now.AddDays(-1);
                return await _mobileService.SendMailAsync(new EMessageLogDto()
                {
                    Subject = plate + " Plakalı Aracın " + date.ToString("dd MMMM yyyy") + " Tarihli Raporu Hk.",
                    Body = template,
                    Type = (int)MessageLogType.EMail,
                    Email = driveUser.Email,
                    MailCc = mailCc
                });
            }
            catch (Exception)
            {
            }
            return result;
        }
        #endregion

        #region Aracın son koordinat bilgilerini günceller
        public async Task AracSonKoordinatGuncelle()
        {
            if (new ModeDetector().IsDebug)
                return;

            try
            {
                var url = rootUrl + $"GetVehicleStatusJSON?Username={userName}&PIN1={password}&PIN2={password}&callBack=c";
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string datastring = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(datastring))
                            datastring = datastring.Substring(2, datastring.Length - 4);

                        var list = JsonConvert.DeserializeObject<List<ECoordinateDto>>(datastring);

                        var vehicleList = await _vehicleRepository.WhereAsync(w => w.Status && w.ArventoNo != null);
                        foreach (var item in vehicleList)
                        {
                            var latLong = list.FirstOrDefault(f => f.Node == item.ArventoNo);
                            if (latLong != null)
                            {
                                item.Latitude = latLong.LatitudeY;
                                item.Longitude = latLong.LongitudeX;
                                item.LastAddress = latLong.Address;
                                item.LastSpeed = latLong.Speed;
                                item.LastCoordinateInfo = DateTime.ParseExact(latLong.LocalDateTime, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
                            }
                        }

                        var dateNow = DateTime.Now;
                        vehicleList = vehicleList.Where(w => w.Latitude > 0 && w.LastCoordinateInfo != null).ToList();
                        vehicleList = vehicleList.Where(w => w.LastCoordinateInfo.Value.Date == dateNow.Date).ToList();
                        _vehicleRepository.UpdateRange(vehicleList);
                        await _uow.SaveChangesAsync();
                    }
                }
            }
            catch (Exception) { }
        }
        public async Task<List<ECoordinateDto>> GetAracSonKoordinatList()
        {
            var vehicleList = new List<ECoordinateDto>();
            var url = rootUrl + $"GetVehicleStatusJSON?Username={userName}&PIN1={password}&PIN2={password}&callBack=c";
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    string datastring = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(datastring) && datastring.Length > 4)
                        datastring = datastring.Substring(2, datastring.Length - 4);

                    var list = JsonConvert.DeserializeObject<List<ECoordinateDto>>(datastring) ?? new List<ECoordinateDto>();
                    foreach (var item in list)
                    {
                        if (!string.IsNullOrEmpty(item.LocalDateTime) && item.LocalDateTime.Length >= 14)
                        {
                            if (DateTime.TryParseExact(item.LocalDateTime.Substring(0, 14), "yyyyMMddHHmmss",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            {
                                item.LocalDateTime2 = parsedDate;
                                item.LocalDateTime = parsedDate.ToString("dd.MM.yyyy HH:mm", new CultureInfo("tr-TR"));
                            }
                        }
                    }

                    var dateNow = DateTime.Now.Date.AddDays(-3);
                    var last2Day = list.Where(w => w.LocalDateTime2 > dateNow && w.LatitudeY > 0).Select(s => s.Node).Distinct().ToList();

                    var vehicles = await _reportService.GetActiveListWithMemoryCache();
                    var activeUser = await _reportService.GetCachedMemoryActiveUser();
                    if (vehicles != null)
                    {
                        vehicleList = vehicles.Where(w => last2Day.Contains(w.ArventoNo))
                            .Select(s => new ECoordinateDto
                            {
                                UnitId = s.UnitId ?? 0,
                                ParentUnitId = s.UnitParentId ?? 0,
                                VehicleId = s.VehicleId,
                                DebitUserId = s.DebitUserId,
                                DebitNameSurname = s.DebitNameSurname2,
                                licensePlate = s.Plate2,
                                ArventoNo = s.ArventoNo,
                                MaxSpeed = s.MaxSpeed,
                            }).ToList();

                        if (!activeUser.IsAdmin)
                        {
                            if (activeUser.ParentUnitId > 0)
                                vehicleList = vehicleList.Where(w => w.ParentUnitId == activeUser.ParentUnitId).ToList();

                            if (activeUser.UnitId > 0)
                                vehicleList = vehicleList.Where(w => w.UnitId == activeUser.UnitId).ToList();
                        }

                        foreach (var item in vehicleList)
                        {
                            var arventoVehicle = list.FirstOrDefault(f => f.Node == item.ArventoNo);
                            item.LatitudeY = arventoVehicle.LatitudeY;
                            item.LongitudeX = arventoVehicle.LongitudeX;
                            item.Address = arventoVehicle.Address;
                            item.Speed = arventoVehicle.Speed;
                            item.LocalDateTime = arventoVehicle.LocalDateTime;
                        }
                    }
                }
            }


            return vehicleList;
        }
        #endregion

        #region Hız Bildirimi
        public async Task ArventoPlakaHiziSorgula()
        {
            if (new ModeDetector().IsDebug)
                return;

            var date = DateTime.Now.AddSeconds(-3);
            try
            {
                var url = rootUrl + $"GetVehicleAlarmStatusJson?Username={userName}&PIN1={password}&PIN2={password}&Language=0";
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var datastring = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(datastring))
                            datastring = datastring.Substring(1, datastring.Length - 3);
                        var list = JsonConvert.DeserializeObject<List<ESpeedDto>>(datastring);

                        var speedList = list.Where(w => w.AlarmType == "Harita Hız Alarmı" && w.GmtDateTime >= date).OrderByDescending(o => o.GmtDateTime).ToList();
                        if (speedList.Any())
                            await Task.Run(() => InsertNotice(speedList));

                        var coordinateList = list.Where(w => w.AlarmType == "Konum Bilgisi" && w.GmtDateTime >= date).OrderByDescending(o => o.GmtDateTime).ToList();
                        if (coordinateList.Any())
                            await Task.Run(() => InsertPlateCoordinate(coordinateList));
                    }
                }
            }
            catch (Exception) { }
        }

        public async Task InsertNotice(List<ESpeedDto> speedList)
        {
            try
            {
                var vehicleList = _vehicleRepository.Where(w => w.Status && w.ArventoNo != null).ToList();
                var arventoNoList = speedList.GroupBy(g => g.DeviceNo).Select(s => s.Key).ToList();
                var cityList = _cityRepository.Where(w => w.ParentId == null).ToList();

                foreach (var item in arventoNoList)
                {
                    var vehicle = vehicleList.FirstOrDefault(f => f.ArventoNo == item);
                    if (vehicle != null)
                    {
                        var maxSpeed = vehicle.MaxSpeed.GetValueOrDefault(120);
                        maxSpeed += maxSpeed * 20 / 100;

                        var topSpeed = speedList.Where(f => f.DeviceNo == vehicle.ArventoNo && f.Speed >= maxSpeed)
                            .OrderByDescending(o => o.Speed).Take(1).FirstOrDefault();

                        if (topSpeed != null)
                        {
                            var speed = (decimal)(topSpeed.Speed);
                            var address = topSpeed.Address;
                            var addArr = address.Split(',').ToArray();
                            var speedDate = topSpeed.GmtDateTime;

                            var cityId = cityList.FirstOrDefault(w => w.Name.ToLower().Contains(addArr[addArr.Length - 2].Replace(" ", "").ToLower()))?.Id;

                            await _noticeRepository.InsertAsync(new Notice()
                            {
                                CreatedBy = 1,
                                VehicleId = vehicle.Id,
                                CityId = cityId,
                                NoticeType = (int)NoticeType.Speed,
                                TransactionDate = topSpeed.GmtDateTime,
                                Speed = speed,
                                Address = address,
                                ImportType = (int)ImportType.Arvento,
                                State = (int)NoticeState.Draft,

                                LastDebitUserId = vehicle.LastUserId,
                                LastUnitId = vehicle.LastUnitId,
                                LastDebitStatus = vehicle.LastStatus,
                                LastDebitKm = vehicle.LastKm
                            });
                            await _uow.SaveChangesAsync();

                            if (vehicle.LastUserId > 0)
                            {
                                #region Push Notification
                                await Task.Run(() =>
                                 _mobileService.PushNotificationAsync(new EMessageLogDto()
                                 {
                                     Subject = "Hız Limiti Aşımı",
                                     Body = vehicle.Plate + " plakalı araç için hız " + speed + ". Adres: " + address,
                                     Type = (int)MessageLogType.PushNotification,
                                     UserId = vehicle.LastUserId.Value,
                                 }));

                                //todo:silinecek
                                await Task.Run(() =>
                                _mobileService.PushNotificationAsync(new EMessageLogDto()
                                {
                                    Subject = "Hız Limiti Aşımı",
                                    Body = vehicle.Plate + " plakalı araç için hız " + speed + ". Adres: " + address,
                                    Type = (int)MessageLogType.PushNotification,
                                    UserId = 439,
                                }));
                                #endregion

                                #region Email
                                var driveUser = await _userRepository.FindAsync(vehicle.LastUserId.Value);
                                if (driveUser != null && !string.IsNullOrEmpty(driveUser.Email))
                                {
                                    string mailCc = "";

                                    #region Müdür ekleniyor
                                    try
                                    {
                                        if (driveUser.Flag != (int)Flag.Manager && driveUser.UnitId > 0)
                                        {
                                            var driveUnit = await _unitRepository.FindAsync(driveUser.UnitId.Value);//kullanıcı birim

                                            var parentId = driveUnit.ParentId > 0 ? driveUnit.ParentId : driveUnit.Id;//kullanıcı müdürlük bazın yetki verildiyse id değeri alınır

                                            var manager = (from u in _userRepository.GetAll()
                                                           join ur in _userRoleRepository.GetAll() on u.Id equals ur.UserId
                                                           where u.Status &&
                                                           u.IsSendMail &&
                                                           u.UnitId == parentId &&
                                                           u.Flag == (int)Flag.Manager &&
                                                           !string.IsNullOrEmpty(u.Email)
                                                           select new User() { Email = u.Email }).ToList();

                                            if (manager.Any())
                                                mailCc = String.Join(";", manager.Select(s => s.Email).Distinct().ToList());
                                        }
                                    }
                                    catch (Exception) { }
                                    #endregion

                                    #region Görev&Zimmetli farklı kişiyle o damaile ekleniyor
                                    try
                                    {
                                        var dateNow = DateTime.Now;
                                        var trip = _tripRepository.Where(f => f.Status && f.VehicleId == vehicle.Id && f.StartDate > dateNow && dateNow <= (f.EndDate ?? dateNow)).FirstOrDefault();
                                        if (trip.DriverId != vehicle.LastUserId)
                                        {
                                            var tripUser = _userRepository.Find(trip.DriverId);
                                            if (!string.IsNullOrEmpty(tripUser.Email))
                                                mailCc += mailCc == "" ? tripUser.Email : (";" + tripUser.Email);
                                        }
                                    }
                                    catch (Exception) { }
                                    #endregion

                                    var bodyMail = "Merhabalar,<br/>";
                                    bodyMail += vehicle.Plate + " plakalı araç için aşırı hız algılandı. <br/>Mevcut hızınız <b>" + speed + " km/h</b> olarak görünüyor.<br/>";
                                    bodyMail += "Güvenliğiniz için hız limitine uyunuz.<br/><br/>";
                                    bodyMail += "Hız İhlali : <br/>";
                                    bodyMail += "Adres : <b>" + address + "</b><br/>";
                                    bodyMail += "Tarih : <b>" + speedDate + "</b><br/><br/>";
                                    bodyMail += "<a href='https://basaranerp.com/'>basaranerp.com</a> tarafından otomatik gönderilmiştir. Lütfen yanıtlamayınız.";

                                    if (string.IsNullOrEmpty(mailCc))
                                        mailCc = "mehmetpehlivan@basaranteknoloji.net";
                                    else
                                        mailCc = "mehmetpehlivan@basaranteknoloji.net;" + mailCc;

                                    await Task.Run(() =>
                                    _mobileService.SendMailAsync(new EMessageLogDto()
                                    {
                                        Subject = "Hız Limiti Aşımı",
                                        Body = bodyMail,
                                        Type = (int)MessageLogType.EMail,
                                        Email = driveUser.Email,
                                        MailCc = mailCc,
                                        //MailBcc = "eyup.yesilova@basaranteknoloji.net;onerozkara@basaranteknoloji.net;raziyeorhan@basaranteknoloji.net" //todo:silinecek
                                    }));
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }

        public async Task InsertPlateCoordinate(List<ESpeedDto> speedList)
        {
            try
            {
                var dateNow = DateTime.Now;
                var vehicleList = await Task.FromResult(_vehicleRepository.Where(w => w.Status && w.ArventoNo != null).ToList());
                var arventoNoList = speedList.GroupBy(g => g.DeviceNo).Select(s => s.Key).ToList();

                var list = new List<VehicleCoordinate>();
                foreach (var item in arventoNoList)
                {
                    var vehicle = vehicleList.FirstOrDefault(f => f.ArventoNo == item);
                    if (vehicle != null)
                    {
                        var coordinateList = speedList.Where(w => w.GmtDateTime != null && w.DeviceNo == item).Select(s => new VehicleCoordinate
                        {
                            VehicleId = vehicle.Id,
                            Latitude = s.Latitude.ToString(),
                            Longitude = s.Longitude.ToString(),
                            Speed = s.Speed.ToString(),
                            LocalDate = s.GmtDateTime.Value
                        }).ToList();
                        list.AddRange(coordinateList);
                    }
                }

                var entities = list.Distinct().OrderBy(o => o.LocalDate).ToList();
                await _vehicleCoordinateRepository.InsertRangeAsync(entities);
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Arvento No Güncelleme
        public void Arvento2ErpNodeGuncelle()
        {
            try
            {
                if (IsJobRun())
                    return;

                JobSetTrueFalse("true");

                var date = DateTime.Now;
                var nodeList = GetArventoPlateList();

                JobSetTrueFalse("false");

                if (nodeList.Count == 1 && nodeList[0].Node == null)
                {
                    MailSender("Arvento Plaka Node Aktarımı Hk.", "Plakaya bağlı node bilgileri Arvento'dan çekilemedi. Method name: Arvento2ErpNodeGuncelle");
                    return;
                }

                var vehicleList = _vehicleRepository.Where(w => w.Status).ToList();
                var isChanged = false;
                foreach (var item in nodeList)
                {
                    var vehicle = vehicleList.FirstOrDefault(w => w.Status && w.Plate == item.Plate);
                    if (vehicle != null && vehicle.ArventoNo != item.Node)
                    {
                        isChanged = true;
                        vehicle.ArventoNo = item.Node;
                    }
                }
                if (isChanged)//değişiklik varsa güncelle
                {
                    _vehicleRepository.UpdateRange(vehicleList);
                    _uow.SaveChanges();
                }
            }
            catch (Exception)
            {
                JobSetTrueFalse("false");
                MailSender("Arvento Plaka Node Aktarımı Hk.", "Plakaya bağlı node bilgileri veritabana güncellenirken hata oluştu");
            }

        }

        public List<EVehicleArventoDto> GetArventoPlateList()
        {
            var url = rootUrl + $"GetIMSIList?Username={userName}&PIN1={password}&PIN2={password}";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            List<EVehicleArventoDto> list = new List<EVehicleArventoDto>();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                foreach (XmlNode child in doc.ChildNodes)
                {
                    foreach (XmlNode child2 in child.ChildNodes)
                    {
                        foreach (XmlNode child3 in child2.ChildNodes)
                        {
                            if (child3.Name == "NewDataSet")
                            {
                                foreach (XmlNode item in child3.ChildNodes)
                                {
                                    var model = new EVehicleArventoDto();
                                    if (item.SelectSingleNode("Node") != null)
                                        model.Node = item.SelectSingleNode("Node").FirstChild.Value;
                                    if (item.SelectSingleNode("LicensePlate") != null)
                                        model.Plate = item.SelectSingleNode("LicensePlate").FirstChild.Value.Replace(" ", "");
                                    if (item.SelectSingleNode("Driver") != null)
                                        model.Driver = item.SelectSingleNode("Driver").FirstChild.Value;
                                    if (item.SelectSingleNode("IMSI") != null)
                                        model.IMSI = item.SelectSingleNode("IMSI").FirstChild.Value;
                                    list.Add(model);
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }
        #endregion

        #region Aracın koordinatlarını günceller
        public void ArventoPlakaKoordinatEkle()
        {
            try
            {
                var dateNow = DateTime.Now;
                if (dateNow.Hour == 17 || dateNow.Hour == 18 || dateNow.Hour == 19)//Mesai dışı çalışıyor 19:01'de
                    return;

                if (IsJobRun())
                    return;

                JobSetTrueFalse("true");

                var taskScheduler = _jobRepository.FirstOrDefault(f => f.Name == "ArventoCoordinate");
                if (taskScheduler != null)
                {
                    var startDate = taskScheduler.EndDate;
                    var endDate = DateTime.Now;
                    var tripList = _tripRepository.Where(w => w.Status && w.StartDate.Date >= startDate).ToList();
                    var vehicleList = _vehicleRepository.Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo)).ToList();

                    foreach (var item in vehicleList)
                    {
                        try
                        {
                            var start = DateTime.Now;
                            var coordinateList = GeneralReport(startDate, endDate, item.ArventoNo);

                            var list = new List<VehicleCoordinate>();
                            foreach (var co in coordinateList)
                            {
                                list.Add(new VehicleCoordinate()
                                {
                                    VehicleId = item.Id,
                                    Latitude = co.Enlem,
                                    Longitude = co.Boylam,
                                    Speed = co.Hiz,
                                    Driver = co.Surucu,
                                    LocalDate = co.Tarih
                                });
                            }
                            var entities = list.OrderBy(o => o.LocalDate).ToList();
                            _vehicleCoordinateRepository.InsertRange(entities);
                            _uow.SaveChanges();

                            var end = DateTime.Now;
                            int diffSeconds = (int)(end - start).TotalSeconds;//arvento 30 sn'de bir istek kabul ediyor
                            if (diffSeconds < 30)
                                Thread.Sleep((30 - diffSeconds) * 1000);
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(30 * 1000);
                        }
                    }

                    taskScheduler.StartDate = startDate;
                    taskScheduler.EndDate = endDate;
                    taskScheduler.LastRunDate = DateTime.Now;
                    taskScheduler.ErrorMessage = null;
                    UpdateJob(taskScheduler);
                }

            }
            catch (Exception ex)
            {
                JobSetTrueFalse("false");
                MailSender("Arvento Koordinat Hk.", "Plakaya bağlı koordinat bilgileri veritabana eklenirken hata oluştu");
            }

            JobSetTrueFalse("false");
        }

        #endregion

        #region Mail İşlemleri
        public void MailSender(string subject, string body)
        {
            try
            {
                var adminMailList = _parameterRepository.FirstOrDefault(f => f.KeyP == ParameterEnum.AdminMailList.ToString());
                var mailList = adminMailList?.ValueP;
                if (!string.IsNullOrEmpty(mailList))
                    _mailService.SendMail(mailList, subject, body);
            }
            catch (Exception ex) { }
        }

        public void UpdateJob(TaskScheduler_ entity)
        {
            try
            {
                _jobRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }

        //Arvento'ya 30 sn'de istek atar
        public void SearchSecondCheck(DateTime start)
        {
            var end = DateTime.Now;
            int diffSeconds = (int)(end - start).TotalSeconds;//arvento 30 sn'de bir istek kabul ediyor
            if (diffSeconds < 30)
                Thread.Sleep((30 - diffSeconds) * 1000);
        }

        public string GetCityName(int cityId)
        {
            var result = (from c1 in _cityRepository.GetAll()
                          join c2 in _cityRepository.GetAll() on c1.ParentId equals c2.Id
                          where c1.Id == cityId
                          select new ETripDto()
                          {
                              StartCityName = c2.Name + "-" + c1.Name
                          }).FirstOrDefault();

            if (result != null)
                return result.StartCityName;
            return "";
        }
        #endregion

        #region Genel İşlemler 
        //UnitId'ye göre araçları listeler (Kurumsal iletişim, Satış pazarlama araçları gibi)
        public async Task<List<Vehicle>> GetVehicleListByUnitId(int parentUnitId)
        {
            var list = await (from v in _vehicleRepository.GetAll()
                              join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                              from unit in unitL.DefaultIfEmpty()
                              join unit2 in _unitRepository.GetAll() on unit.ParentId equals unit2.Id into unit2L
                              from unit2 in unit2L.DefaultIfEmpty()
                              where v.Status && unit2.Id == parentUnitId
                              select v).ToListAsync();

            return list;
        }
        #endregion
    }
}
