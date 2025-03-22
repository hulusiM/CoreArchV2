using Microsoft.AspNetCore.DataProtection;

namespace CoreArchV2.Core.Util.Hash
{
    //Kullanımı 
    //var SCollection = new ServiceCollection();
    //SCollection.AddDataProtection();
    //var LockerKey = SCollection.BuildServiceProvider();
    //var locker = ActivatorUtilities.CreateInstance<TwoWayHash>(LockerKey);
    //string encryptKey = locker.Encrypt("hulusi");
    //string deencryptKey = locker.Decrypt(encryptKey);

    public class TwoWayHash //Çift yönlü şifreleme yapar
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private const string Key = "salt-hulusi-pass";
        public TwoWayHash(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        public string Encrypt(string input)
        {
            var protector = _dataProtectionProvider.CreateProtector(Key);
            return protector.Protect(input);
        }

        public string Decrypt(string input)
        {
            var protector = _dataProtectionProvider.CreateProtector(Key);
            return protector.Unprotect(input);
        }
    }
}
