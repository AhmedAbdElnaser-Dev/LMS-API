using AutoMapper;
using LMS_API.Controllers.Departments.Commands;
using LMS_API.Controllers.Departments.ViewModels;
using LMS_API.Models;

namespace LMS_API.MappingProfiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<Department, DepartmentVM>()
                .ForMember(dest => dest.Supervisor, opt => opt.MapFrom(src =>
                    src.Supervisor != null
                        ? new SupervisorVM
                        {
                            Id = src.SupervisorId,
                            FullName = $"{src.Supervisor.FirstName} {src.Supervisor.LastName}"
                        }
                        : new SupervisorVM { Id = src.SupervisorId, FullName = "Supervisor Not Found" }))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    src.Category != null ? src.Category.Name : "No Category"))
                .ForMember(dest => dest.Translations, opt => opt.MapFrom<DepartmentTranslationsResolver>());

            CreateMap<DepartmentTranslation, DepartmentTranslationVM>();

            CreateMap<EditDepartmentCommand, Department>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SupervisorId, opt => opt.MapFrom(src => src.SupervisorId))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId));

            CreateMap<CreateDepartmentCommand, Department>();

            CreateMap<DepartmentTranslation, CreateDepartmentTranslationCommand>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language));

            CreateMap<CreateDepartmentTranslationCommand, DepartmentTranslation>();

            CreateMap<DepartmentTranslation, DepartmentTranslationVM>();

            CreateMap<EditDepartmentTranslationCommand, DepartmentTranslation>();
        }
    }
}
