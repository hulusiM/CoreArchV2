namespace CoreArchV2.Core.Enum
{
    public enum DebitState
    {
        Debit = 1, // Kullanıcıya zimmetleme 
        Pool = 2, // Havuza alma
        Deleted = 3, // Araç silinmiş
        InService = 4, //Serviste
    }
}