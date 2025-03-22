namespace CoreArchV2.Core.Enum.NoticeVehicle.Notice
{
    public enum NoticeState
    {
        Draft = 1,    //Taslak
        SendUnit = 2, //Birime gönderildi
        SendCancelled = 3,//Gönderim iptal edildi
        OpenNotice = 4,//Talep açıldı
        AnswerUnit = 5,//Birimden cevap geldi
        RedirectToCancelled = 6,//yönlendirirken iptal edildi



        Mission = 20,//Görevde
        OffMission = 21,//Görev dışı
        SpecialPermission = 22, //Özel izin
        Other = 23,//Diğer
        Closed = 99,    //Kapatıldı
    }
}
