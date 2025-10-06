using CoreArchV2.Api.ApiExtentions;
using CoreArchV2.Api.Extentions;
using CoreArchV2.Api.Filters;
using CoreArchV2.Api.Licence.Services;
using CoreArchV2.Data;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Data.UnitOfWorkLicence;
using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<CoreArchDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DevConnection")));
builder.Services.AddDbContext<LicenceDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("LicenceDbConnection")));
builder.Services.Configure<MobilePushNotificationToken>(options => configuration.GetSection("MobilePushNotificationToken").Bind(options));

builder.Services.UseCustomAutoMapper();
builder.Services.AddAuthorization();
var secretKey = configuration.GetSection("AppSettings:SecretKey").Value;
builder.Services.AddAuthenticationServices(secretKey);


builder.Services.AddScoped<IArventoService, ArventoService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ILicenceService, LicenceService>();
builder.Services.AddScoped<IUowLicence, UowLicence>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<NotFoundFilter>();

builder.Services.AddControllers(o =>
{
    o.Filters.Add(new ValidationFilter());
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoreArchV2.Api",
        Version = "v1"
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
var MyHollyOrigins = "_myHollyOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyHollyOrigins, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCustomApiErrorExtention();
app.UseHttpsRedirection();

#region Api'de alınan hatayı apk güncellemeden görmek için Log Middleware
object _fileLock = new object();
app.Use(async (context, next) =>
{
    var logFilePath = Path.Combine(AppContext.BaseDirectory, "log2.txt");

    try
    {
        context.Request.EnableBuffering();

        string bodyText = "";
        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
        {
            bodyText = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var reqLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] REQUEST {context.Request.Method} {context.Request.Path}\nBody: {bodyText}\n";

        lock (_fileLock)
        {
            using var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(reqLog);
        }

        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await next();

        memStream.Position = 0;
        string responseBody = await new StreamReader(memStream).ReadToEndAsync();
        memStream.Position = 0;

        var resLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RESPONSE {context.Response.StatusCode}\nBody: {responseBody}\n";

        lock (_fileLock)
        {
            using var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(resLog);
        }

        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
    catch (Exception ex)
    {
        lock (_fileLock)
        {
            using var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream);
            writer.WriteLine($"[ERROR {DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}");
        }
        throw;
    }
});
#endregion

app.UseRouting();
app.UseCors(MyHollyOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //c.RoutePrefix = "";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreArchV2.Api V1");
});
app.MapControllers();
app.Run();
