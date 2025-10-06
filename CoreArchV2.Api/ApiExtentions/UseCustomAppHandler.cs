using CoreArchV2.Dto.EApiDto;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace CoreArchV2.Api.ApiExtentions
{
    public static class UseCustomAppHandler
    {
        public static void UseCustomApiErrorExtention(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(config =>
            {
                config.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var ex = error.Error;
                        AErrorDto errorDto = new AErrorDto();
                        errorDto.Status = 500;
                        errorDto.Errors.Add(ex.Message);
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(errorDto));
                    }
                });
            });
        }
    }
}
