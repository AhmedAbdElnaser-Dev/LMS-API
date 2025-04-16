using System;
using System.Threading.Tasks;
using LMS_API.Models;
using LMS_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers.Units.Commands
{
    public class AddUnitCommand
    {
        public Guid CourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
