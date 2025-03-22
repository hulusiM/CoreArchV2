namespace CoreArchV2.Core.Enum
{
    /// <summary>
    /// 1Ekleme yaparken Combo/LookUpList metodunu güncelle !!!!
    /// </summary>
    public enum TypeList
    {
        //Logistics
        VehicleType = 1, //Araç tipi
        FuelType = 2, //Yakıt Tipi
        UsageType = 3, //Kullanım tipi
        FixtureType = 4, //Demirbaş Liste
        CriminialType = 5, //Ceza türleri
        RentACarFirm = 6, //Kiralama firmaları
        SupplierACar = 7, //Bakım/onarım tedarikçi liste
        MaintenancetType = 8, //Bakım/onarım türü
        FuelStationIdType = 9, //Benzin istasyonları
        TireTypeId = 10, //Lastik tipleri
        DimensionTypeId = 11, //Lastik ebatları
        VehicleDeleteTypeId = 12, //Araç silme nedenleri
        VehicleRentType = 13, //Araç kiralama türü
        VehicleMaterialType = 14, //Araç üstündeki malzemeler (yangın tüpü,zincir vs)

        //15:boşta
        //Tender
        ContactType = 16,//İletişim Kanalı
        TenderUnitType = 17,//İhale Birim türleri --> adet,metre,metrekare
        MoneyType = 18, //Para Birimi
        TenderProjectType = 19, //Teklif Satış kalemi

        //Logistics
        VehicleAmountType = 20, //Araç Tutar Bedeli
        EnginePowerType = 21, //Motor cc bilgileri

        FirstVehicleImage = 22, //Araç ilk fotoğrafları tipi
        LastVehicleImage = 23, //Araç son fotoğrafları tipi
        AccidentVehicleImage = 24, //Araç kaza fotoğrafları tipi
        GeneralVehicleImage = 25, //Araç genel belge tipi 
        GearType = 26, //Vites türü
    }
}