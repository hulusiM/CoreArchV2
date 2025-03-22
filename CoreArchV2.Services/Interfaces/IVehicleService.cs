using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Services.Interfaces
{
    public interface IVehicleService
    {
        PagedList<EVehicleDto> GetAllWithPaged(int? page, EVehicleDto filterModel);
        Vehicle FindPlate(string plate);
        List<Vehicle> FindPlateList(string plate);
        Task<EResultDto> SendMailVehiclePhotography(EMailVehiclePhotographyDto model);
        EResultDto InsertVehicle(EVehicleFixRentDto tempModel);
        EResultDto UpdateVehicle(EVehicleFixRentDto tempModel);
        EVehicleFixRentDto GetByIdVehicle(int id);
        Task<EResultDto> BulkCostUpdate(decimal amountPercent);
        Vehicle GetByIdJustVehicle(int id);

        EResultDto InsertVehicleExaminationDate(VehicleExaminationDate tempModel);
        EResultDto UpdateVehicleExaminationDate(VehicleExaminationDate tempModel);
        VehicleExaminationDate GetByIdVehicleExaminationDate(int id);
        EResultDto VehicleSetService(EVehicleServiceDto model);
        EResultDto VehicleOutService(EVehicleServiceDto model);

        EResultDto InsertVehicleDebit(VehicleDebit tempModel);
        EResultDto UpdateVehicleDebit(VehicleDebit tempModel);
        EVehicleDto GetByIdVehicleDebit(int vehicleId);
        VehicleDebit GetByVehicleDebitId(int vehicleDebitId);
        VehicleDebit GetByDebitWithVehicleId(int vehicleId);
        EResultDto VehicleDebitSetUserNull(VehicleDebit model);
        EResultDto VehicleDebitDelete(int vehicleDebitId);
        EVehicleDto GetByVehicleIdLastDebit(int vehicleId);//plakaya ait son zimmet bilgisi
        List<EVehicleDto> GetLastDebitUserHistory(int vehicleId);
        List<EVehicleDto> GetAllVehicleList(EVehicleDto filterModel);
        List<EVehicleDto> GetAllVehicleList();
        List<EVehicleDto> GetActiveVehicleForExcel(RFilterModelDto filterModel);//müdülürlük bazında excel
        Task<RVehicleFuelDto> GetVehicleDebitRangeByPlate(string plate, DateTime fuelDate);

        EVehicleDto GetByVehicleIdFileList(int vehicleId);
        EVehicleDto GetByVehicleIdLastLoadImageList(int vehicleId, int typeId);
        EResultDto VehicleFileInsert(IList<IFormFile> files, int vehicleId);
        EResultDto VehiclePhotographyInsert(EVehiclePhysicalImageLoadDto model);
        EResultDto VehicleDelete(IList<IFormFile> files, EVehicleTransferFileDto IncomingModel);

        EResultDto InsertVehicleAmount(VehicleAmount model);
        EVehicleContractWithAmountDto GetByVehicleIdVehicleAmountHistory(int vehicleId, int vehicleAmountTypeId, bool isAdmin);

        EResultDto InsertVehicleContract(VehicleContract model);
        EResultDto UpdateVehicleContract(VehicleContract model);
        EResultDto DeleteVehicleContract(int vehicleId);
        List<EVehicleContractWithAmountDto> GetByIdVehicleIdContractDateAndAmount(int vehicleId);
        EResultDto DeleteVehicleAmount(int vehicleContractId, int vehicleAmountId);

        EResultDto InsertVehicleMaterial(int vehicleId, int[] materials, int createdBy);
        int[] GetByIdVehicleMaterial(int vehicleId);

        Task<ENotificationDto> GetNotificationMessages(RFilterModelDto filterModel);
        Task<List<RVehicleCostDto>> GetTimeUpContractVehicle(RFilterModelDto filterModel);
        Task<List<EVehicleExaminationDateDto>> GetTimeUpExaminationVehicle(RFilterModelDto filterModel);
        Task<List<EVehicleDto>> GetServiceInVehicleList();

        Task<List<EVehicleDto>> GetNotLoadVehicleImage(EVehicleDto filterModel);


    }
}