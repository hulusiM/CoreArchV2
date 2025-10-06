using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using CoreArchV2.Utilies.SessionOperations;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class RoleService : IRoleService
    {
        private readonly IGenericRepository<Authorization> _authorizationRepository;
        private readonly IGenericRepository<RoleAuthorization> _roleAuthorizationRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;

        public RoleService(IUnitOfWork uow)
        {
            _uow = uow;
            _roleRepository = uow.GetRepository<Role>();
            _userRepository = uow.GetRepository<User>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _authorizationRepository = uow.GetRepository<Authorization>();
            _roleAuthorizationRepository = uow.GetRepository<RoleAuthorization>();
        }

        public SessionContext _workContext
        {
            get => new SessionContext();
            set => _roleRepository._workContext = value;
        }

        public PagedList<ERoleDto> GetAllWithPaged(int? page, ERoleDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = from r in _roleRepository.GetAll()
                       select new ERoleDto
                       {
                           Id = r.Id,
                           Status = r.Status,
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           CreatedDate = r.CreatedDate,
                           CreatedBy = r.CreatedBy,
                           CreatedIp = r.CreatedIp,
                           Name = r.Name,
                           CustomButton = "<li class='text-primary-400'><a data-toggle='modal' onclick='funcEditRole(" + r.Id +
                                          ");'><i class='icon-pencil5'></i></a></li>" +
                                          "<li class='text-danger-400'><a onclick='funcDeleteModal(" + r.Id +
                                          ")'><i class='icon-trash-alt'></i></a></li>"
                       };

            if (filterModel.Id > 0)
                list = list.Where(w => w.Id == filterModel.Id);

            var lastResult =
                new PagedList<ERoleDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }

        public List<EAuthorizationDto> RoleWithChildrenAuthorizationList(int roleId)
        {
            var getRoleList = GetAuthListForRoleId(roleId);
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
                              Controller = m.Controller,
                              Action = m.Action,
                              DisplayOrder = m.DisplayOrder,
                              icon = m.Icon,
                              IsMenu = m.IsMenu,
                              IsUnControlledAuthority = m.IsUncontrolledAuthority
                          }).Distinct().OrderBy(a => a.DisplayOrder).ToList();

            foreach (var t in result)
                t.state = new EStateDto
                {
                    selected = getRoleList.Where(w => w.AuthorizationId == t.id).FirstOrDefault() == null
                        ? false
                        : true
                };

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

            var stateControlList = StateCheck(list, getRoleList);
            return stateControlList;
        }

        public EResultDto Delete(int id)
        {
            var result = new EResultDto();
            try
            {
                var isUserRole = _userRoleRepository.Any(a => a.RoleId == id);
                if (isUserRole)
                {
                    result.IsSuccess = false;
                    result.Message = "Role üzerinde kullanıcı bulunuyor";
                }
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var roleAuthList = _roleAuthorizationRepository.GetAll().Where(w => w.RoleId == id).ToList();
                        foreach (var item in roleAuthList)
                            DeleteRoleAuthorization(item.Id);
                        _uow.SaveChanges(); //silinirse conflict hatası alınır

                        var model = _roleRepository.Find(id);
                        _roleRepository.Delete(model);
                        _uow.SaveChanges();

                        scope.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }

        public bool DeleteRoleAuthorization(int id)
        {
            try
            {
                var model = _roleAuthorizationRepository.Find(id);
                _roleAuthorizationRepository.Delete(model);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Insert(ERoleAuthorizationDto model)
        {
            try
            {
                var role = new Role
                {
                    Status = Convert.ToBoolean(Status.Active),
                    CreatedDate = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                    Name = model.RoleName
                };
                _roleRepository.Insert(role);
                _uow.SaveChanges();

                //Menu parent Add
                var newAuthorizationlist = new List<int>();
                foreach (var item in model.AuthorizationIdList)
                {
                    var listParent = RecursiveParentNodeFind(item);
                    newAuthorizationlist.AddRange(listParent);
                }

                newAuthorizationlist = newAuthorizationlist.Distinct().ToList();
                //-----------------------------------------------------//

                foreach (var item in newAuthorizationlist)
                {
                    var roleAuthorization = new RoleAuthorization
                    {
                        Status = Convert.ToBoolean(Status.Active),
                        CreatedDate = DateTime.Now,
                        CreatedBy = model.CreatedBy,
                        CreatedIp = "1",
                        RoleId = role.Id,
                        AuthorizationId = item
                    };
                    _roleAuthorizationRepository.Insert(roleAuthorization);
                }

                _uow.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool Update(ERoleAuthorizationDto model)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (UpdateRoleWithTransactionScope(model))
                    scope.Complete();
            }

            return true;
        }

        private List<EAuthorizationDto> StateCheck(List<EAuthorizationDto> allAuth,
            List<ERoleAuthorizationDto> roleList)
        {
            foreach (var t in allAuth)
                if (t.children.Count == 0)
                {
                    t.state = new EStateDto
                    {
                        selected = roleList.Where(w => w.AuthorizationId == t.id).FirstOrDefault() == null
                            ? false
                            : true
                    };
                }
                else
                {
                    t.state = new EStateDto
                    {
                        selected = false
                    };
                    StateCheck(t.children, roleList);
                }

            return allAuth;
        }

        private List<EAuthorizationDto> AddChildMenu(List<EAuthorizationDto> model, int parentId)
        {
            var newList = new List<EAuthorizationDto>();
            var childList = model.Where(o => o.ParentId == parentId).ToList();

            foreach (var item in childList)
            {
                var child = AddChildMenu(model, item.id);
                item.children = child;
                newList.Add(item);
            }

            return newList;
        }

        private List<ERoleAuthorizationDto> GetAuthListForRoleId(int roleId)
        {
            var list = (from ra in _roleAuthorizationRepository.GetAll()
                        join a in _authorizationRepository.GetAll() on ra.AuthorizationId equals a.Id
                        where ra.RoleId == roleId && a.Status
                        select new ERoleAuthorizationDto
                        {
                            RoleId = ra.RoleId,
                            AuthorizationId = ra.AuthorizationId
                        }).ToList();
            return list;
        }

        private List<int> RecursiveParentNodeFind(int authorizationId)
        {
            var arr = new List<int>();
            var authorization = _authorizationRepository.Find(authorizationId);
            if (authorization != null)
            {
                arr.Add(authorization.Id);
                if (authorization.ParentId > 0)
                {
                    var mod = RecursiveParentNodeFind(authorization.ParentId.Value);
                    arr.AddRange(mod);
                }
            }

            return arr;
        }

        public bool UpdateRoleWithTransactionScope(ERoleAuthorizationDto model)
        {
            try
            {
                // Tüm rolleri silip tekrar insert ediyoruz
                var roleAuthList = _roleAuthorizationRepository.GetAll().Where(w => w.RoleId == model.Id).ToList();
                foreach (var item in roleAuthList)
                    DeleteRoleAuthorization(item.Id);

                //kayıt edilen menünün kırılımları bulunup insert ediliyor
                var newAuthorizationlist = new List<int>();
                foreach (var item in model.AuthorizationIdList)
                {
                    var listParent = RecursiveParentNodeFind(item);
                    newAuthorizationlist.AddRange(listParent);
                }

                newAuthorizationlist = newAuthorizationlist.Distinct().ToList();

                //RoleAuthorization table insert
                foreach (var item in newAuthorizationlist)
                {
                    var roleAuthorization = new RoleAuthorization
                    {
                        Status = Convert.ToBoolean(Status.Active),
                        CreatedBy = model.CreatedBy,
                        RoleId = model.Id,
                        AuthorizationId = item
                    };
                    _roleAuthorizationRepository.Insert(roleAuthorization);
                }

                //Role table update
                var roleEntity = _roleRepository.Find(model.Id);
                roleEntity.Name = model.RoleName;
                _roleRepository.Update(roleEntity);
                _uow.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}