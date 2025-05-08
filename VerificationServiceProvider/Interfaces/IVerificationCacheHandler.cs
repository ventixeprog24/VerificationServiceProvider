using VerificationServiceProvider.Models;

namespace VerificationServiceProvider.Interfaces
{
    public interface IVerificationCacheHandler
    {
        void SaveVerificationCode(SaveVerificationCodeModel model);
        bool ValidateVerificationCode(CodeValidationModel model);
    }
}
