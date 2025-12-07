using AutoMapper;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para mapeos de Reserva
/// </summary>
public class ReservaMappingProfile : Profile
{
    public ReservaMappingProfile()
    {
        // Reserva -> ReservaDto
        CreateMap<Reserva, ReservaDto>()
            .ForMember(dest => dest.EstacionNombre,
                opt => opt.MapFrom(src => src.Estacion != null ? src.Estacion.Nombre : string.Empty))
            .ForMember(dest => dest.BarberoNombre,
                opt => opt.MapFrom(src => src.Estacion != null && src.Estacion.Barbero != null
                    ? src.Estacion.Barbero.NombreCompleto
                    : null))
            .ForMember(dest => dest.ServicioNombre,
                opt => opt.MapFrom(src => src.Servicio != null ? src.Servicio.Nombre : string.Empty))
            .ForMember(dest => dest.FechaHoraFin,
                opt => opt.MapFrom(src => src.FechaHora.AddMinutes(src.DuracionMinutos)));

        // Reserva -> ReservaListDto
        CreateMap<Reserva, ReservaListDto>()
            .ForMember(dest => dest.EstacionNombre,
                opt => opt.MapFrom(src => src.Estacion != null ? src.Estacion.Nombre : string.Empty))
            .ForMember(dest => dest.ServicioNombre,
                opt => opt.MapFrom(src => src.Servicio != null ? src.Servicio.Nombre : string.Empty));
    }
}

