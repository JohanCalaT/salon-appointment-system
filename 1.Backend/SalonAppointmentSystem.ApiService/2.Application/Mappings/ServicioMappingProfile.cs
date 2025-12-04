using AutoMapper;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.Shared.DTOs.Servicios;

namespace SalonAppointmentSystem.ApiService.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para mapeos de Servicio
/// </summary>
public class ServicioMappingProfile : Profile
{
    public ServicioMappingProfile()
    {
        // Servicio -> ServicioDto
        CreateMap<Servicio, ServicioDto>();

        // CreateServicioRequest -> Servicio
        CreateMap<CreateServicioRequest, Servicio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Reservas, opt => opt.Ignore());

        // UpdateServicioRequest -> Servicio
        CreateMap<UpdateServicioRequest, Servicio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Reservas, opt => opt.Ignore());
    }
}

