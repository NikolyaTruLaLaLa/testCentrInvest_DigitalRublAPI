using Application.DTO;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Client, ClientDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName} {src.Patronymic}".Trim()));
            CreateMap<Wallet, WalletDto>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive));
        }
    }
}
