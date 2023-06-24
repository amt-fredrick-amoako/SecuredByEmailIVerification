using SecuredByEmailIVerification.Model;

namespace SecuredByEmailIVerification.ServiceContracts
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task SendWelcomeEmailAsync(WelcomeRequest request);
    }
}
