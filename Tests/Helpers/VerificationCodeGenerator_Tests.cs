using VerificationServiceProvider.Helpers;

namespace Tests.Helpers
{
    public class VerificationCodeGenerator_Tests
    {
        private readonly VerificationCodeGenerator _generator = new();

        [Fact]
        public void GenerateVerificationCode_ShouldReturn6DigitString()
        {
            var code = _generator.GenerateVerificationCode();

            Assert.NotNull(code);
            Assert.Equal(6, code.Length);
            Assert.All(code, c => Assert.Contains(c, "0123456789"));  
        }

        [Fact]
        public void GenerateVerificationCode_ShouldReturnDifferentCodes()
        {
            var code1 = _generator.GenerateVerificationCode();
            var code2 = _generator.GenerateVerificationCode();

            Assert.NotEqual(code1, code2);
        }
    }
}
