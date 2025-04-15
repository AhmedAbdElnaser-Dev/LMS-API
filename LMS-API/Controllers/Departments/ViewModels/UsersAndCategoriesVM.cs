namespace LMS_API.Controllers.Departments.ViewModels
{
    public class UsersAndCategoriesVM
    {
        public List<UserVM> Users { get; set; } = new List<UserVM>();
        public List<CategoryVM> Categories { get; set; } = new List<CategoryVM>();
    }
}
