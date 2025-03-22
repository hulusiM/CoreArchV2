using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Licence.Dto;
using CoreArchV2.Core.Entity.Licence.Entity;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.NoticeVehicle.Notice;
using CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_;
using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeUnitDto_;
using CoreArchV2.Dto.ETenderDto;
using CoreArchV2.Dto.ETripDto;

namespace CoreArchV2.Web.MapProfileWeb
{
    public class MapperProfileWeb : Profile
    {
        public MapperProfileWeb()
        {
            CreateMap<User, EUserDto>();
            CreateMap<EUserDto, User>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore())
                .ForMember(f => f.Image, opt => opt.Ignore())
                .ForMember(f => f.Password, opt => opt.Ignore());

            CreateMap<Authorization, EAuthorizationDto>();
            CreateMap<EAuthorizationDto, Authorization>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<UserRole, EUserRoleDto>();
            CreateMap<EUserRoleDto, UserRole>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<VehicleTransferLog, EVehicleTransferFileDto>();
            CreateMap<EVehicleTransferFileDto, VehicleTransferLog>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<Maintenance, EMaintenanceDto>();
            CreateMap<EMaintenanceDto, Maintenance>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());


            CreateMap<Vehicle, EVehicleFixRentDto>();
            CreateMap<EVehicleFixRentDto, Vehicle>()
                .ForMember(f => f.Id, opt => opt.Ignore())
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore())
                .ForMember(f => f.LastUserId, opt => opt.Ignore());


            CreateMap<VehicleRent, EVehicleFixRentDto>();
            CreateMap<EVehicleFixRentDto, VehicleRent>()
                .ForMember(f => f.Id, opt => opt.Ignore());

            CreateMap<FuelLog, EFuelLogDto>();
            CreateMap<EFuelLogDto, FuelLog>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<CriminalLog, ECriminalLogDto>();
            CreateMap<ECriminalLogDto, CriminalLog>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<Tire, ETireDto>();
            CreateMap<ETireDto, Tire>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<VehicleRequest, EVehicleRequestDto>();
            CreateMap<EVehicleRequestDto, VehicleRequest>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<TireDebit, ETireDto>();
            CreateMap<ETireDto, TireDebit>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<Unit, EUnitDto>();
            CreateMap<EUnitDto, Unit>()
                .ForMember(f => f.ParentId, opt => opt.Ignore());

            CreateMap<VehicleBrandModel, EUnitDto>();
            CreateMap<EUnitDto, VehicleBrandModel>()
                .ForMember(f => f.ParentId, opt => opt.Ignore());

            CreateMap<VehicleDebit, VehicleDebit>()
                .ForMember(f => f.State, opt => opt.Ignore())
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.StartDate, opt => opt.Ignore())
                .ForMember(f => f.EndDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());


            #region Tender
            CreateMap<Institution, EUnitDto>();
            CreateMap<EUnitDto, Institution>()
                .ForMember(f => f.ParentId, opt => opt.Ignore());

            CreateMap<TenderDetail, ETenderDetailDto>();
            CreateMap<TenderDetail, TenderDetail>();
            CreateMap<ETenderDetailDto, TenderDetail>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());

            CreateMap<Tender, ETender_Dto>();
            CreateMap<ETender_Dto, Tender>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore())
                .ForMember(f => f.CreatedUnitId, opt => opt.Ignore())
                .ForMember(f => f.State, opt => opt.Ignore())
                .ForMember(f => f.SalesNumber, opt => opt.Ignore());


            #endregion

            #region Notice
            CreateMap<Notice, ENoticeDto>();
            CreateMap<ENoticeDto, Notice>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.State, opt => opt.Ignore())
                .ForMember(f => f.ImportType, opt => opt.Ignore());

            CreateMap<NoticeUnit, ENoticeUnitDto>();
            CreateMap<ENoticeUnitDto, NoticeUnit>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedIp, opt => opt.Ignore());
            #endregion

            #region Trip
            CreateMap<Trip, ETripDto>();
            CreateMap<ETripDto, Trip>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.StartDate, opt => opt.Ignore())
                .ForMember(f => f.State, opt => opt.Ignore());
            #endregion


            CreateMap<LicenceKey, LicenceKeyDto>().ReverseMap();
            CreateMap<LicenceKeyDto, LicenceKey>()
                .ForMember(f => f.FirmKey, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.ErrorMessage, opt => opt.Ignore())
                .ForMember(f => f.LockDate, opt => opt.Ignore())
                .ForMember(f => f.ActiveUserCount, opt => opt.Ignore())
                .ForMember(f => f.ActiveVehicleCount, opt => opt.Ignore())
                .ForMember(f => f.CreatedBy, opt => opt.Ignore());
        }
    }
}