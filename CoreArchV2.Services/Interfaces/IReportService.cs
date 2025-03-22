using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Dto.ETripDto;

namespace CoreArchV2.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<EVehicleDto>> GetActiveListWithMemoryCache();
        Task<EUserDto> GetCachedMemoryActiveUser();

        //Vehicle
        Task<List<RVehicleDto>> RentVehicleFirmCount(RFilterModelDto model);
        Task<List<RVehicleDto>> ManagementVehicleCount(RFilterModelDto model);
        Task<List<RVehicleDto>> UsageTypeVehicleCount(RFilterModelDto model);
        Task<List<RVehicleDto>> FixVehicleTotalAmount(RFilterModelDto model);
        Task<List<RVehicleDto>> ModelYearVehicleCount(RFilterModelDto model);
        Task<List<RVehicleDto>> ModelYearPercentVehicleCount(RFilterModelDto model);

        //VehicleFuel
        Task<RDashboardDto> HeaderInfoFuel(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> MontlyFuelTotalAmount(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> ProjectFuelTotalAmount(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> SubProjectFuelTotalAmount(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> VehicleFuelTotalAmount(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> VehicleFuelTotalKmSpend(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> VehicleFuelTotalSupplierAmount(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> GetFuelListRange(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> VehicleDebitAndMonthlyFuelCompare(RFilterModelDto model);

        //VehicleMaintenance
        Task<RDashboardDto> HeaderInfoMaintenance(RFilterModelDto model);
        Task<List<RVehicleMaintenanceDto>> MontlyMaintenanceTotalAmount(RFilterModelDto model);
        Task<List<RVehicleMaintenanceDto>> ProjectMaintenanceTotalAmount(RFilterModelDto model); //Müdürlük bazında
        Task<List<RVehicleMaintenanceDto>> SubProjectMaintenanceTotalAmount(RFilterModelDto model); //Proje bazında
        Task<object> VehicleMaintenanceTotalAmount(RFilterModelDto model);
        Task<List<RVehicleMaintenanceDto>> FirmVehicleMaintenanceTotalAmountAndCount(RFilterModelDto model);
        Task<List<RVehicleMaintenanceDto>> GetMaintenanceListRange(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> GetDebitListRange(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> GetDebitListRangeForExcel(RFilterModelDto model);
        Task<List<EVehicleDto>> GetLastVehicleDebit(RFilterModelDto model);
        Task<RDashboardDto> ActiveVehicleCount(RFilterModelDto model);

        //VehicleCost
        Task<RAllReportDto> HeaderInfoVehicleCost(RFilterModelDto model);
        Task<RVehicleCostDto> VehicleCostTotalAmount(RFilterModelDto model);
        Task<List<RVehicleCostDto>> MonthlyVehicleCost(RFilterModelDto model);
        //Task<RVehicleCostDto> VehicleCostTotalAmountNotDebit(RFilterModelDto model);
        Task<object> VehicleDetailCostTotalAmount(RFilterModelDto model);
        Task<List<RVehicleCostDto>> GetVehicleCostWithDebitList(RFilterModelDto model);

        //Hgs report
        Task<RVehicleCostDto> GetHgsReport(RFilterModelDto model);
        Task<List<RVehicleMaintenanceDto>> HgsWithDebitlist(RFilterModelDto model, bool isMainTypeAllWithJoin = false);
        Task<List<RVehicleCostDto>> MonthlyHgsTotalAmount(RFilterModelDto model);

        //Trip
        Task<RDashboardDto> HeaderInfoTrip(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> PlateTrip(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> PlateTripKm(RFilterModelDto model);
        Task<List<ETripDto>> MonthlyTrip(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> ProjectTripTotal(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> SubProjectTrip(RFilterModelDto model);
        Task<RDashboardDto> HeaderInfoTripUser(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> TripTotalCountUser(RFilterModelDto model);
        Task<List<RVehicleFuelDto>> TripTotalKmUser(RFilterModelDto model);
    }
}