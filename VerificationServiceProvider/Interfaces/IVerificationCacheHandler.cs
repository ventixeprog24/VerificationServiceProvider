using VerificationServiceProvider.Dtos;

namespace VerificationServiceProvider.Interfaces
{
    public interface IVerificationCacheHandler
    {
        void SaveVerificationCode(SaveVerificationCodeDto validationData);
        bool ValidateVerificationCode(CodeValidationDto validationData);
    }
}
