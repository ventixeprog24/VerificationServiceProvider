namespace VerificationServiceProvider.Models.Verification
{
    public class TokenValidationModel
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
