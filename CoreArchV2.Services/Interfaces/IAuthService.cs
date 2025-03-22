using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Services.Interfaces
{
    public interface IAuthService
    {
        User Register(User user);
        User Login(User user);
        Task<bool> Any(string email);
    }
}