using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;

namespace CoreArchV2.Services.Interfaces
{
    public interface IMessageService
    {
        bool IsAny(string desc);
        EResultDto FuelMessageInsert(Message model, bool sendPn = false);
        void DeleteRange(EFuelLogDto model);
    }
}
