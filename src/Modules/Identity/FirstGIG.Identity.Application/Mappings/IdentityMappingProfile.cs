using AutoMapper;
using FirstGIG.Identity.Application.DTOs;
using FirstGIG.Identity.Domain.Entities;

namespace FirstGIG.Identity.Application.Mappings;

public sealed class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Value))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));
    }
}
