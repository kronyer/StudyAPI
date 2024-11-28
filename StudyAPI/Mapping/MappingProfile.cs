using AutoMapper;
using StudyAPI.DTOs;
using StudyAPI.Models;

namespace StudyAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() { 
        
            CreateMap<Villa, VillaDTO>().ReverseMap(); //O autoMapper usa o nome das propriedades para mapear
            CreateMap<Villa, VillaCreateDTO>().ReverseMap();
            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();

            CreateMap<VillaNumber, VillaNumberDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberCreateDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberUpdateDTO>().ReverseMap();



        }
    }
}
