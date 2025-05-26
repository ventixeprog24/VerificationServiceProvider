using JwtTokenServiceProvider;

namespace VerificationServiceProvider.Interfaces
{
    // Tagit hjälp av AI att skapa nedan interface i samband med testning 
    public interface IJwtTokenServiceClient
    {
        Task<TokenReply> GenerateTokenAsync(TokenRequest request);
        Task<ValidateReply> ValidateTokenAsync(ValidateRequest request);
    }
}
