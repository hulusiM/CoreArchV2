using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace CoreArchV2.Api.Extentions
{
    public static class AuthExtension
    {
        private readonly static string MyHollyOriginsPolicy = "_myHollyOrigins";

        public static void AddAuthenticationServices(this IServiceCollection services, string secretKey)
        {
            var _secretKey = Encoding.ASCII.GetBytes(secretKey);

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            services.AddCors(options =>
            {
                options.AddPolicy(MyHollyOriginsPolicy,
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_secretKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }
    }
}
