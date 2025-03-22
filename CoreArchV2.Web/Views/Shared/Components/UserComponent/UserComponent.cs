using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Views.Shared.Components.UserComponent
{
    public class UserComponent : ViewComponent
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userGenericRepo;

        public UserComponent(IUnitOfWork uow)
        {
            _uow = uow;
            _userGenericRepo = uow.GetRepository<User>();
        }

        //@await Component.InvokeAsync("UserComponent")
        //public string Invoke()
        //{
        //    return "Hulusi";
        //}

        //@await Component.InvokeAsync("UserComponent")
        //public IViewComponentResult Invoke(User model)
        //{
        //    return new HtmlContentViewComponentResult(new HtmlString("<b>Hulusi</b>"));
        //}

        //@await Component.InvokeAsync("UserComponent",5)
        //public IViewComponentResult Invoke(int id)
        //{
        //    EUserDto model = new EUserDto();
        //    model.Name = "Hulusi";
        //    return View(model);
        //}

        //@await Component.InvokeAsync("UserComponent",new { isAproved=true,id=44 })
        //public IViewComponentResult Invoke(bool isAproved,int id)
        //{
        //    return View();
        //}

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}