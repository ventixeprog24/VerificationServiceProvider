using EmailServiceProvider;

namespace VerificationServiceProvider.Interfaces
{
    // Tagit hjälp av AI att skapa nedan interface i samband med testning 
    public interface IEmailServiceClient
    {
        Task<EmailReply> SendEmailAsync(EmailRequest request);
    }
}
