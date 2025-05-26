using EmailServiceProvider;
using VerificationServiceProvider.Interfaces;

namespace VerificationServiceProvider.Helpers
{
    // Tagit hjälp av AI att skapa nedan interface i samband med testning 

    public class EmailServiceClientWrapper(EmailServicer.EmailServicerClient client) : IEmailServiceClient
    {
        private readonly EmailServicer.EmailServicerClient _client = client;

        public Task<EmailReply> SendEmailAsync(EmailRequest request)
            => _client.SendEmailAsync(request).ResponseAsync;
    }
}
