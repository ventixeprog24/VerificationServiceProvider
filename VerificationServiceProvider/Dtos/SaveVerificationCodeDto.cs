namespace VerificationServiceProvider.Dtos
{
    public class SaveVerificationCodeDto
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public TimeSpan ValidFor { get; set; }
    }
}
