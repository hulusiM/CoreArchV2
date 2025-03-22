using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using CoreArchV2.Utilies;
using CoreArchV2.Utilies.SessionOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoreArchV2.Web.Controllers
{
    public class UserRoleController : AdminController
    {
        private readonly FirmSetting _firmSetting;
        private readonly IGenericRepository<Authorization> _authorizationGenericRepository;
        private readonly IMapper _mapper;
        private readonly IMenuService _menuService;
        private readonly IGenericRepository<RoleAuthorization> _roleAuthorizationGenericRepository;
        private readonly IGenericRepository<Role> _roleGenericRepository;
        private readonly IRoleService _roleService;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userGenericRepository;
        private readonly IGenericRepository<UserRole> _userRoleGenericRepository;
        private readonly IUserRoleService _userRoleService;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IMailService _mailService;

        public UserRoleController(IUnitOfWork uow,
            IRoleService roleService,
            IUserRoleService userRoleService,
            IMenuService menuService,
            IMailService mailService,
            IOptions<FirmSetting> firmSetting,
            IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _firmSetting = firmSetting.Value;
            _mailService = mailService;
            _roleGenericRepository = uow.GetRepository<Role>();
            _roleAuthorizationGenericRepository = uow.GetRepository<RoleAuthorization>();
            _authorizationGenericRepository = uow.GetRepository<Authorization>();
            _userRoleGenericRepository = uow.GetRepository<UserRole>();
            _userGenericRepository = uow.GetRepository<User>();
            _roleService = roleService;
            _menuService = menuService;
            _userRoleService = userRoleService;
            _unitRepository = uow.GetRepository<Unit>();
        }

        public IActionResult Index()
        {
            //ViewBag.ComboRoleList = new SelectList(_roleGenericRepository.GetAll()
            //  .Select(s => new { Id = s.Id, Name = s.Name })
            //  .ToList(), "Id", "Name");

            return View();
        }

        public IActionResult UserRoleGetAll(int? page, EUserRoleDto filterModel)
        {
            var result = _userRoleService.GetAllWithPaged(page, filterModel);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/UserRole/UserRoleGetAll"));
            return Json(result);
        }


        public IActionResult GetRoleWithHomePageList(int roleId) => Json(_menuService.GetRoleWithHomePageList(roleId));

        #region Static Methods
        public async Task<IActionResult> Insert(EUserRoleDto model)
        {
            model.CreatedBy = HttpContext.Session.GetComplexData<SessionContext>("_sessionContext").User.Id;
            return Json(await _userRoleService.Insert(model));
        }

        public IActionResult GetById(int Id) => Json(_userRoleGenericRepository.Find(Id));

        public IActionResult Update(EUserRoleDto model)
        {
            return Json(_userRoleService.Update(model));
        }

        public IActionResult Delete(int id, int deletedBy)
        {
            return Json(_userRoleService.Delete(id, deletedBy));
        }

        #endregion Static Methods
    }
}