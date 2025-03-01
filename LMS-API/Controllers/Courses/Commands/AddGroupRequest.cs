using System.ComponentModel.DataAnnotations;

namespace LMS_API.Controllers.Courses.Commands
{
	public class AddGroupRequest
	{
		[Required]
		public Guid InstructorId { get; set; }
	}
}
