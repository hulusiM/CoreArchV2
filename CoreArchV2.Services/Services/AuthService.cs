using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Util.Hash;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _authRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public AuthService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _authRepository = uow.GetRepository<User>();
        }

        public User Register(User user)
        {
            user.Password = OneWayHash.Create(user.Password);
            _authRepository.Insert(user);
            return user;
        }

        public User Login(User user)
        {
            var _user = _authRepository.FirstOrDefault(x => x.MobilePhone == user.MobilePhone && x.Password == OneWayHash.Create(user.Password));
            if (_user == null)
                return null;

            return _user;
        }

        public async Task<bool> Any(string email)
        {
            if (await _authRepository.AnyAsync(x => x.Email == email))
                return true;

            return false;
        }
    }
}