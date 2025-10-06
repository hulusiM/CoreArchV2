using AutoMapper;
using CoreArchV2.Api.Helper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Enum;
using CoreArchV2.Dto.EApiDto;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CoreArchV2.Api.Controllers
{    // [Route("api/[controller]/[action]")] --> Best practices açısından böyle olması doğru değil,metod isimleri dış dünyaya açılmamalı
    [Produces("application/json")]
    [Route("User")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMenuService _menuService;
        private readonly IMobileService _mobileService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService,
            IMenuService menuService,
            IMobileService mobileService,
            IMapper mapper)
        {
            _userService = userService;
            _menuService = menuService;
            _mobileService = mobileService;
            _mapper = mapper;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AUserLoginDto model)
        {
            try
            {
                var user = _userService.FindByUsernameAndPass(new EUserDto { MobilePhone = model.MobilePhone, Password = model.Password });
                if (user == null)
                    return new ObjectActionResult(success: false, statusCode: HttpStatusCode.BadRequest, data: "Kullanıcı bulunamadı");

                if (user != null && user.ParentUnitId == null && user.UnitId == null)
                    return new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Herhangi bir birime/projeye yetkiniz bulunmamaktadır. Yöneticiyle iletişime geçiniz.");

                var authMenuList = await Task.FromResult(_menuService.AuthMenuList(user.Id));
                if (authMenuList.Any())
                {
                    bool isManager = false;
                    foreach (var item in authMenuList)
                    {
                        if (item.id == (int)AuthorizationId.TripAuthorization)
                        {
                            isManager = true;
                            break;
                        }
                        else if (item.children.Any(x => x.id == (int)AuthorizationId.TripAuthorization))
                        {
                            isManager = true;
                            break;
                        }
                    }
                    user.IsManager = isManager;
                    return new ObjectActionResult(
                    success: true,
                    statusCode: HttpStatusCode.OK,
                    data: user);
                }
                else
                    return new ObjectActionResult(success: false, statusCode: HttpStatusCode.OK, data: "Herhangi bir menüye yetkiniz bulunmamaktadır. Yöneticiyle iletişime geçiniz.");
            }
            catch (Exception ex)
            {
                throw new Exception("User liste çekerken hata oldu");//api tarafına bu hata fırlatılır
            }
        }

        [HttpPost("InsertDevice")]
        public async Task<IActionResult> InsertDevice(Device entity)
        {
            var result = await _userService.InsertDeviceAync(entity);
            if (result.IsSuccess)
            {
                return new NoContentResult();
            }
            else
            {
                return Json(HttpStatusCode.NotFound);
            }
        }

        [HttpGet("GetMobileLastVersion")]
        public async Task<IActionResult> GetMobileLastVersion()
        {
            try
            {
                var version = await _mobileService.GetMobileLastVersion();
                return new ObjectActionResult(
                      success: true,
                      statusCode: HttpStatusCode.OK,
                      data: version);
            }
            catch (Exception)
            {
                return new ObjectActionResult(
                                   success: false,
                                   statusCode: HttpStatusCode.BadRequest,
                                   data: null);
            }
        }

        [HttpGet("GetNotificationList")]
        public async Task<IActionResult> GetNotificationList(int page, int userId)
        {
            try
            {
                var filterModel = new EMessageLogDto()
                {
                    UserId = userId,
                    Type = (int)MessageLogType.PushNotification
                };
                var list = await Task.FromResult(_mobileService.GetAllWithPaged(page, filterModel));
                return new ObjectActionResult(
                      success: true,
                      statusCode: HttpStatusCode.OK,
                      data: list);
            }
            catch (Exception)
            {
                return new ObjectActionResult(
                                   success: false,
                                   statusCode: HttpStatusCode.BadRequest,
                                   data: null);
            }
        }

        [HttpPost("InsertPhotoByVehicle")]
        public async Task<IActionResult> InsertPhotoByVehicle(EVehiclePhysicalImageDto model)
        {
            try
            {
                var result = await Task.FromResult(_mobileService.InsertVehiclePhysicalImageBase64(model));
                return new ObjectActionResult(
                      success: true,
                      statusCode: HttpStatusCode.OK,
                      data: result.Message);
            }
            catch (Exception)
            {
                return new ObjectActionResult(
                                   success: false,
                                   statusCode: HttpStatusCode.BadRequest,
                                   data: null);
            }
        }


        //[HttpPut("update")]
        //public IActionResult Update(EUserDto model)
        //{
        //    if (model.Id <= 0)
        //        throw new Exception("id alanı gereklidir");//best practices olarak uygun değil

        //    //_userService.Update(model);
        //    return new ObjectActionResult(
        //           success: true,
        //           statusCode: HttpStatusCode.NoContent,//veri trafiği açısından nocontent durum kodu dönmesi lazım.
        //           data: "");
        //}
    }
}
