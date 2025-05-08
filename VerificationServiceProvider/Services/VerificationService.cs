using Grpc.Core;
using System.Diagnostics;
using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Models.Verification;

namespace VerificationServiceProvider.Services
{
    public class VerificationService(IVerificationEmailFactory emailFactory, IVerificationCacheHandler cacheHandler, ICodeGenerator codeGenerator) : VerificationContract.VerificationContractBase
    {
        private readonly IVerificationEmailFactory _emailFactory = emailFactory;
        private readonly IVerificationCacheHandler _cacheHandler = cacheHandler;
        private readonly ICodeGenerator _codeGenerator = codeGenerator;
        //private readonly EmailServiceProvider _emailService = emailService;

        public override async Task<VerificationReply> SendVerificationCode(SendVerificationCodeRequest request, ServerCallContext context)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    return VerificationReplyFactory.Failed("Email address is required.");

                var verificationCode = _codeGenerator.GenerateVerificationCode();
                var verificationToken = Guid.NewGuid().ToString();  //** Använda jwt servicen istället?
                var emailMessage = _emailFactory.Create(request.Email, verificationCode, verificationToken);

                //** Ersätta med EmailServiceProvider
                var emailSent = await _emailService.SendEmailAsync(emailMessage);
                if (!emailSent)
                    return VerificationReplyFactory.Failed("Failed to send verification email.");

                _cacheHandler.SaveVerificationCode(new SaveVerificationCodeModel
                {
                    Email = request.Email,
                    Code = verificationCode,
                    ValidFor = TimeSpan.FromMinutes(15)
                });

                _cacheHandler.SaveVerificationToken(new SaveVerificationTokenModel
                {
                    Email = request.Email,
                    Token = verificationToken,
                    ValidFor = TimeSpan.FromMinutes(15)
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
            try
            {
                var valid = _cacheHandler.ValidateCode(new CodeValidationModel { Email = request.Email, Code = request.Code });

                return Task.FromResult(valid
                    ? VerificationReplyFactory.Success()
                    : VerificationReplyFactory.Failed("Invalid or expired verification code."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Task.FromResult(VerificationReplyFactory.Failed("Failed to validate verification code."));
            }
        }

        public override Task<VerificationReply> ValidateVerificationToken(ValidateVerificationTokenRequest request, ServerCallContext context)
        {
            try
            {
                var valid = _cacheHandler.ValidateToken(new TokenValidationModel { Email = request.Email, Token = request.Token });

                return Task.FromResult(valid
                    ? VerificationReplyFactory.Success()
                    : VerificationReplyFactory.Failed("Invalid or expired verification token."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Task.FromResult(VerificationReplyFactory.Failed("Failed to validate verification token."));
            }
        }
    }
}
