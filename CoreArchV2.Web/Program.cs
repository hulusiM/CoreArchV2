using CoreArchV2.Data;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using CoreArchV2.Services.SignalR;
using CoreArchV2.Utilies;
using CoreArchV2.Web.MapProfileWeb;
using CoreArchV2.Web.WebExtentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// MVC, Session, SignalR
services.AddControllersWithViews();
services.AddSignalR();
services.AddHttpContextAccessor();
services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(360);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Hosted Services
services.AddHostedService<CoreArchV2.Services.SignalR.ArventoMapService>();
services.AddHostedService<CoreArchV2.Services.SignalR.BasaranMapService>();

// AutoMapper
services.AddAutoMapper(typeof(MapperProfileWeb));

#region Scoped Servisleri Bağlama
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IMenuService, MenuService>();
services.AddScoped<IRoleService, RoleService>();
services.AddScoped<IVehicleService, VehicleService>();
services.AddScoped<IUserRoleService, UserRoleService>();
services.AddScoped<IMaintenanceService, MaintenanceService>();
services.AddScoped<IFileService, FileService>();
services.AddScoped<IFuelLogService, FuelLogService>();
services.AddScoped<ICriminalLogService, CriminalLogService>();
services.AddScoped<ITireLogService, TireLogService>();
services.AddScoped<IVehicleRequestService, VehicleRequestService>();
services.AddScoped<IReportService, ReportService>();
services.AddScoped<ITenderService, TenderService>();
services.AddScoped<IMessageService, MessageService>();
services.AddScoped<IUtilityService, UtilityService>();
services.AddScoped<IBrandService, BrandService>();
services.AddScoped<INoticeService, NoticeService>();
services.AddScoped<ITripService, TripService>();
services.AddScoped<IMobileService, MobileService>();
services.AddScoped<IArventoService, CoreArchV2.Services.Arvento.ArventoService>();
services.AddScoped<ICacheService, CacheService>();
services.AddScoped<IOutOfHourService, OutOfHourService>();
services.AddScoped<IMailService, MailService>();
services.AddScoped<ILicenceWebService, LicenceWebService>();
services.AddScoped<IVehicleMapService, VehicleMapService>();
#endregion

#region Veritabanı Bağlantıları
services.AddDbContext<CoreArchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));
services.AddDbContext<LicenceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LicenceDbConnection")));
#endregion

#region Konfigürasyon Ayarlarını Bağlama
services.Configure<MailSetting>(builder.Configuration.GetSection("MailSetting"));
services.Configure<ArventoSetting>(builder.Configuration.GetSection("ArventoSetting"));
services.Configure<FirmSetting>(builder.Configuration.GetSection("FirmSetting"));
services.Configure<POSetting>(builder.Configuration.GetSection("POSetting"));
services.Configure<WebSendPushNotification>(builder.Configuration.GetSection("WebSendPushNotification"));
services.Configure<LicenceSetting>(builder.Configuration.GetSection("LicenceSetting"));
#endregion

services.AddControllers().AddJsonOptions(jsonOptions =>
{
    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// ✅ Middleware sıralaması çok önemli
if (!app.Environment.IsDevelopment())
    app.UseHsts();
else
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
});

app.UseRouting();

// Authentication ve Authorization middleware’leri
app.UseAuthentication();
app.UseAuthorization();

// Session middleware’ini endpointlerden önce aktif et
app.UseSession();

// Özel middleware (Session aktif olduktan sonra)
var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
app.UseCustomRoleManagement(accessor);

// En sonda endpoint mapping
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ArventoMapHub>("/ArventoMapHub");
    endpoints.MapHub<BasaranVehicleMapHub>("/BasaranVehicleMapHub");
    endpoints.MapHub<SignalRHub>("/SignalRHub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Login}/{action=Index}/{id?}");
});

app.Run();
