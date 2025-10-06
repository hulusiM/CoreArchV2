using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Dto.EApiDto;
using CoreArchV2.Dto.ETripDto;

namespace CoreArchV2.Api.MapProfileApi
{
    public class MapperProfileApi : Profile
    {
        public MapperProfileApi()
        {
            CreateMap<AUserDto, User>();
            CreateMap<User, AUserDto>();

            CreateMap<AUserLoginDto, User>();
            CreateMap<AUserRegisterDto, User>();

            CreateMap<Trip, ETripDto>();
            CreateMap<ETripDto, Trip>()
                .ForMember(f => f.CreatedBy, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.CreatedDate, opt => opt.Ignore())
                .ForMember(f => f.StartDate, opt => opt.Ignore())
                .ForMember(f => f.State, opt => opt.Ignore());
        }
    }
}
