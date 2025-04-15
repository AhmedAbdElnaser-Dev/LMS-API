namespace LMS_API.Controllers.Courses.ViewModels
{
    public class GroupInfo
    {
        public Guid Id { get; set; }
        public UserViewModel Instructor { get; set; }
    }
}
