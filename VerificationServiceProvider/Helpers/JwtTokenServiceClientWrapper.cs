using JwtTokenServiceProvider;
using VerificationServiceProvider.Interfaces;

namespace VerificationServiceProvider.Helpers
{
    // Tagit hjälp av AI att skapa nedan interface i samband med testning 
    public class JwtTokenServiceClientWrapper(JwtTokenServiceContract.JwtTokenServiceContractClient client) : IJwtTokenServiceClient
    {
        private readonly JwtTokenServiceContract.JwtTokenServiceContractClient _client = client;

        public Task<TokenReply> GenerateTokenAsync(TokenRequest request)
            => _client.GenerateTokenAsync(request).ResponseAsync;

        public Task<ValidateReply> ValidateTokenAsync(ValidateRequest request)
            => _client.ValidateTokenAsync(request).ResponseAsync;
    }
}
