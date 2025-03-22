using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    public class MenuService : IMenuService
    {
        private readonly IGenericRepository<Authorization> _authorizationRepository;
        private readonly IGenericRepository<RoleAuthorization> _roleAuthorizationRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;

        public MenuService(IUnitOfWork uow)
        {
            _uow = uow;
            _userRepository = uow.GetRepository<User>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _roleRepository = uow.GetRepository<Role>();
            _roleAuthorizationRepository = uow.GetRepository<RoleAuthorization>();
            _authorizationRepository = uow.GetRepository<Authorization>();
        }

        // Yetkili olduğu menüleri subChild olarak listeler
        public List<EAuthorizationDto> AuthMenuList(int userId)
        {
            var result = (from u in _userRoleRepository.GetAll()
                          join ra in _roleAuthorizationRepository.GetAll() on u.RoleId equals ra.RoleId
                          join a in _authorizationRepository.GetAll() on ra.AuthorizationId equals a.Id
                          where u.UserId == userId && a.Status
                          select new EAuthorizationDto
                          {
                              id = a.Id,
                              UserId = u.UserId,
                              ParentId = a.ParentId,
                              Name = a.Name,
                              Controller = a.Controller.Replace(" ", ""),
                              Action = a.Action.Replace(" ", ""),
                              DisplayOrder = a.DisplayOrder,
                              icon = a.Icon,
                              RoleId = ra.RoleId,
                              IsMenu = a.IsMenu,
                              Attribute = a.Attribute,
                              IsUnControlledAuthority = a.IsUncontrolledAuthority
                          }).Distinct().OrderBy(a => a.DisplayOrder).ToList();

            var list = result.Where(w => w.ParentId == null).Select(s => new EAuthorizationDto
            {
                id = s.id,
                ParentId = s.ParentId,
                Name = s.Name,
                Controller = s.Controller,
                Action = s.Action,
                DisplayOrder = s.DisplayOrder,
                icon = s.icon,
                RoleId = s.RoleId,
                IsMenu = s.IsMenu,
                Attribute = s.Attribute,
                IsUnControlledAuthority = s.IsUnControlledAuthority,
                children = AddChildMenu(result, s.id)
            }).ToList();

            return list;
        }

        // Yetkisiz Menüler içinde arama yapar
        public List<EAuthorizationDto> GetAllAuthorization()
        {
            var list = (from a in _authorizationRepository.GetAll()
                        where a.Status
                        select new EAuthorizationDto
                        {
                            id = a.Id,
                            Status = a.Status,
                            CreatedBy = a.CreatedBy,
                            CreatedDate = a.CreatedDate,
                            ParentId = a.ParentId,
                            Name = a.Name,
                            Controller = a.Controller,
                            Action = a.Action,
                            icon = a.Icon,
                            IsMenu = a.IsMenu,
                            IsUnControlledAuthority = a.IsUncontrolledAuthority,
                            DisplayOrder = a.DisplayOrder
                        }).ToList();
            return list;
        }

        public List<EAuthorizationDto> GetAllAuthMenuList()
        {
            var result = (from m in _authorizationRepository.GetAll()
                          join a in _roleAuthorizationRepository.GetAll() on m.Id equals a.AuthorizationId into aL
                          from a in aL.DefaultIfEmpty()
                          join u in _userRoleRepository.GetAll() on a.RoleId equals u.RoleId into uL
                          from u in uL.DefaultIfEmpty()
                          where m.IsUncontrolledAuthority && m.Status
                          select new EAuthorizationDto
                          {
                              id = m.Id,
                              ParentId = m.ParentId,
                              Name = m.Name,
                              text = m.Name,
                              icon = m.Icon,
                              //id = m.Id,
                              Controller = m.Controller,
                              Action = m.Action,
                              DisplayOrder = m.DisplayOrder,
                              IsMenu = m.IsMenu,
                              IsUnControlledAuthority = m.IsUncontrolledAuthority
                          }).Distinct().OrderBy(a => a.DisplayOrder).ToList();

            var list = result.Where(w => w.ParentId == null).Select(s => new EAuthorizationDto
            {
                id = s.id,
                text = s.Name,
                ParentId = s.ParentId,
                Name = s.Name,
                Controller = s.Controller,
                Action = s.Action,
                DisplayOrder = s.DisplayOrder,
                icon = s.icon,
                IsMenu = s.IsMenu,
                IsUnControlledAuthority = s.IsUnControlledAuthority,
                children = AddChildMenu(result, s.id)
            }).ToList();

            return list;
        }

        private List<EAuthorizationDto> AddChildMenu(List<EAuthorizationDto> model, int parentId)
        {
            var newList = new List<EAuthorizationDto>();
            var childList = model.Where(o => o.ParentId == parentId).ToList();

            foreach (var item in childList)
            {
                //var entity = Mapper.DynamicMap<EAuthorizationDto>(item);
                var child = AddChildMenu(model, item.id);
                item.children = child;
                newList.Add(item);
            }

            return newList;
        }


        //Kullanıcıya bağlı anasayfayı getir
        public string GetUserIdWithHomePage(int userId)
        {
            var homePageUrl = "";
            var result = (from ur in _userRoleRepository.GetAll()
                          join a in _authorizationRepository.GetAll() on ur.HomePageId equals a.Id
                          where ur.UserId == userId && a.Status
                          select new EAuthorizationDto()
                          {
                              Name = "/" + a.Controller + "/" + a.Action
                          }).FirstOrDefault();

            if (result != null)
                homePageUrl = result.Name;
            return homePageUrl;
        }

        public List<EComboboxDto> GetRoleWithHomePageList(int roleId)
        {
            var list = (from a in _authorizationRepository.GetAll()
                        join ra in _roleAuthorizationRepository.GetAll() on a.Id equals ra.AuthorizationId
                        //join ur in _userRoleRepository.GetAll() on ra.RoleId equals ur.RoleId
                        where ra.RoleId == roleId && a.Status && a.IsMenu &&
                              (a.Controller != null && a.Controller != "") &&
                              (a.Action != null && a.Action != "")
                        select new EComboboxDto()
                        {
                            Id = a.Id,
                            Name = a.Name,
                        }).Distinct().ToList();

            return list;
        }

        //Sistemde yetkili kullanıcıları listeler
        public List<User> GetAuthUserList()
        {
            var list = (from u in _userRepository.GetAll()
                        join ur in _userRoleRepository.GetAll() on u.Id equals ur.UserId
                        where u.Status
                        select new User()
                        {
                            Email = u.Email
                        }).ToList();
            return list;
        }
    }
}