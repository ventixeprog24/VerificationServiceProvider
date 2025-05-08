using Microsoft.Extensions.Caching.Memory;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Models.Verification;

namespace VerificationServiceProvider.Infrastructure
{
    public class VerificationCacheHandler(IMemoryCache cache) : IVerificationCacheHandler
    {
        private readonly IMemoryCache _cache = cache;

        private string GetKey(string type, string email)
        {
            return $"verification_{type}:{email.ToLowerInvariant()}";
        }

        private bool Validate(string key, string value) 
        {
            if (_cache.TryGetValue(key, out string? storedValue) && storedValue == value)
            {
                _cache.Remove(key);
                return true;
            }
            return false;
        }

        public void SaveVerificationCode(SaveVerificationCodeModel model)
        {
            var key = GetKey("code", model.Email);
            _cache.Set(key, model.Code, model.ValidFor);
        }

        public void SaveVerificationToken(SaveVerificationTokenModel model)
        {
            var key = GetKey("token", model.Email);
            _cache.Set(key, model.Token, model.ValidFor);
        }

        public bool ValidateCode(CodeValidationModel model)
        {
            var key = GetKey("code", model.Email);
            return Validate(key, model.Code);
        }

        public bool ValidateToken(TokenValidationModel model)
        {
            var key = GetKey("token", model.Email);
            return Validate(key, model.Token);
        }
    }
}
