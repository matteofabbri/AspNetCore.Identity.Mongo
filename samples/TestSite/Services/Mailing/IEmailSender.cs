using System.Threading.Tasks;

namespace SampleSite.Mailing;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
}