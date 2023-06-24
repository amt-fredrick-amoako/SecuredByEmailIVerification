using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SecuredByEmailIVerification.Model;
using SecuredByEmailIVerification.ServiceContracts;
using SecuredByEmailIVerification.Settings;

namespace SecuredByEmailIVerification.Services
{
    public class MailService : IMailService
    {
        /*
         * Learning Email sending with SMTPClient
         * **************************************
         * Steps to setup mail
         * **************************************
         * create a new MimeMessage object
         * use the MailBoxAddress.Parse to parse the address of the sender and set it to the email.Sender property
         * use the same static method to parse the email of the reciever and set it to the email.To with the add method
         * set the subject property.
         * create a new BodyBuilder object
         * check to see if there are attachments
         * if there are attachments create a memorystream object and load the attachments to the stream
         * set the stream object to a bytes array i.e byte[] fileBytes
         * add the fileBytes array to the BodyBuilder's Attachment list
         * add the body to the bodyBuilder.HtmlBody property
         * now set the email to builder.ToMessageBody method this constucts message body based on attachments, linked resources and text-based messages
         * 
         * ***********************************************************
         * Steps to setup a SMTPClient and send the already setup mail
         * ***********************************************************
         * create a new SMTPClient object
         * use the SMTPClient.Connect method to connect to the mail server using the appropriate method parameters i.e Host, Port, SocketConnection options
         * use the SMTPClient.Authenticate method to pass authentication parameters to the server i.e username and password
         * now send mail using the SMTPClient.SendAsync method and pass in the configured email object
         * Disconnect from server after sending mail
         */
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> options)
        {
            _mailSettings = options.Value;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async Task SendWelcomeEmailAsync(WelcomeRequest request)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\csharp\Template\");
            using StreamReader strReader = new StreamReader(directoryInfo.FullName + "WelcomeTemplate.html");
            string MailText = strReader.ReadToEnd();
            strReader.Close();

            MailText = MailText.Replace("[username]", request.UserName).Replace("[email]", request.ToEmail);
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject = $"Welcome {request.UserName}";
            var builder = new BodyBuilder();
            builder.HtmlBody = MailText;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);

        }
    }
}
