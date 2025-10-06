using AutoMapper;
using CoreArchV2.Api.MapProfileApi;
using Microsoft.OpenApi.Models;

namespace CoreArchV2.Api.ApiExtentions
{
    public static class UseCustomServicesHandler
    {
        public static void UseCustomSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreArch.Api", Version = "v1" });
            });
        }
        public static void UseCustomAutoMapper(this IServiceCollection services)
        {
            // Mapper konfigürasyonu oluştur
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfileApi());
            });

            // Mapper instance’ını oluştur ve DI container’a ekle
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // .NET 8 uyumlu — Assembly bazlı ekleme
            services.AddAutoMapper(typeof(Program).Assembly);
        }
    }
}
