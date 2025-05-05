using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Models;

namespace VerificationServiceProvider.Services
{
    public class VerificationService(IMemoryCache cache, IVerificationEmailFactory verificationEmailFactory) : VerificationContract.VerificationContractBase
    {
        private readonly IMemoryCache _cache = cache;
        private readonly IVerificationEmailFactory _verificationEmailFactory = verificationEmailFactory;
        //private readonly EmailServiceProvider _emailService = emailService;
        private static readonly Random _random = new();

        public override async Task<VerificationReply> SendVerificationCode(SendVerificationCodeRequest request, ServerCallContext context)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    return VerificationReplyFactory.Failed("Recipient email address is required.");

                var verificationCode = GenerateVerificationCode();
                var verificationToken = Guid.NewGuid().ToString(); //** Använda jwt servicen istället?

                var emailMessage = _verificationEmailFactory.Create(request.Email, verificationCode, verificationToken);

                //** Använda EmailServiceProvider
                var emailSent = await _emailService.SendEmailAsync(emailMessage);
                if (!emailSent)
                    return VerificationReplyFactory.Failed("Failed to send verification email.");


                SaveVerificationCode(new SaveVerificationCodeModel
                {
                    Email = request.Email,
                    Code = verificationCode,
                    ValidFor = TimeSpan.FromMinutes(5)
                });

                SaveVerificationToken(new SaveVerificationTokenModel
                {
                    Email = request.Email,
                    Token = verificationToken,
                    ValidFor = TimeSpan.FromMinutes(5)
                });


                return VerificationReplyFactory.Success("Verification email sent successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return VerificationReplyFactory.Failed("Failed to send verification email.");
            }
        }

        public override Task<VerificationReply> ValidateVerificationCode(ValidateVerificationCodeRequest request, ServerCallContext context)
        {
            var key = $"verification_code:{request.Email.ToLowerInvariant()}";

            if (_cache.TryGetValue(key, out string? storedCode))
            {
                if (storedCode == request.Code)
                {
                    _cache.Remove(key);
                    return Task.FromResult(VerificationReplyFactory.Success());
                }
            }
            return Task.FromResult(VerificationReplyFactory.Failed("Invalid or expired verification code."));
        }

        public override Task<VerificationReply> ValidateVerificationToken(ValidateVerificationTokenRequest request, ServerCallContext context)
        {
            var key = $"verification_token:{request.Email.ToLowerInvariant()}";

            if (_cache.TryGetValue(key, out string? storedToken))
            {
                if (storedToken == request.Token)
                    return Task.FromResult(VerificationReplyFactory.Success());
            }
            return Task.FromResult(VerificationReplyFactory.Failed("Invalid or expired verification token."));
        }

        public void SaveVerificationCode(SaveVerificationCodeModel model)
        {
            var key = $"verification_code:{model.Email.ToLowerInvariant()}";
            _cache.Set(key, model.Code, model.ValidFor);
        }

        public void SaveVerificationToken(SaveVerificationTokenModel model)
        {
            var key = $"verification_token:{model.Email.ToLowerInvariant()}";
            _cache.Set(key, model.Token, model.ValidFor);
        }

        private string GenerateVerificationCode()
        {
            var code = _random.Next(100000, 999999).ToString();
            return code;
        }
    }
}
