using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Utilies.SessionOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreArchV2.Web.Controllers
{
    public abstract class AdminController : Controller
    {
        public EUserDto _loginUserInfo;
        public SessionContext _workContext;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _workContext = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext");
            if (_workContext != null)
                _loginUserInfo = _workContext.User;
        }
    }
}