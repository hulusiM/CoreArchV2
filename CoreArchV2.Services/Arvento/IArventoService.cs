using CoreArchV2.Services.Arvento.Dto;

namespace CoreArchV2.Services.Arvento
{
    public interface IArventoService
    {
        EPlateFromNodeDto GetLicensePlateFromNode(string node);
        List<EGeneralReport2Dto> GeneralReport(DateTime start, DateTime end, string node);
        List<EVehicleArventoDto> GetArventoPlateList();
        void Arvento2ErpNodeGuncelle();
        void ArventoPlakaKoordinatEkle();
        void InsertPlateCoordinateRange();
        Task ArventoPlakaHiziSorgula();
        Task ArventoMesaiDisiKullanimRaporu();
        Task ArventoMesaiIciKullanimRaporu();
        Task AracKullanimRaporuMailGonder();
        Task AracSonKoordinatGuncelle();
        Task<List<ECoordinateDto>> GetAracSonKoordinatList();
    }
}
