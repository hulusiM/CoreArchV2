using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Utilies.SessionOperations;
using CoreArchV2.Web.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using System.Text;

namespace CoreArchV2.Web.WebExtentions
{
    public static class UseCustomAppHandler
    {
        public static void UseCustomRoleManagement(this IApplicationBuilder app, IHttpContextAccessor accessor)
        {
            app.Use(async (context, next) =>
            {
                var workContext = accessor.HttpContext.Session.GetComplexData<SessionContext>("_sessionContext");
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                    if (controllerActionDescriptor != null)
                    {
                        var controllerName = controllerActionDescriptor.ControllerName;
                        var actionName = controllerActionDescriptor.ActionName;
                        var urlPath = accessor.HttpContext.Request.QueryString.ToString();
                        if (controllerName != "Login")
                        {
                            if (workContext == null)
                                context.Response.Redirect("/Login/Index?redirectUrl=/" + controllerName + "/" + actionName + urlPath);
                            else if (!workContext.GetAllAuthorizationList.Where(w => w.IsUnControlledAuthority == false).Any(a => a.Controller == controllerName && a.Action == actionName))
                            {
                                if (!AuthCheck(workContext.AuthMenuList, controllerName, actionName))
                                {
                                    var menu = workContext.GetAllAuthorizationList.FirstOrDefault(w => w.Controller == controllerName && w.Action == actionName);
                                    if (menu != null && menu.IsMenu) //is page
                                        context.Response.Redirect("/Home/NotAuthorityPage");
                                    else //is method
                                        await context.Response.WriteAsync("NotAuthorizationCore");
                                }
                                else
                                    await next();
                            }
                            else
                                await next();
                        }
                        else
                            await next();
                    }
                }
                else
                    context.Response.Redirect("/Home/NotFoundPage");
            });
        }
        public static bool AuthCheck(List<EAuthorizationDto> authMenuList, string controllerName, string actionName)
        {
            bool chckAuth = false;
            foreach (var item in authMenuList)
            {
                if (item.Controller == controllerName && item.Action == actionName)
                {
                    chckAuth = true;
                    break;
                }

                if (item.children.Count > 0)
                {
                    chckAuth = AuthCheck(item.children, controllerName, actionName);
                    if (chckAuth)
                        break;
                }
            }
            return chckAuth;
        }
    }

    public class NonWwwRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            var currentHost = req.Host;
            if (!currentHost.Host.StartsWith("www."))
            {
                var newHost = new HostString(currentHost.Host.Substring(4), currentHost.Port ?? 80);
                var newUrl = new StringBuilder().Append("https://www.").Append(req.PathBase).Append(req.Path).Append(req.QueryString);
                context.HttpContext.Response.Redirect(newUrl.ToString());
                context.Result = RuleResult.EndResponse;
            }
        }
    }
}
