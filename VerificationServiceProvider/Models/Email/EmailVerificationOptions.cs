namespace VerificationServiceProvider.Models.Email
{
    public class EmailVerificationOptions
    {
        public string FrontendUrl { get; set; } = null!;
        public string VerificationPath { get; set; } = null!;
        public string DomainUrl { get; set; } = null!;
    }
}
