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
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace CoreArchV2.Services.Arvento
{
    public class ArventoService : IArventoService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        string _rootUrl = "http://ws.arvento.com/v1/report.asmx/";
        string _userName = string.Empty;
        string _password = string.Empty;

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
            _userName = _arventoSetting.UserName;
            _password = _arventoSetting.Password;
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
        public async Task<EPlateFromNodeDto> GetLicensePlateFromNode(string node)
        {
            if (string.IsNullOrWhiteSpace(node))
                return null;

            var url = $"{_rootUrl}GetLicensePlateFromNode";

            var payload = new
            {
                Username = _userName,
                PIN1 = _password,
                PIN2 = _password,
                Node = node
            };

            try
            {
                // HttpClient reuse edilmeli (örneğin sınıfın içinde static olarak tutulabilir)
                using var client = new HttpClient();

                var json = JsonConvert.SerializeObject(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                    return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseBody))
                    return null;

                // Bazı Arvento endpoint’leri JSON yerine XML benzeri içerik dönebiliyor
                if (responseBody.TrimStart().StartsWith("<"))
                    return null;

                var result = JsonConvert.DeserializeObject<EPlateFromNodeDto>(responseBody);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Verilen tarihe göre plakanın koordinatlarını listeler
        public async Task<List<EGeneralReport2Dto>> GeneralReport(DateTime start, DateTime end, string node)
        {
            var list = new List<EGeneralReport2Dto>();

            try
            {
                var url = _rootUrl + $"GeneralReport2?Username={_userName}&PIN1={_password}&PIN2={_password}&" +
                 "StartDate=" + start.ToString("MMddyyyyHHmmss") +
                 "&EndDate=" + end.ToString("MMddyyyyHHmmss") +
                 "&Node=" + node +
                 "&Group=&Compress=&chkLocation=1&chkSpeed=1&chkPause=1&chkMotion=1&chkRegion=1&txtSpeedMin=&txtSpeedMax=&chkTemperatureSensor1=1&chkTemperatureSensorPer1=1&chkTemperatureSensorAlm1=1&chkTemperatureSensor2=1&chkTemperatureSensorPer2=1&chkTemperatureSensorAlm2=1&txtTemperatureMin=&txtTemperatureMax=&chkEmergency=1&chkDoor=1&chkPauseTime=1&chkContactAlarm=1&chkIdlingTime=1&chkIdlingAlarm=1&chkFuelLevel=1&chkPower=1&chkDriverIdentification=1&chkPossibleAccident=1&chkAcceleration=1&chkVehicleMovedWithoutDriverCard=1&MinuteDif=UTC+02:00&Language=0";

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                    return list;

                var result = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(result))
                    return list;

                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(result);
                }
                catch (Exception xmlEx)
                {
                    return new List<EGeneralReport2Dto>();
                }

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

                                        if (item.SelectSingleNode("Kayıt_x0020_No")?.FirstChild != null)
                                            model.KayitNo = Convert.ToInt32(item.SelectSingleNode("Kayıt_x0020_No").FirstChild.Value);

                                        if (item.SelectSingleNode("Cihaz_x0020_No")?.FirstChild != null)
                                            model.CihazNo = item.SelectSingleNode("Cihaz_x0020_No").FirstChild.Value;

                                        if (item.SelectSingleNode("Plaka")?.FirstChild != null)
                                            model.Plaka = item.SelectSingleNode("Plaka").FirstChild.Value.Replace(" ", "");

                                        if (item.SelectSingleNode("Sürücü")?.FirstChild != null)
                                            model.Surucu = item.SelectSingleNode("Sürücü").FirstChild.Value;

                                        if (item.SelectSingleNode("Tarih_x002F_Saat")?.FirstChild != null)
                                            model.Tarih = Convert.ToDateTime(Convert.ToDateTime(item.SelectSingleNode("Tarih_x002F_Saat").FirstChild.Value)
                                                .ToString("dd.MM.yyyy HH:mm"));

                                        if (item.SelectSingleNode("Tür")?.FirstChild != null)
                                            model.Tur = item.SelectSingleNode("Tür").FirstChild.Value;

                                        if (item.SelectSingleNode("Hız_x0020_km_x002F_s")?.FirstChild != null)
                                            model.Hiz = item.SelectSingleNode("Hız_x0020_km_x002F_s").FirstChild.Value;

                                        if (item.SelectSingleNode("Adres")?.FirstChild != null)
                                            model.Adres = item.SelectSingleNode("Adres").FirstChild.Value;

                                        model.Enlem = item.SelectSingleNode("Enlem")?.FirstChild?.Value;
                                        model.Boylam = item.SelectSingleNode("Boylam")?.FirstChild?.Value;

                                        if (item.SelectSingleNode("Duraklama_x0020_Süresi")?.FirstChild != null)
                                            model.DuraklamaSuresi = item.SelectSingleNode("Duraklama_x0020_Süresi").FirstChild.Value;

                                        list.Add(model);
                                    }
                                }
                            }
                        }
                    }
                }

                var groupCoordinate = list
                    .GroupBy(g => new { g.Enlem, g.Boylam })
                    .Select(s => new EGeneralReport2Dto()
                    {
                        Enlem = s.Key.Enlem,
                        Boylam = s.Key.Boylam,
                        Plaka = s.FirstOrDefault()?.Plaka,
                        Surucu = s.FirstOrDefault()?.Surucu,
                        Tarih = s.OrderBy(o => o.Tarih).FirstOrDefault()?.Tarih ?? DateTime.MinValue,
                        Hiz = s.FirstOrDefault()?.Hiz,
                        DuraklamaSuresi = s.OrderByDescending(o => o.KayitNo).FirstOrDefault()?.DuraklamaSuresi
                    })
                    .GroupBy(a => a.Tarih)
                    .Select(b => new EGeneralReport2Dto()
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
            catch (TaskCanceledException)
            {
                return new List<EGeneralReport2Dto>();
            }
            catch (HttpRequestException ex)
            {
                return new List<EGeneralReport2Dto>();
            }
            catch (Exception ex)
            {
                return new List<EGeneralReport2Dto>();
            }
        }

        //Verilen günler arasındaki koordinatları çeker !!Manuel kullanım için
        public async Task InsertPlateCoordinateRange()
        {
            try
            {
                var vehicleList = _vehicleRepository
                    .Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo))
                    .ToList();

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
                        var coordinateList = await GeneralReport(startDate2, endDate2, item.ArventoNo);
                        if (coordinateList.Any())
                        {
                            coordinateList = coordinateList.OrderBy(o => o.Tarih).ToList();
                            var list = new List<VehicleCoordinate>(coordinateList.Count);

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
                            await _vehicleCoordinateRepository.InsertRangeAsync(entities);
                            await _uow.SaveChangesAsync();

                            var end = DateTime.Now;
                            int diffSeconds = (int)(end - start).TotalSeconds;

                            // Arvento API 30 saniyede bir istek kabul ediyor
                            if (diffSeconds < 30)
                            {
                                int delay = (30 - diffSeconds) * 1000;
                                await Task.Delay(delay);
                            }
                        }
                        else
                        {
                            await Task.Delay(5000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Task.Delay(30000);
            }
        }

        public async Task<VehicleOperatingReport> VehicleOperatingReport(string node, DateTime start, DateTime end)
        {
            var model = new VehicleOperatingReport();
            try
            {
                var url = _rootUrl + $"VehicleOperatingReport?Username={_userName}&PIN1={_password}&PIN2={_password}&" +
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
                var url = _rootUrl + $"VehicleOperatingReport?Username={_userName}&PIN1={_password}&PIN2={_password}&" +
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
            if (start.Hour != 7 || start.DayOfWeek == DayOfWeek.Saturday)
                return;

            try
            {
                var dateNow = DateTime.Now;
                var startDate = dateNow;
                var endDate = dateNow;
                var startDate2 = dateNow;
                var endDate2 = dateNow;
                var unique = "";

                var startDateEntity = startDate;
                var endDateEntity = endDate;

                // 🔹 Tarih aralıkları hesaplanıyor
                if (dateNow.DayOfWeek == DayOfWeek.Monday)
                {
                    // Pazartesi sabahı: pazar gecesi dahil
                    startDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 7, 0, 0);
                    dateNow = dateNow.AddDays(-1);
                    unique = dateNow.ToString("ddMMyyyy");
                    startDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 59, 59);

                    startDateEntity = startDate;
                    endDateEntity = endDate2;
                }
                else if (dateNow.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Pazar günü: cuma akşamı dahil
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
                    // Diğer günlerde: bir önceki günün 19:00–23:59 ve bugünün 00:00–07:00 arası
                    startDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                    endDate2 = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 7, 0, 0);
                    dateNow = dateNow.AddDays(-1);
                    startDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 19, 0, 0);
                    endDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 59, 59);
                    unique = dateNow.ToString("ddMMyyyy");

                    startDateEntity = startDate;
                    endDateEntity = endDate2;
                }

                var vehicleList = _vehicleRepository
                    .Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo))
                    .ToList();

                foreach (var item in vehicleList)
                {
                    try
                    {
                        var data = new VehicleOperatingReport();
                        var templist = new List<VehicleOperatingReport>();

                        // 1️⃣ Akşam (19:00–23:59)
                        var firstRow = await VehicleOperatingReport(item.ArventoNo, startDate, endDate);
                        if (firstRow != null && !string.IsNullOrEmpty(firstRow.MesafeKm) && firstRow.MesafeKm != "0")
                            templist.Add(firstRow);

                        start = DateTime.Now;
                        SearchSecondCheck(start);

                        // 2️⃣ Sabah (00:00–07:00)
                        var secondRow = await VehicleOperatingReport(item.ArventoNo, startDate2, endDate2);
                        if (secondRow != null && !string.IsNullOrEmpty(secondRow.MesafeKm) && secondRow.MesafeKm != "0")
                            templist.Add(secondRow);

                        start = DateTime.Now;
                        if (templist.Any())
                        {
                            data = await GetGroupBySingle(templist);

                            if (data != null)
                            {
                                if (!string.IsNullOrEmpty(firstRow?.MesafeKm) && firstRow.MesafeKm != "0" &&
                                    !string.IsNullOrEmpty(secondRow?.MesafeKm) && secondRow.MesafeKm != "0")
                                {
                                    data.IlkKontakAcildi = firstRow.IlkKontakAcildi;
                                    data.SonKontakKapandi = secondRow.SonKontakKapandi;
                                }
                                else if (string.IsNullOrEmpty(firstRow?.MesafeKm) || firstRow.MesafeKm == "0")
                                {
                                    data.IlkKontakAcildi = secondRow?.IlkKontakAcildi;
                                    data.SonKontakKapandi = secondRow?.SonKontakKapandi;
                                }
                                else
                                {
                                    data.IlkKontakAcildi = firstRow?.IlkKontakAcildi;
                                    data.SonKontakKapandi = firstRow?.SonKontakKapandi;
                                }
                            }
                        }

                        if (data != null && !string.IsNullOrEmpty(data.ArventoNo) &&
                            !string.IsNullOrEmpty(data.MesafeKm) && data.MesafeKm != "0")
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
                    catch (Exception ex)
                    {
                        SearchSecondCheck(start);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MailSender("ArventoMesaiDisiKullanimRaporu Hk.",
                    "Araç raporu çekilirken hata oluştu. Method name: ArventoMesaiDisiKullanimRaporu");
            }
        }
        public async Task<VehicleOperatingReport> GetGroupBySingle(List<VehicleOperatingReport> list)
        {
            try
            {
                if (list == null || !list.Any())
                    return null;

                // CPU-bound işlemi arka planda çalıştırıyoruz
                return await Task.Run(() =>
                {
                    var groupByList = list.GroupBy(g => g.ArventoNo).ToList();

                    var entity = groupByList
                        .Select(s => new VehicleOperatingReport()
                        {
                            MesafeKm = s.Sum(x => SafeDecimal(x.MesafeKm)).ToString("0.##"),
                            MaxHiz = s.Max(x => SafeDecimal(x.MaxHiz)).ToString("0.##"),

                            Tarih = s.FirstOrDefault()?.Tarih,
                            ArventoNo = s.FirstOrDefault()?.ArventoNo,
                            ArventoPlaka = s.FirstOrDefault()?.ArventoPlaka?.Replace(" ", "").ToUpper(),

                            DuraklamaSuresiSaat = s.Sum(x => SafeInt(x.DuraklamaSuresiSaat)),
                            DuraklamaSuresiDakika = s.Sum(x => SafeInt(x.DuraklamaSuresiDakika)),
                            DuraklamaSuresiSaniye = s.Sum(x => SafeInt(x.DuraklamaSuresiSaniye)),

                            RolantiSuresiSaat = s.Sum(x => SafeInt(x.RolantiSuresiSaat)),
                            RolantiSuresiDakika = s.Sum(x => SafeInt(x.RolantiSuresiDakika)),
                            RolantiSuresiSaniye = s.Sum(x => SafeInt(x.RolantiSuresiSaniye)),

                            HareketSuresiSaat = s.Sum(x => SafeInt(x.HareketSuresiSaat)),
                            HareketSuresiDakika = s.Sum(x => SafeInt(x.HareketSuresiDakika)),
                            HareketSuresiSaniye = s.Sum(x => SafeInt(x.HareketSuresiSaniye)),

                            KontakAcikKalmaSuresiSaat = s.Sum(x => SafeInt(x.KontakAcikKalmaSuresiSaat)),
                            KontakAcikKalmaSuresiDakika = s.Sum(x => SafeInt(x.KontakAcikKalmaSuresiDakika)),
                            KontakAcikKalmaSuresiSaniye = s.Sum(x => SafeInt(x.KontakAcikKalmaSuresiSaniye)),

                            AracSonDurumBilgileri = s.FirstOrDefault()?.AracSonDurumBilgileri,

                            HizAlarm = s.Sum(x => SafeInt(x.HizAlarm)).ToString(),
                            SehirIciHizAlarm = s.Sum(x => SafeInt(x.SehirIciHizAlarm)).ToString(),
                            SehirDisiHizAlarm = s.Sum(x => SafeInt(x.SehirDisiHizAlarm)).ToString(),
                            OtoyolHizAlarm = s.Sum(x => SafeInt(x.OtoyolHizAlarm)).ToString(),
                            RolantiAlarm = s.Sum(x => SafeInt(x.RolantiAlarm)).ToString(),
                            DuraklamaAlarm = s.Sum(x => SafeInt(x.DuraklamaAlarm)).ToString(),
                            HareketAlarm = s.Sum(x => SafeInt(x.HareketAlarm)).ToString(),
                            KontakAcildiAlarm = s.Sum(x => SafeInt(x.KontakAcildiAlarm)).ToString(),
                            KontakKapandiAlarm = s.Sum(x => SafeInt(x.KontakKapandiAlarm)).ToString(),
                            AniHizlanmaAlarm = s.Sum(x => SafeInt(x.AniHizlanmaAlarm)).ToString(),
                            AniYavaslamaAlarm = s.Sum(x => SafeInt(x.AniYavaslamaAlarm)).ToString(),
                            MotorDevirAsimiAlarm = s.Sum(x => SafeInt(x.MotorDevirAsimiAlarm)).ToString(),
                        })
                        .FirstOrDefault();

                    if (entity == null)
                    {
                        Console.WriteLine("[Arvento] GetGroupBySingle: entity null döndü.");
                        return null;
                    }

                    // ✅ Property'leri local değişkenlere al -> ref olarak normalize et -> geri ata
                    NormalizeEntityTimes(entity);

                    Console.WriteLine($"[Arvento] GetGroupBySingle: entity oluşturuldu (Plaka: {entity.ArventoPlaka})");
                    return entity;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Arvento ERROR] GetGroupBySingle hata: {ex.Message}");
                return null;
            }
        }
        private static int SafeInt(object value)
        {
            if (value == null) return 0;
            if (value is int i) return i;
            if (int.TryParse(value.ToString(), out var result))
                return result;
            return 0;
        }
        private static decimal SafeDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(",", ".");
            return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }
        private static void NormalizeTime(int? saniye, int? dakika, int? saat)
        {
            int s = saniye ?? 0, d = dakika ?? 0, h = saat ?? 0;
            NormalizeTime(s, d, h);
            saniye = s; dakika = d; saat = h;
        }

        private static void NormalizeEntityTimes(VehicleOperatingReport entity)
        {
            // Duraklama
            int dS = entity.DuraklamaSuresiSaniye ?? 0;
            int dD = entity.DuraklamaSuresiDakika ?? 0;
            int dH = entity.DuraklamaSuresiSaat ?? 0;
            NormalizeTime(dS, dD, dH);
            entity.DuraklamaSuresiSaniye = dS;
            entity.DuraklamaSuresiDakika = dD;
            entity.DuraklamaSuresiSaat = dH;

            // Rölanti
            int rS = entity.RolantiSuresiSaniye ?? 0;
            int rD = entity.RolantiSuresiDakika ?? 0;
            int rH = entity.RolantiSuresiSaat ?? 0;
            NormalizeTime(rS, rD, rH);
            entity.RolantiSuresiSaniye = rS;
            entity.RolantiSuresiDakika = rD;
            entity.RolantiSuresiSaat = rH;

            // Hareket
            int hS = entity.HareketSuresiSaniye ?? 0;
            int hD = entity.HareketSuresiDakika ?? 0;
            int hH = entity.HareketSuresiSaat ?? 0;
            NormalizeTime(hS, hD, hH);
            entity.HareketSuresiSaniye = hS;
            entity.HareketSuresiDakika = hD;
            entity.HareketSuresiSaat = hH;

            // Kontak
            int kS = entity.KontakAcikKalmaSuresiSaniye ?? 0;
            int kD = entity.KontakAcikKalmaSuresiDakika ?? 0;
            int kH = entity.KontakAcikKalmaSuresiSaat ?? 0;
            NormalizeTime(kS, kD, kH);
            entity.KontakAcikKalmaSuresiSaniye = kS;
            entity.KontakAcikKalmaSuresiDakika = kD;
            entity.KontakAcikKalmaSuresiSaat = kH;
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

                if (dateNow.DayOfWeek == DayOfWeek.Saturday || dateNow.DayOfWeek == DayOfWeek.Sunday)
                    return;

                #region Mesai İçi Kullanım
                var startDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 7, 0, 0); // 07:00
                var endDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 19, 0, 0); // 19:00

                var vehicleList = _vehicleRepository
                    .Where(w => w.Status && !string.IsNullOrEmpty(w.ArventoNo))
                    .ToList();

                foreach (var item in vehicleList)
                {
                    try
                    {
                        var start = DateTime.Now;
                        var data = await VehicleOperatingReport(item.ArventoNo, startDate, endDate);

                        if (data != null && !string.IsNullOrEmpty(data.ArventoNo) &&
                            !string.IsNullOrEmpty(data.MesafeKm) && data.MesafeKm != "0")
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
                        int diffSeconds = (int)(end - start).TotalSeconds;

                        // Arvento API 30 saniyede bir istek kabul ediyor
                        if (diffSeconds < 30)
                        {
                            int delayMs = (30 - diffSeconds) * 1000;
                            await Task.Delay(delayMs);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(30000);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MailSender("ArventoMesaiIciKullanimRaporu Hk.",
                    "Araç raporu çekilirken hata oluştu. Method name: ArventoMesaiIciKullanimRaporu");
            }
        }

        #endregion

        #region Arvento job çalışıyor mu ?
        public async Task<bool> IsJobRun()
        {
            try
            {
                var param = await _parameterRepository
                    .FirstOrDefaultNoTrackingAsync(f => f.KeyP == "IsArventoJobRun");

                if (param?.ValueP is null)
                    return false;

                if (bool.TryParse(param.ValueP.ToString(), out bool isRunning))
                    return isRunning;

                return Convert.ToBoolean(param.ValueP);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task JobSetTrueFalse(string val)
        {
            try
            {
                var param = await _parameterRepository
                    .FirstOrDefaultAsync(f => f.KeyP == "IsArventoJobRun");

                if (param == null)
                    return;

                param.ValueP = val;
                _parameterRepository.Update(param);

                await _uow.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region Mesai İçi/Dışı Kullanım Raporu Mail Gönder
        public async Task AracKullanimRaporuMailGonder()
        {
            if (new ModeDetector().IsDebug)
                return;

            var now = DateTime.Now;
            var dateLastDay = now.AddDays(-1);

            if (dateLastDay.Hour != 12)
                return;

            try
            {
                var list = await _vehicleOperationReportRepository
                    .WhereAsync(w => w.Status && !w.IsSendMail && w.UniqueLine == dateLastDay.ToString("ddMMyyyy"));

                if (!list.Any())
                    return;

                var dateNow = now.ToString("dd.MM.yyyy HH:mm");
                var mailTemplate = await _parameterRepository
                    .FirstOrDefaultAsync(f => f.KeyP == ParameterEnum.GorevDisiKullanimMailTemplate.ToString());

                if (mailTemplate == null || string.IsNullOrEmpty(mailTemplate.ValueP))
                    return;

                var baseTemplate = mailTemplate.ValueP
                    .Replace("{dateNow}", dateNow);

                // Tarih placeholderları dolduruluyor
                var startDateIc = list.FirstOrDefault(f => f.Type == 1);
                var startDateDis = list.FirstOrDefault(f => f.Type == 0);

                baseTemplate = baseTemplate
                    .Replace("{Mesai_Ici_Tarihi}", startDateIc != null
                        ? $"{startDateIc.StartDate:dd MMMM HH:mm}<br>{startDateIc.EndDate:dd MMMM HH:mm}"
                        : "-")
                    .Replace("{Mesai_Disi_Tarihi}", startDateDis != null
                        ? $"{startDateDis.StartDate:dd MMMM HH:mm}<br>{startDateDis.EndDate:dd MMMM HH:mm}"
                        : "-");

                var groupedPlates = list.GroupBy(g => g.VehicleId).ToList();

                var disabledProjects = await GetDisabledProjectMailList();
                var allUnits = await GetAllUnit();

                foreach (var group in groupedPlates)
                {
                    var first = group.FirstOrDefault();
                    if (first == null) continue;

                    var unit = allUnits.FirstOrDefault(f => f.Id == first.LastUnitId);
                    if (unit == null) continue;

                    if (disabledProjects.Any(x => x.Id == unit.Id || x.Id == unit.ParentId))
                        continue;

                    try
                    {
                        var mesaiIci = group.FirstOrDefault(f => f.Type == 1);
                        var mesaiDisi = group.FirstOrDefault(f => f.Type == 0);

                        var mailContent = await BuildMailTemplateAsync(baseTemplate, mesaiIci, mesaiDisi, group.Key);
                        if (mailContent == null)
                            continue;

                        var userId = mesaiIci?.LastDebitUserId ?? mesaiDisi?.LastDebitUserId;
                        if (userId <= 0)
                            continue;

                        var result = await SendMailFromTrip(userId.Value, mailContent, first.ArventoPlaka);
                        if (result.IsSuccess)
                        {
                            foreach (var entity in group)
                                entity.IsSendMail = true;

                            _vehicleOperationReportRepository.UpdateRange(group);
                            await _uow.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async Task<string> BuildMailTemplateAsync(string baseTemplate, VehicleOperatingReport? mesaiIci, VehicleOperatingReport? mesaiDisi, int vehicleId)
        {
            var template = new StringBuilder(baseTemplate);

            string Replace(string key, string value) =>
                template.Replace(key, value ?? "-").ToString();

            try
            {
                var culture = new System.Globalization.CultureInfo("tr-TR");

                // Mesai içi
                if (mesaiIci != null)
                {
                    var gorevIci = _tripRepository
                        .Where(f => f.Status && f.VehicleId == vehicleId && mesaiIci.StartDate <= f.StartDate && f.StartDate <= mesaiIci.EndDate)
                        .OrderByDescending(o => o.Id)
                        .FirstOrDefault();

                    Replace("{ArventoKm_ic}", $"{mesaiIci.MesafeKm} Km");
                    Replace("{Max_hiz_ic}", $"{mesaiIci.MaxHiz} Km/h");
                    Replace("{GorevAcildiMi_ic}", gorevIci != null ? "✔ Evet" : "✘");

                    if (gorevIci != null)
                    {
                        var timespan = (gorevIci.State == (int)TripState.EndTrip)
                            ? (gorevIci.EndDate!.Value - gorevIci.StartDate)
                            : (DateTime.Now - gorevIci.StartDate);

                        var sure = $"{timespan.Days} Gün, {timespan.Hours} Saat, {timespan.Minutes} Dk";
                        Replace("{GorevSuresi_ic}", sure);
                        Replace("{GorevdeYaptigiKm_ic}", $"{gorevIci.EndKm - gorevIci.StartKm} Km");
                    }
                }

                // Mesai dışı
                if (mesaiDisi != null)
                {
                    var gorevDisi = _tripRepository
                        .Where(f => f.Status && f.VehicleId == vehicleId && mesaiDisi.StartDate <= f.StartDate && f.StartDate <= mesaiDisi.EndDate)
                        .OrderByDescending(o => o.Id)
                        .FirstOrDefault();

                    Replace("{ArventoKm_dis}", $"{mesaiDisi.MesafeKm} Km");
                    Replace("{Max_hiz_dis}", $"{mesaiDisi.MaxHiz} Km/h");
                    Replace("{GorevAcildiMi_dis}", gorevDisi != null ? "✔ Evet" : "✘");

                    if (gorevDisi != null)
                    {
                        var timespan = (gorevDisi.State == (int)TripState.EndTrip)
                            ? (gorevDisi.EndDate!.Value - gorevDisi.StartDate)
                            : (DateTime.Now - gorevDisi.StartDate);

                        var sure = $"{timespan.Days} Gün, {timespan.Hours} Saat, {timespan.Minutes} Dk";
                        Replace("{GorevSuresi_dis}", sure);
                        Replace("{GorevdeYaptigiKm_dis}", $"{gorevDisi.EndKm - gorevDisi.StartKm} Km");
                    }
                }

                return template.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MailTemplateHata] AraçId={vehicleId}, Hata: {ex.Message}");
                return null;
            }
        }
        public async Task<List<Unit>> GetDisabledProjectMailList()
        {
            var list = await (from vo in _vehicleOpParam.GetAll()
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
                              }).ToListAsync();

            return list;
        }
        public async Task<List<EUnitDto>> GetAllUnit()
        {
            var list = await (from u in _unitRepository.GetAll()
                              join u2 in _unitRepository.GetAll() on u.ParentId equals u2.Id into unit2L
                              from u2 in unit2L.DefaultIfEmpty()
                              select new EUnitDto
                              {
                                  Id = u.Id,
                                  ParentId = u.ParentId,
                                  Name = u.Name + "/" + u2.Name
                              }).ToListAsync();

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
                var url = _rootUrl + $"GetVehicleStatusJSON?Username={_userName}&PIN1={_password}&PIN2={_password}&callBack=c";
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
            var url = _rootUrl + $"GetVehicleStatusJSON?Username={_userName}&PIN1={_password}&PIN2={_password}&callBack=c";
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

            var checkDate = DateTime.UtcNow.AddSeconds(-5); // bir tık daha güvenli aralık
            string url = $"{_rootUrl}GetVehicleAlarmStatusJson?Username={_userName}&PIN1={_password}&PIN2={_password}&Language=0";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode) return;

                var dataString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(dataString))
                    return;

                // JSON temizleme
                dataString = dataString.Trim();
                if (dataString.StartsWith("\"")) dataString = dataString.Substring(1);
                if (dataString.EndsWith("\"")) dataString = dataString.Substring(0, dataString.Length - 1);
                dataString = dataString.Replace("\\\"", "\"");

                var list = JsonConvert.DeserializeObject<List<ESpeedDto>>(dataString);
                if (list == null || !list.Any()) return;

                // sadece son 5 saniye içindeki kayıtlar
                var filtered = list.Where(w => w.GmtDateTime >= checkDate).ToList();
                if (!filtered.Any()) return;

                var speedList = filtered.Where(w => w.AlarmType == "Harita Hız Alarmı").ToList();
                var coordinateList = filtered.Where(w => w.AlarmType == "Konum Bilgisi").ToList();

                var tasks = new List<Task>();
                if (speedList.Any())
                    tasks.Add(InsertNotice(speedList));

                if (coordinateList.Any())
                    tasks.Add(InsertPlateCoordinate(coordinateList));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex) { }
        }

        private async Task InsertNotice(List<ESpeedDto> speedList)
        {
            try
            {
                var vehicleList = _vehicleRepository.Where(w => w.Status && w.ArventoNo != null).ToList();
                var cityList = _cityRepository.Where(w => w.ParentId == null).ToList();

                foreach (var group in speedList.GroupBy(g => g.DeviceNo))
                {
                    var deviceNo = group.Key;
                    var vehicle = vehicleList.FirstOrDefault(f => f.ArventoNo == deviceNo);
                    if (vehicle == null) continue;

                    var maxSpeed = (vehicle.MaxSpeed ?? 120) * 1.2m;
                    var topSpeed = group.OrderByDescending(o => o.Speed)
                                        .FirstOrDefault(o => o.Speed >= (double)maxSpeed);

                    if (topSpeed == null) continue;

                    var address = topSpeed.Address ?? "";
                    var addressParts = address.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var cityName = addressParts.Length >= 2 ? addressParts[^2].Trim().ToLowerInvariant() : "";
                    var cityId = cityList.FirstOrDefault(c => c.Name.ToLowerInvariant().Contains(cityName))?.Id;

                    var notice = new Notice
                    {
                        CreatedBy = 1,
                        VehicleId = vehicle.Id,
                        CityId = cityId,
                        NoticeType = (int)NoticeType.Speed,
                        TransactionDate = topSpeed.GmtDateTime,
                        Speed = (decimal)topSpeed.Speed,
                        Address = address,
                        ImportType = (int)ImportType.Arvento,
                        State = (int)NoticeState.Draft,
                        LastDebitUserId = vehicle.LastUserId,
                        LastUnitId = vehicle.LastUnitId,
                        LastDebitStatus = vehicle.LastStatus,
                        LastDebitKm = vehicle.LastKm
                    };

                    await _noticeRepository.InsertAsync(notice);
                    await _uow.SaveChangesAsync();

                    if (vehicle.LastUserId.HasValue)
                        _ = NotifyDriver(vehicle, topSpeed, cityId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InsertNotice ERROR] {ex.Message}");
            }
        }

        private async Task NotifyDriver(Vehicle vehicle, ESpeedDto topSpeed, int? cityId)
        {
            try
            {
                var speed = (decimal)topSpeed.Speed;
                var address = topSpeed.Address;
                var date = topSpeed.GmtDateTime;

                // Push Notification
                await _mobileService.PushNotificationAsync(new EMessageLogDto
                {
                    Subject = "Hız Limiti Aşımı",
                    Body = $"{vehicle.Plate} plakalı araç için hız {speed} km/h. Adres: {address}",
                    Type = (int)MessageLogType.PushNotification,
                    UserId = vehicle.LastUserId.Value
                });

                // Email
                var driver = await _userRepository.FindAsync(vehicle.LastUserId.Value);
                if (driver == null || string.IsNullOrEmpty(driver.Email)) return;

                var bodyMail = $@"
                Merhabalar,<br/>
                {vehicle.Plate} plakalı araç için aşırı hız algılandı.<br/>
                Mevcut hızınız <b>{speed} km/h</b> olarak görünüyor.<br/>
                <b>Adres:</b> {address}<br/>
                <b>Tarih:</b> {date}<br/><br/>
                <a href='https://basaranerp.com/'>basaranerp.com</a> tarafından otomatik gönderilmiştir.
            ";

                await _mobileService.SendMailAsync(new EMessageLogDto
                {
                    Subject = "Hız Limiti Aşımı",
                    Body = bodyMail,
                    Type = (int)MessageLogType.EMail,
                    Email = driver.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotifyDriver ERROR] {ex.Message}");
            }
        }

        private async Task InsertPlateCoordinate(List<ESpeedDto> list)
        {
            try
            {
                var vehicles = _vehicleRepository.Where(w => w.Status && w.ArventoNo != null).ToList();
                var entities = new List<VehicleCoordinate>();

                foreach (var g in list.GroupBy(g => g.DeviceNo))
                {
                    var vehicle = vehicles.FirstOrDefault(f => f.ArventoNo == g.Key);
                    if (vehicle == null) continue;

                    entities.AddRange(g.Where(w => w.GmtDateTime.HasValue)
                                       .Select(s => new VehicleCoordinate
                                       {
                                           VehicleId = vehicle.Id,
                                           Latitude = s.Latitude.ToString(),
                                           Longitude = s.Longitude.ToString(),
                                           Speed = s.Speed.ToString(),
                                           LocalDate = s.GmtDateTime.Value
                                       }));
                }

                if (entities.Count > 0)
                {
                    await _vehicleCoordinateRepository.InsertRangeAsync(entities.Distinct().OrderBy(e => e.LocalDate).ToList());
                    await _uow.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InsertPlateCoordinate ERROR] {ex.Message}");
            }
        }

        #endregion

        #region Arvento No Güncelleme
        public async Task Arvento2ErpNodeGuncelle()
        {
            try
            {
                if (await IsJobRun())
                    return;

                await JobSetTrueFalse("true");

                var date = DateTime.Now;
                var nodeList = await GetArventoPlateList();

                await JobSetTrueFalse("false");

                if (nodeList.Count == 1 && nodeList[0].Node == null)
                {
                    MailSender("Arvento Plaka Node Aktarımı Hk.", "Plakaya bağlı node bilgileri Arvento'dan çekilemedi. Method name: Arvento2ErpNodeGuncelle");
                    return;
                }

                var vehicleList = await _vehicleRepository.Where(w => w.Status).ToListAsync();
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
                await JobSetTrueFalse("false");
                MailSender("Arvento Plaka Node Aktarımı Hk.", "Plakaya bağlı node bilgileri veritabana güncellenirken hata oluştu");
            }
        }
        public async Task<List<EVehicleArventoDto>> GetArventoPlateList()
        {
            var list = new List<EVehicleArventoDto>();

            try
            {
                var url = $"{_rootUrl}GetIMSIList?Username={_userName}&PIN1={_password}&PIN2={_password}";

                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                    return list;

                var xmlString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(xmlString))
                    return list;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                var nodes = xmlDoc.SelectNodes("//NewDataSet/*");
                if (nodes == null || nodes.Count == 0)
                    return list;

                foreach (XmlNode node in nodes)
                {
                    var model = new EVehicleArventoDto
                    {
                        Node = node.SelectSingleNode("Node")?.InnerText?.Trim(),
                        Plate = node.SelectSingleNode("LicensePlate")?.InnerText?.Replace(" ", ""),
                        Driver = node.SelectSingleNode("Driver")?.InnerText,
                        IMSI = node.SelectSingleNode("IMSI")?.InnerText
                    };

                    if (!string.IsNullOrWhiteSpace(model.Node))
                        list.Add(model);
                }
            }
            catch (HttpRequestException ex)
            {
            }
            catch (XmlException ex)
            {
            }
            catch (Exception ex)
            {
            }

            return list;
        }
        #endregion

        #region Aracın koordinatlarını günceller
        public async Task ArventoPlakaKoordinatEkle()
        {
            try
            {
                var dateNow = DateTime.Now;
                if (dateNow.Hour == 17 || dateNow.Hour == 18 || dateNow.Hour == 19)//Mesai dışı çalışıyor 19:01'de
                    return;

                if (await IsJobRun())
                    return;

                await JobSetTrueFalse("true");

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
                            var coordinateList = await GeneralReport(startDate, endDate, item.ArventoNo);

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
                            await _vehicleCoordinateRepository.InsertRangeAsync(entities);
                            await _uow.SaveChangesAsync();

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
                await JobSetTrueFalse("false");
                MailSender("Arvento Koordinat Hk.", "Plakaya bağlı koordinat bilgileri veritabana eklenirken hata oluştu");
            }

            await JobSetTrueFalse("false");
        }

        #endregion

        #region Mail İşlemleri
        public async Task MailSender(string subject, string body)
        {
            try
            {
                var adminMailList = await _parameterRepository.FirstOrDefaultNoTrackingAsync(f => f.KeyP == ParameterEnum.AdminMailList.ToString());
                var mailList = adminMailList?.ValueP;
                if (!string.IsNullOrEmpty(mailList))
                    _mailService.SendMail(mailList, subject, body);
            }
            catch (Exception ex) { }
        }

        public async Task UpdateJob(TaskScheduler_ entity)
        {
            try
            {
                _jobRepository.Update(entity);
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
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

        public async Task<string> GetCityName(int cityId)
        {
            var result = await (from c1 in _cityRepository.GetAll()
                                join c2 in _cityRepository.GetAll() on c1.ParentId equals c2.Id
                                where c1.Id == cityId
                                select new ETripDto()
                                {
                                    StartCityName = c2.Name + "-" + c1.Name
                                }).FirstOrDefaultAsync();

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
