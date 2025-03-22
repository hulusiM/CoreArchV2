namespace CoreArchV2.Utilies
{
    public class AppSettingParam
    {
    }

    public class MailSetting
    {
        public string MethodNo { get; set; } = "1";
        public string Host { get; set; }
        public string FromEmail { get; set; }
        public int Port { get; set; }
        public string Pass { get; set; }
        public string DisplayName { get; set; }
    }

    public class FirmSetting
    {
        public string DisplayName { get; set; }
        public string WebSite { get; set; }
        public string WebSiteName { get; set; }
        public string HangfireUrl { get; set; }
        public string MobileApkUrl { get; set; }
    }

    public class ArventoSetting
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class POSetting
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FleetId { get; set; }
    }

    public class MobilePushNotificationToken
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; }
    }
    public class WebSendPushNotification
    {
        public string Id { get; set; }
        public string Key { get; set; }
    }

    public class LicenceSetting
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
    }
}
