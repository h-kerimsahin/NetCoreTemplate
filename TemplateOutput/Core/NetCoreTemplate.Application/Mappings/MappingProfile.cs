using AutoMapper;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
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

        CreateMap<AppPermission, PermissionDto>()
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName.ToString()));

        CreateMap<AppRole, RoleDto>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.RolePermissions.Select(rp => rp.Permission).ToList()));

        CreateMap<AppUser, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Profile.FullName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile.LastName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile.PhoneNumber))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Profile.AvatarUrl))
            .ForMember(dest => dest.IsLockedOut, opt => opt.MapFrom(src => src.IsLockedOut));

        CreateMap<AuditEntry, AuditEntryDto>()
            .ForMember(dest => dest.EntityState, opt => opt.MapFrom(src => src.EntityState.ToString()))
            .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId.ToString()));

        CreateMap<AppNotification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.NotificationType));
    }
}