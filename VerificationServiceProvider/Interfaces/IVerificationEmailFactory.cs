using EmailServiceProvider;
using VerificationServiceProvider.Models;

namespace VerificationServiceProvider.Interfaces
{
    public interface IVerificationEmailFactory
    { 
        EmailRequest Create(VerificationEmailContentModel model);
    }
}
