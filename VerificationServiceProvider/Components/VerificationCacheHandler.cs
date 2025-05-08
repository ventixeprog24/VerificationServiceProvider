using Microsoft.Extensions.Caching.Memory;
using VerificationServiceProvider.Dtos;
using VerificationServiceProvider.Interfaces;

namespace VerificationServiceProvider.Components
{
    public class VerificationCacheHandler(IMemoryCache cache) : IVerificationCacheHandler
    {
        private readonly IMemoryCache _cache = cache;

        public void SaveVerificationCode(SaveVerificationCodeDto validationData)
        {
            var key = validationData.Email.ToLowerInvariant();
            _cache.Set(key, validationData.Code, validationData.ValidFor);
        }

        public bool ValidateVerificationCode(CodeValidationDto validationData)
        {
            var key = validationData.Email.ToLowerInvariant();

            if (_cache.TryGetValue(key, out string? storedCode) && storedCode == validationData.Code)
            {
                _cache.Remove(key);
                return true;
            }
            return false;
        }
    }
}
