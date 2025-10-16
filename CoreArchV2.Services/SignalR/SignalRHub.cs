using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CoreArchV2.Services.SignalR
{
    public class SignalRHub : Hub
    {
        private static ConcurrentDictionary<string, bool> ConnectedClients = new ConcurrentDictionary<string, bool>();
        public override async Task OnConnectedAsync()
        {
            ConnectedClients.TryAdd(Context.ConnectionId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedClients.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }
        public static bool HasConnectedClients()
        {
            return ConnectedClients.Count > 0;
        }

        //private readonly IUnitOfWork _uow;
        //private readonly IGenericRepository<User> _userRepository;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IGenericRepository<Message> _messageRepository;
        //private readonly IGenericRepository<ChatMessage> _chatMessageRepository;
        //private readonly IGenericRepository<ActiveUserForSignalR> _activeUserRepository;

        //public SignalRHub(IUnitOfWork uow,
        //    IHttpContextAccessor httpContextAccessor)
        //{
        //    _uow = uow;
        //    _userRepository = uow.GetRepository<User>();
        //    _messageRepository = uow.GetRepository<Message>();
        //    _chatMessageRepository = uow.GetRepository<ChatMessage>();
        //    _activeUserRepository = uow.GetRepository<ActiveUserForSignalR>();
        //    _httpContextAccessor = httpContextAccessor;
        //}

        //public async Task OnlineUser()
        //{
        //    await Clients.All.SendAsync("onlineUser");
        //}

        //public async Task ChatMessage(string userName, string message)
        //{
        //    var nowDate = DateTime.Now;
        //    var chatEntity = new ChatMessage();
        //    try
        //    {
        //        var userId = _httpContextAccessor.HttpContext.Request.Cookies["UserId"];
        //        var messageLog = new ChatMessage
        //        {
        //            CreatedDate = nowDate,
        //            UserId = Convert.ToInt32(userId),
        //            Message = message
        //        };
        //        chatEntity = await _chatMessageRepository.InsertAsync(messageLog);
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    await Clients.All.SendAsync("broadcastMessage", userName, message, chatEntity.Id,
        //        nowDate.ToString("HH:mm:ss"));
        //}

        //public async Task GetChatMessages(int pageIndex)
        //{
        //    var userInfo =
        //        await _userRepository.FindAsync(
        //            Convert.ToInt32(_httpContextAccessor.HttpContext.Request.Cookies["UserId"]));
        //    if (pageIndex == 0)
        //        await UserLoginSetDateAsync(userInfo);

        //    var passMessageHistory = await Task.FromResult((from m in _chatMessageRepository.GetAll()
        //                                                    join u in _userRepository.GetAll() on m.UserId equals u.Id
        //                                                    select new EChatMessageDto
        //                                                    {
        //                                                        Id = m.Id,
        //                                                        UserId = m.UserId,
        //                                                        UserNameSurname = u.Name + " " + u.Surname,
        //                                                        Message = m.Message,
        //                                                        CreatedDate = m.CreatedDate
        //                                                    }).OrderByDescending(o => o.Id).Skip(pageIndex * 15).Take(15).ToList());

        //    await Clients.All.SendAsync("setChatMessages", passMessageHistory);
        //}

        //public async Task UserLoginSetDateAsync(User userInfo)
        //{
        //    try
        //    {
        //        if (userInfo.IsActive == null || !userInfo.IsActive.Value)
        //        {
        //            userInfo.IsActive = true;
        //            userInfo.LoginDate = DateTime.Now;
        //            await _uow.SaveChangesAsync();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //public async Task DeleteChatMessage(int id)
        //{
        //    var entity = await _chatMessageRepository.FindAsync(id);
        //    var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.Request.Cookies["UserId"]);
        //    if (entity.UserId == Convert.ToInt32(userId))
        //    {
        //        _chatMessageRepository.Delete(entity);
        //        await _uow.SaveChangesAsync();
        //        await Clients.All.SendAsync("chatMessageDeleted", id, userId);
        //    }
        //}

        //public async Task WritingMessage(string username)
        //{
        //    var message = username + " yazıyor";
        //    await Clients.All.SendAsync("chatMessageWriting", message);
        //}

        //public override Task OnConnectedAsync()
        //{
        //    //var connectionId = Context.ConnectionId;
        //    //var userId = Convert.ToInt32(_httpContextAccessor.HttpContext.Request.Cookies["UserId"]);
        //    //var url = _httpContextAccessor.HttpContext?.Request?.GetDisplayUrl();
        //    return base.OnConnectedAsync();
        //}

        //public override async Task OnDisconnectedAsync(Exception exception)
        //{
        //    try
        //    {
        //        var connectionId = Context.ConnectionId;
        //        var entity = _activeUserRepository.FirstOrDefault(w => w.Status && w.ConnectionId == connectionId);
        //        if (entity != null)
        //        {
        //            var allSessionKill = (await _activeUserRepository.WhereAsync(w => w.UserId == entity.UserId && w.Status)).ToList();
        //            allSessionKill.ForEach(f => f.Status = false);
        //            _activeUserRepository.UpdateRange(allSessionKill);
        //            _uow.SaveChanges();
        //        }

        //        await OnlineUser();
        //    }
        //    catch (Exception e) { }

        //    await Clients.All.SendAsync("OnLeft", DateTime.Now);
        //}
    }
}