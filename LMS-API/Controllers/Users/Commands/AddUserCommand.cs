using System;
using System.ComponentModel.DataAnnotations;
using LMS_API.Models.Enums;

namespace LMS_API.Controllers.Users.Commands
{
    public class AddUserCommand
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(8, 120, ErrorMessage = "Age must be between 8 and 120.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender selection.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Timezone is required.")]
        [StringLength(100, ErrorMessage = "Timezone cannot be longer than 100 characters.")]
        public string Timezone { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Telegram Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Telegram Number cannot be longer than 15 characters.")]
        public string TelegramNumber { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(SuperAdmin|Admin|Manager|User)$", ErrorMessage = "Invalid role. Allowed roles: SuperAdmin, Admin, Manager, User.")]
        public string Role { get; set; }
    }
}
