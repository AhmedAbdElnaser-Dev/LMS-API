using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}

namespace LMS_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(8, 120, ErrorMessage = "Age must be between 8 and 120.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender selection.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Timezone is required.")]
        [StringLength(100, ErrorMessage = "Timezone cannot be longer than 100 characters.")]
        public string Timezone { get; set; } = "UTC";

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Telegram Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Telegram Number cannot be longer than 15 characters.")]
        public string TelegramNumber { get; set; }

        [Required(ErrorMessage = "Available Time is required.")]
        public DateTime AvailableTime { get; set; } = DateTime.UtcNow;
    }
}
