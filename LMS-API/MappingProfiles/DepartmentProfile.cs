using AutoMapper;
using LMS_API.Controllers.Departments.Commands;
using LMS_API.Controllers.Departments.ViewModels;
using LMS_API.Models;

namespace LMS_API.MappingProfiles
{
    public class DepartmentProfile: Profile
    {
        public DepartmentProfile()
        {
            CreateMap<Department, DepartmentVM>();
            CreateMap<DepartmentVM, Department>();
            CreateMap<CreateDepartmentCommand, Department>();
        }
    }
}
