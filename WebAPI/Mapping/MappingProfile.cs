using Application.DTO;
using AutoMapper;
using Domain.Entities; 
using WebAPI.Contracts;

namespace WebAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Client, ClientResponse>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.LastName} {src.FirstName} {src.Patronymic}"));
            
            CreateMap<ClientDto, ClientResponse>();
            CreateMap<WalletDto, WalletResponse>();

            CreateMap<Wallet, WalletResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
