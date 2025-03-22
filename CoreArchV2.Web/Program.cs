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

services.AddControllersWithViews();
services.AddHttpContextAccessor();
services.AddSignalR();
services.AddHostedService<VehicleTrackingService>();
services.AddAutoMapper(typeof(MapperProfileWeb));
services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(360); // AJAX tarafında da güncellenmeli
    options.Cookie.HttpOnly = true;
});

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
services.AddScoped<IArventoService, ArventoService>();
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
});// JSON Ayarları (Varsayılan camelCase yerine PascalCase)

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Statik Dosya Servisi
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
});

app.UseSession();
app.UseRouting();
app.UseStatusCodePages();
//app.UseMiddleware<SecurityHeadersMiddleware>();//header güvenlik middleware


app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<VehicleMapHub>("/VehicleMapHub");
    endpoints.MapHub<SignalRHub>("/SignalRHub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Login}/{action=Index}");
});

// Özel Middleware (Role Yönetimi)
var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
app.UseCustomRoleManagement(accessor);

app.UseAuthorization();
app.UseHttpsRedirection();
app.Run();
