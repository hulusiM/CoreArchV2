namespace CoreArchV2.Services.Interfaces
{
    public interface IHangFireRunService
    {
        Task TripClosedControlAfterPushNotification();
        Task DontUploadPicturesSendPnMail();
    }
}
