using CoreArchV2.Data;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.PO;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 📌 Servis kayıtları
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPoService, PoService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHangFireRunService, HangFireRunService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<IArventoService, ArventoService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.Configure<MailSetting>(options => configuration.GetSection("MailSetting").Bind(options));
builder.Services.Configure<ArventoSetting>(options => configuration.GetSection("ArventoSetting").Bind(options));
builder.Services.Configure<FirmSetting>(options => configuration.GetSection("FirmSetting").Bind(options));
builder.Services.Configure<POSetting>(options => configuration.GetSection("POSetting").Bind(options));

builder.Services.AddAutoMapper(typeof(Program));

// 📌 DbContext
builder.Services.AddDbContext<CoreArchDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DevConnection")));

// 📌 Hangfire yapılandırması
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(configuration.GetConnectionString("DevConnection"),
              new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.Zero,
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true
              });
});

builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromMilliseconds(3000);
});

var app = builder.Build();

// 📌 Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// 📌 Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// 📌 Ana sayfa → otomatik Hangfire Recurring sekmesine yönlendirme
app.MapGet("/", context =>
{
    context.Response.Redirect("/hangfire/recurring");
    return Task.CompletedTask;
});

// 📌 Swagger'ı tamamen kapatmak için (emin ol ki aşağıdaki satırlar yok):
// app.UseSwagger();
// app.UseSwaggerUI();


// 📌 Job servislerini scope içinden al
using (var scope = app.Services.CreateScope())
{
    var poService = scope.ServiceProvider.GetRequiredService<IPoService>();
    var generalService = scope.ServiceProvider.GetRequiredService<IHangFireRunService>();
    var arventoService = scope.ServiceProvider.GetRequiredService<IArventoService>();

    // PO Yakıt ekler
    RecurringJob.RemoveIfExists(nameof(poService.FuelInsert));
    RecurringJob.AddOrUpdate(nameof(poService.FuelInsert),
        () => poService.FuelInsert(),
        Cron.Hourly,
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

    // Açık görevler için kapatma bildirimi
    RecurringJob.RemoveIfExists(nameof(generalService.TripClosedControlAfterPushNotification));
    RecurringJob.AddOrUpdate(nameof(generalService.TripClosedControlAfterPushNotification),
        () => generalService.TripClosedControlAfterPushNotification(),
        "10 19 * * *",
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

    //--------------------------------------- Arvento Job'ları ---------------------------------------//

    // Mesai Dışı Kullanım
    RecurringJob.RemoveIfExists(nameof(arventoService.ArventoMesaiDisiKullanimRaporu));
    RecurringJob.AddOrUpdate(nameof(arventoService.ArventoMesaiDisiKullanimRaporu),
        () => arventoService.ArventoMesaiDisiKullanimRaporu(),
        "01 07 * * *",
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

    // Mesai İçi Kullanım
    RecurringJob.RemoveIfExists(nameof(arventoService.ArventoMesaiIciKullanimRaporu));
    RecurringJob.AddOrUpdate(nameof(arventoService.ArventoMesaiIciKullanimRaporu),
        () => arventoService.ArventoMesaiIciKullanimRaporu(),
        "01 19 * * *",
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

    // Arvento hız bildirimi
    RecurringJob.RemoveIfExists(nameof(arventoService.ArventoPlakaHiziSorgula));
    RecurringJob.AddOrUpdate(nameof(arventoService.ArventoPlakaHiziSorgula),
        () => arventoService.ArventoPlakaHiziSorgula(),
        "*/3 * * * * *",
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

    // Mesai İçi/Dışı Kullanım Raporu Mail
    RecurringJob.RemoveIfExists(nameof(arventoService.AracKullanimRaporuMailGonder));
    RecurringJob.AddOrUpdate(nameof(arventoService.AracKullanimRaporuMailGonder),
        () => arventoService.AracKullanimRaporuMailGonder(),
        "01 12 * * *",
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));
}

app.MapControllers();
app.Run();
