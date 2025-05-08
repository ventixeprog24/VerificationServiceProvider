namespace VerificationServiceProvider.Models
{
    public class EmailVerificationOptions
    {
        public string SenderAddress { get; set; } = null!;
        public string FrontendUrl { get; set; } = null!;
        public string VerificationPath { get; set; } = null!;
        public string DomainUrl { get; set; } = null!;
    }
}
