using Microsoft.Extensions.Caching.Memory;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Models;

namespace VerificationServiceProvider.Components
{
    public class VerificationCacheHandler(IMemoryCache cache) : IVerificationCacheHandler
    {
        private readonly IMemoryCache _cache = cache;

        public void SaveVerificationCode(SaveVerificationCodeModel model)
        {
            var key = model.Email.ToLowerInvariant();
            _cache.Set(key, model.Code, model.ValidFor);
        }

        public bool ValidateVerificationCode(CodeValidationModel model)
        {
            var key = model.Email.ToLowerInvariant();

            if (_cache.TryGetValue(key, out string? storedCode) && storedCode == model.Code)
            {
                _cache.Remove(key);
                return true;
            }
            return false;
        }
    }
}
