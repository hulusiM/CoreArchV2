using CoreArchV2.Services.Arvento.Dto;

namespace CoreArchV2.Services.Arvento
{
    public interface IArventoService
    {
        Task<EPlateFromNodeDto> GetLicensePlateFromNode(string node);
        Task<List<EGeneralReport2Dto>> GeneralReport(DateTime start, DateTime end, string node);
        Task InsertPlateCoordinateRange();
        Task<List<EVehicleArventoDto>> GetArventoPlateList();
        Task Arvento2ErpNodeGuncelle();
        Task ArventoPlakaKoordinatEkle();
        Task ArventoPlakaHiziSorgula();
        Task ArventoMesaiDisiKullanimRaporu();
        Task ArventoMesaiIciKullanimRaporu();
        Task AracKullanimRaporuMailGonder();
        Task AracSonKoordinatGuncelle();
        Task<List<ECoordinateDto>> GetAracSonKoordinatList();
    }
}
