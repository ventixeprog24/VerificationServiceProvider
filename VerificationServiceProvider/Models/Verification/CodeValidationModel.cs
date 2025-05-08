namespace VerificationServiceProvider.Models.Verification
{
    public class CodeValidationModel
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
