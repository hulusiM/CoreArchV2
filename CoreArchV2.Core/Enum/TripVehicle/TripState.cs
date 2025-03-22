namespace CoreArchV2.Core.Enum.TripVehicle
{
    public enum TripState
    {
        StartTrip = 7,
        EndTrip = 8,
        AddAddress = 9,//adres eklendi
        ChangeKm = 10,//km değişti
        AllowedForManager = 11,//Onaylı
        NotAllowedForManager = 12,//Onaysız
        CloseTrip = 13,//Yönetici onay vermezse görev kapalı statüsüne geçer
    }
}
