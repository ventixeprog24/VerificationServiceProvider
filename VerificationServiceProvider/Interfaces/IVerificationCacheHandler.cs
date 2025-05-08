using VerificationServiceProvider.Models.Verification;

namespace VerificationServiceProvider.Interfaces
{
    public interface IVerificationCacheHandler
    {
        void SaveVerificationCode(SaveVerificationCodeModel model);
        void SaveVerificationToken(SaveVerificationTokenModel model);
        bool ValidateCode(CodeValidationModel model);
        bool ValidateToken(TokenValidationModel model);
    }
}
