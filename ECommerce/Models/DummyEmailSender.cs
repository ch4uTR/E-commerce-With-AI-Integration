using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"[DUMMY EMAIL] To: {email}, Subject: {subject}");
            Console.WriteLine(htmlMessage);
            return Task.CompletedTask;
        }
    }
}

