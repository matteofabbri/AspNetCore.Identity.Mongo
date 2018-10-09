using System.Threading.Tasks;

namespace Example.CustomUser.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
