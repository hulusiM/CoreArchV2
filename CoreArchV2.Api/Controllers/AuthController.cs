using AutoMapper;
using CoreArchV2.Api.Helper;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace CoreArchV2.Api.Controllers
{
    [Produces("application/json")]
    [Route("Auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly MobilePushNotificationToken _pnSetting;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService,
            IConfiguration configuration,
            IOptions<MobilePushNotificationToken> pnSetting,
            IMapper mapper)
        {
            _authService = authService;
            _pnSetting = pnSetting.Value;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpGet("GetToken")]
        public IActionResult GetToken(string username, string password)
        {
            //var user = _mapper.Map<User>(userLoginDto);
            //user = _authService.Login(user);

            //if (user == null)
            //    return new ObjectActionResult(
            //       success: false,
            //       statusCode: HttpStatusCode.BadRequest,
            //       data: null);

            if (_pnSetting.UserName.Equals(username) && _pnSetting.Password.Equals(password))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_pnSetting.SecretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.NameIdentifier,password),
                    new Claim(ClaimTypes.Name,username)
                    //new Claim(ClaimTypes.Role,user.Role)
                }),
                    Expires = DateTime.Now.AddYears(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenValue = tokenHandler.WriteToken(token);

                return new ObjectActionResult(
                    success: true,
                    statusCode: HttpStatusCode.OK,
                    data: tokenValue);
            }
            else
            {
                return new ObjectActionResult(
                    success: false,
                    statusCode: HttpStatusCode.BadRequest,
                    data: null);
            }
        }

    }
}
