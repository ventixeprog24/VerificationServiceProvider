using Grpc.Core;
using JwtTokenServiceProvider;
using System.Diagnostics;
using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Models;
using EmailServiceClient = EmailServiceProvider.EmailServicer.EmailServicerClient;
using JwtTokenServiceClient = JwtTokenServiceProvider.JwtTokenServiceContract.JwtTokenServiceContractClient;

namespace VerificationServiceProvider.Services
{
    public class VerificationService(IVerificationEmailFactory emailFactory, IVerificationCacheHandler cacheHandler, ICodeGenerator codeGenerator, JwtTokenServiceClient jwtTokenService, EmailServiceClient emailService) : VerificationContract.VerificationContractBase
    {
        private readonly IVerificationEmailFactory _emailFactory = emailFactory;
        private readonly IVerificationCacheHandler _cacheHandler = cacheHandler;
        private readonly ICodeGenerator _codeGenerator = codeGenerator;
        private readonly JwtTokenServiceClient _jwtTokenService = jwtTokenService;
        private readonly EmailServiceClient _emailService = emailService;

        public override async Task<VerificationReply> SendVerificationCode(SendVerificationCodeRequest request, ServerCallContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return VerificationReplyFactory.Failed("Email address is required.");

            try
            {
                var code = _codeGenerator.GenerateVerificationCode();

                Console.WriteLine($"[DEBUG] Email before token request: {request.Email}");

                var tokenReply = await _jwtTokenService.GenerateTokenAsync(new TokenRequest { Email = request.Email });

                Console.WriteLine($"[DEBUG] TokenReply.Succeeded: {tokenReply.Succeeded}");
                Console.WriteLine($"[DEBUG] TokenReply.Message: {tokenReply.TokenMessage}");

                if (!tokenReply.Succeeded)
                    return VerificationReplyFactory.Failed(tokenReply.TokenMessage);

                var emailMessage = _emailFactory.Create(new VerificationEmailContentModel
                {
                    Email = request.Email,
                    Code = code,
                    Token = tokenReply.TokenMessage,
                });

                var emailSent = await _emailService.SendEmailAsync(emailMessage);
                if (!emailSent.Succeeded)
                    return VerificationReplyFactory.Failed("Failed to send verification email.");

                _cacheHandler.SaveVerificationCode(new SaveVerificationCodeModel
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
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return Task.FromResult(VerificationReplyFactory.Failed("Missing required information."));

            try
            {
                var valid = _cacheHandler.ValidateVerificationCode(new CodeValidationModel { Email = request.Email, Code = request.Code });

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
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
                return VerificationReplyFactory.Failed("Token is required.");

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
