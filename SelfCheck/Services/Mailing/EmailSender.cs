using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SampleSite.Mailing
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public string UserId { get;set; }

        public string Token { get; set; }

        public Task SendMailConfirmationLink(string userId, string token)
        {
            UserId = userId;
            Token = token;
            return Task.CompletedTask;
        }

        Task IEmailSender.SendMailPasswordReset(string userId, string token)
        {
            return Task.CompletedTask;
        }
    }
}
