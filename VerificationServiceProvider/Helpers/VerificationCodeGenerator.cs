using VerificationServiceProvider.Interfaces;

namespace VerificationServiceProvider.Helpers
{
    public class VerificationCodeGenerator : ICodeGenerator
    {
        private static readonly Random _random = new();

        public string GenerateVerificationCode()
        {
            var code = _random.Next(100000, 999999).ToString();
            return code;
        }
    }
}
