namespace PlanningAPI.Models
{
    using AutoMapper;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Module, ModuleDto>();

            CreateMap<OrgSecGrpSetup, OrgSecGrpSetupDto>()
                .ForMember(dest => dest.ModuleName,
                    opt => opt.MapFrom(src => src.Module.Name))
                .ForMember(dest => dest.ProfileName,
                    opt => opt.MapFrom(src => src.OrgSecProfile.Name));
        }
    }
}
