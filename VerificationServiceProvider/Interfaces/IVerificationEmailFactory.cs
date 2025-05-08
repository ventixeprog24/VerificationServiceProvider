namespace VerificationServiceProvider.Interfaces
{
    public interface IVerificationEmailFactory
    { 
        EmailMessage Create(string email, string code, string token);
    }
}
