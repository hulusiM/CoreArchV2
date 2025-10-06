using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace CoreArchV2.Services.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<RoleAuthorization> _roleAuthorizationRepository;
        private readonly IGenericRepository<ActiveUserForSignalR> _activeUserRepository;
        public MessageService(IUnitOfWork uow,
            IHubContext<SignalRHub> hubContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _uow = uow;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            _roleRepository = uow.GetRepository<Role>();
            _messageRepository = uow.GetRepository<Message>();
            _userRepository = uow.GetRepository<User>();
            _userRoleRepository = uow.GetRepository<UserRole>();
            _activeUserRepository = uow.GetRepository<ActiveUserForSignalR>();
            _roleAuthorizationRepository = uow.GetRepository<RoleAuthorization>();
        }

        public bool IsAny(string desc) => _messageRepository.Any(a => a.Status == true && a.Description == desc);

        public EResultDto FuelMessageInsert(Message model, bool sendPn = false)
        {
            var result = new EResultDto();
            try
            {
                if (!IsAny(model.Description))
                {
                    //var allUserList = _userRoleRepository.GetAll().ToList();
                    var allUserList = (from r in _roleRepository.GetAll()
                                       join ra in _roleAuthorizationRepository.GetAll() on r.Id equals ra.RoleId
                                       join ur in _userRoleRepository.GetAll() on r.Id equals ur.RoleId
                                       join u in _userRepository.GetAll() on ur.UserId equals u.Id
                                       where u.Status && ra.AuthorizationId == (int)AuthorizationId.FuelReportPage
                                       select new EUserDto()
                                       {
                                           Id = ur.UserId
                                       }).Distinct().ToList();

                    foreach (var item in allUserList)
                    {
                        model.UserId = item.Id;
                        model.Id = 0;
                        _messageRepository.Insert(model);

                        var connection = _activeUserRepository.FirstOrDefault(f => f.Status && f.UserId == item.Id);
                        if (sendPn && connection != null)
                            _hubContext.Clients.Client(connection.ConnectionId).SendAsync("receivePnMessage", "info", "Yeni Bildirim", model.Description);
                    }
                    _uow.SaveChanges();
                }
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }

        //gönderilen toplu mesajları yakıtlar pasife alınınca siler
        public void DeleteRange(EFuelLogDto model)
        {
            try
            {
                var messages = _messageRepository.Where(w => w.Description == model.Description).ToList();
                if (messages.Count > 0)
                {
                    _messageRepository.DeleteRange(messages);
                    _uow.SaveChanges();
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
