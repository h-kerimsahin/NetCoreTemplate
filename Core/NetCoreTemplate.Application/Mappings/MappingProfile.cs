using AutoMapper;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.UserProfile;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppUser, MeResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Profile.FullName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile.LastName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile.PhoneNumber))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Profile.AvatarUrl));
        CreateMap<AppUserProfile, UserProfileDto>();
    }
}
