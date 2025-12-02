using AutoMapper;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Users;

namespace SalonAppointmentSystem.ApiService.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para mapeos de Usuario
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // ApplicationUser -> UserDto
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.EmailConfirmado, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.EstacionNombre, opt => opt.MapFrom(src => src.Estacion != null ? src.Estacion.Nombre : null))
            .ForMember(dest => dest.Rol, opt => opt.Ignore()); // Se asigna manualmente desde UserManager

        // CreateUserRequest -> ApplicationUser
        CreateMap<CreateUserRequest, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.PuntosAcumulados, opt => opt.MapFrom(src => 0))
            // Ignorar propiedades que no vienen del request
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.Estacion, opt => opt.Ignore())
            .ForMember(dest => dest.Reservas, opt => opt.Ignore());

        // UpdateUserRequest -> ApplicationUser (para actualizaciones completas)
        CreateMap<UpdateUserRequest, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            // Ignorar propiedades que no deben actualizarse
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
            .ForMember(dest => dest.Estacion, opt => opt.Ignore())
            .ForMember(dest => dest.Reservas, opt => opt.Ignore());
    }
}

