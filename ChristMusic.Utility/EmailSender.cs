using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristMusic.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions emailOptions;

        public EmailSender(IOptions<EmailOptions> options)
        {
            emailOptions = options.Value;
        }

        public Task SendEmailAsync(string to, string subject, string htmlMessageBody)
        {
            return Execute(emailOptions.SendGridKey, subject, htmlMessageBody, to);
        }

        private Task Execute(string sendGridKey, string subject, string message, string to)
        {
            var client = new SendGridClient(sendGridKey);
            var _from = new EmailAddress("christMusicLtd@gmail.com", "ChristMusic");
            var _to = new EmailAddress(to, "End User");
            var msg = MailHelper.CreateSingleEmail(_from, _to, subject, "", message);
            return client.SendEmailAsync(msg);
        }
    }
}
