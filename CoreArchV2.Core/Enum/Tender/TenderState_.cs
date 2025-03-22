namespace CoreArchV2.Core.Enum.Tender
{
    public enum TenderState_
    {
        Draft = 1,           // Taslak
        SendInstitution = 2, // Kuruma gönderildi
        Revise = 3,          // Revizeye alındı
        NotResult = 4,       // Kurumdan olumsuz sonuçlandı
        WorkStarted = 6,     // İş başladı
        Delivered = 7,       // Teslim edildi
        StartedWarranty = 8, // Garanti başladı
        EndWarranty = 9,     // Garanti bitti
        JobIncrease = 10,    // İş arttırıma gidildi
        Removed = 11,        // Silindi
    }
}
