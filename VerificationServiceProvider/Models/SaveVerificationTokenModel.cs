namespace VerificationServiceProvider.Models
{
    public class SaveVerificationTokenModel
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public TimeSpan ValidFor { get; set; }
    }
}
