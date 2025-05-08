using Grpc.Core;
using JwtTokenServiceProvider;
using System.Diagnostics;
using VerificationServiceProvider.Dtos;
using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Interfaces;
using JwtTokenServiceClient = JwtTokenServiceProvider.JwtTokenServiceContract.JwtTokenServiceContractClient;

namespace VerificationServiceProvider.Services
{
    public class VerificationService(IVerificationEmailFactory emailFactory, IVerificationCacheHandler cacheHandler, ICodeGenerator codeGenerator, JwtTokenServiceClient jwtTokenService) : VerificationContract.VerificationContractBase
    {
        private readonly IVerificationEmailFactory _emailFactory = emailFactory;
        private readonly IVerificationCacheHandler _cacheHandler = cacheHandler;
        private readonly ICodeGenerator _codeGenerator = codeGenerator;
        private readonly JwtTokenServiceClient _jwtTokenService = jwtTokenService;
        //private readonly EmailServiceProvider _emailService = emailService;

        public override async Task<VerificationReply> SendVerificationCode(SendVerificationCodeRequest request, ServerCallContext context)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    return VerificationReplyFactory.Failed("Email address is required.");

                var code = _codeGenerator.GenerateVerificationCode();

                var tokenReply = await _jwtTokenService.GenerateTokenAsync(new TokenRequest { Email = request.Email });
                if (!tokenReply.Succeeded)
                    return VerificationReplyFactory.Failed("Failed to generate token.");

                var emailMessage = _emailFactory.Create(request.Email, code, tokenReply.TokenMessage);

                //** Ersätta med EmailServiceProvider
                var emailSent = await _emailService.SendEmailAsync(emailMessage);
                if (!emailSent)
                    return VerificationReplyFactory.Failed("Failed to send verification email.");

                _cacheHandler.SaveVerificationCode(new SaveVerificationCodeDto
                {
                    Email = request.Email,
                    Code = code,
                    ValidFor = TimeSpan.FromMinutes(15)
                });

                return VerificationReplyFactory.Success("Verification email sent.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return VerificationReplyFactory.Failed("Failed to send verification email.");
            }
        }

        public override Task<VerificationReply> ValidateVerificationCode(ValidateVerificationCodeRequest request, ServerCallContext context)
        {
            try
            {
                var valid = _cacheHandler.ValidateVerificationCode(new CodeValidationDto { Email = request.Email, Code = request.Code });

                return Task.FromResult(valid
                    ? VerificationReplyFactory.Success("Valid code.")
                    : VerificationReplyFactory.Failed("Invalid or expired verification code."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Task.FromResult(VerificationReplyFactory.Failed("Failed to validate verification code."));
            }
        }

        public override async Task<VerificationReply> ValidateVerificationToken(ValidateVerificationTokenRequest request, ServerCallContext context)
        {
            try
            {
                var valid = await _jwtTokenService.ValidateTokenAsync(new ValidateRequest { Token = request.Token });

                return valid.IsTokenOk
                    ? VerificationReplyFactory.Success("Valid token")
                    : VerificationReplyFactory.Failed("Invalid or expired verification token.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return VerificationReplyFactory.Failed("Failed to validate verification token.");
            }
        }
    }
}
