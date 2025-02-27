using LMS_API.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace LMS_API.Services
{
    public class NoOpEmailSender : IEmailSender<ApplicationUser>
    {
        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) => Task.CompletedTask;
        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) => Task.CompletedTask;
        public Task SendEmailAsync(ApplicationUser user, string subject, string htmlMessage) => Task.CompletedTask;

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            throw new NotImplementedException();
        }
    }
}
