namespace VerificationServiceProvider.Models
{
    public class VerificationEmailContentModel
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
