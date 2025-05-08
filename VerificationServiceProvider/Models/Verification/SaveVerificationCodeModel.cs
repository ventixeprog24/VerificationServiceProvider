namespace VerificationServiceProvider.Models.Verification
{
    public class SaveVerificationCodeModel
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public TimeSpan ValidFor { get; set; }
    }
}
