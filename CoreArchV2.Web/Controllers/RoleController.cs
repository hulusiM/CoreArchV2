using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class RoleController : AdminController
    {
        private readonly IGenericRepository<Authorization> _authorizationGenericRepository;
        private readonly IMenuService _menuService;
        private readonly IGenericRepository<RoleAuthorization> _roleAuthorizationGenericRepository;
        private readonly IGenericRepository<Role> _roleGenericRepository;
        private readonly IRoleService _roleService;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<UserRole> _userRoleGenericRepository;

        public RoleController(IUnitOfWork uow,
            IRoleService roleService,
            IMenuService menuService)
        {
            _uow = uow;
            _roleGenericRepository = uow.GetRepository<Role>();
            _roleAuthorizationGenericRepository = uow.GetRepository<RoleAuthorization>();
            _authorizationGenericRepository = uow.GetRepository<Authorization>();
            _userRoleGenericRepository = uow.GetRepository<UserRole>();
            _roleService = roleService;
            _menuService = menuService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleGetAll(int? page, ERoleDto filterModel)
        {
            var result = _roleService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Role/RoleGetAll"));
            return Json(result);
        }

        public IActionResult GetAllMenuList()
        {
            var list = _menuService.GetAllAuthMenuList();
            return Json(list);
        }

        #region Static Methods

        public IActionResult Insert(ERoleAuthorizationDto model)
        {
            var checkRoleName = _roleGenericRepository.GetAll().Any(w => w.Name == model.RoleName);
            if (checkRoleName)
                return Json("Bu rol adı zaten var");

            model.CreatedBy = _workContext.User.Id;
            return Json(_roleService.Insert(model));
        }

        public IActionResult GetById(int Id)
        {
            var roleList = _roleService.RoleWithChildrenAuthorizationList(Id);
            if (roleList.Count > 0 && Id > 0)
                roleList[0].RoleName = _roleGenericRepository.Find(Id).Name;

            return Json(roleList);
        }

        [HttpPost]
        public IActionResult Update(ERoleAuthorizationDto model)
        {
            model.CreatedBy = _workContext.User.Id;
            if (model.AuthorizationIdList.Count > 0)
                return Json(_roleService.Update(model));
            return Json(false);
        }

        public IActionResult Delete(int Id)
        {
            try
            {
                return Json(_roleService.Delete(Id));
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE"))
                    return Json("Bu rol kullanıcıya atandığı için silinemez.");
                return Json(false);
            }
        }
    }

    #endregion Static Methods
}