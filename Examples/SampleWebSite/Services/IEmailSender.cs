using System.Threading.Tasks;

namespace Example.DefaultUser.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}