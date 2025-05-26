using EmailServiceProvider;
using Grpc.Core;
using JwtTokenServiceProvider;
using Moq;
using VerificationServiceProvider;
using VerificationServiceProvider.Interfaces;
using VerificationServiceProvider.Models;
using VerificationServiceProvider.Services;

namespace Tests.Services
{
    // Tester genererade till stor del med hjälp av AI

    public class VerificationService_Tests
    {
        private readonly Mock<IVerificationEmailFactory> _emailFactoryMock = new();
        private readonly Mock<IVerificationCacheHandler> _cacheHandlerMock = new();
        private readonly Mock<ICodeGenerator> _codeGeneratorMock = new();
        private readonly Mock<IJwtTokenServiceClient> _jwtTokenServiceMock = new();
        private readonly Mock<IEmailServiceClient> _emailServiceMock = new();

        private readonly VerificationService _service;

        public VerificationService_Tests()
        {
            _service = new VerificationService(
                _emailFactoryMock.Object,
                _cacheHandlerMock.Object,
                _codeGeneratorMock.Object,
                _jwtTokenServiceMock.Object,
                _emailServiceMock.Object);
        }

        [Fact]
        public async Task SendVerificationCode_ReturnsSuccess_WhenAllStepsSucceed()
        {
            var email = "test@example.com";
            var code = "123456";
            var token = "generated-token";

            _codeGeneratorMock.Setup(x => x.GenerateVerificationCode()).Returns(code);
            _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<TokenRequest>()))
                .ReturnsAsync(new TokenReply { Succeeded = true, TokenMessage = token });
            _emailFactoryMock.Setup(x => x.CreateVerificationEmail(It.IsAny<VerificationEmailContentModel>()))
                .Returns(new EmailRequest());
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailRequest>()))
                .ReturnsAsync(new EmailReply { Succeeded = true });

            var request = new SendVerificationCodeRequest { Email = email };

            var result = await _service.SendVerificationCode(request, TestServerCallContext.Create());

            Assert.True(result.Succeeded);
            Assert.Equal("Verification email sent.", result.Message);
        }

        [Fact]
        public async Task SendVerificationCode_ReturnsFailed_WhenEmailMissing()
        {
            var result = await _service.SendVerificationCode(new SendVerificationCodeRequest { Email = "" }, TestServerCallContext.Create());

            Assert.False(result.Succeeded);
            Assert.Equal("Email address is required.", result.Error);
        }

        [Fact]
        public async Task SendVerificationCode_ReturnsFailed_WhenTokenGenerationFails()
        {
            _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<TokenRequest>()))
                .ReturnsAsync(new TokenReply { Succeeded = false });

            var result = await _service.SendVerificationCode(new SendVerificationCodeRequest { Email = "test@example.com" }, TestServerCallContext.Create());

            Assert.False(result.Succeeded);
            Assert.Equal("Failed to generate token.", result.Error);
        }

        [Fact]
        public async Task SendVerificationCode_ReturnsFailed_WhenEmailSendFails()
        {
            _codeGeneratorMock.Setup(x => x.GenerateVerificationCode()).Returns("123456");
            _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<TokenRequest>()))
                .ReturnsAsync(new TokenReply { Succeeded = true, TokenMessage = "token" });
            _emailFactoryMock.Setup(x => x.CreateVerificationEmail(It.IsAny<VerificationEmailContentModel>())).Returns(new EmailRequest());
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailRequest>()))
                .ReturnsAsync(new EmailReply { Succeeded = false });

            var result = await _service.SendVerificationCode(new SendVerificationCodeRequest { Email = "test@example.com" }, TestServerCallContext.Create());

            Assert.False(result.Succeeded);
            Assert.Equal("Failed to send verification email.", result.Error);
        }

        [Fact]
        public void ValidateVerificationCode_ReturnsFailed_WhenCodeIsInvalid()
        {
            _cacheHandlerMock.Setup(x => x.ValidateVerificationCode(It.IsAny<CodeValidationModel>())).Returns(false);

            var result = _service.ValidateVerificationCode(new ValidateVerificationCodeRequest
            {
                Email = "test@example.com",
                Code = "invalid"
            }, TestServerCallContext.Create()).Result;

            Assert.False(result.Succeeded);
            Assert.Equal("Invalid or expired verification code.", result.Error);
        }

        [Fact]
        public async Task ValidateVerificationCode_ReturnsSuccess_WhenCodeIsValid()
        {
            _cacheHandlerMock.Setup(x => x.ValidateVerificationCode(It.IsAny<CodeValidationModel>())).Returns(true);

            var result = await _service.ValidateVerificationCode(new ValidateVerificationCodeRequest
            {
                Email = "test@example.com",
                Code = "123456"
            }, TestServerCallContext.Create());

            Assert.True(result.Succeeded);
            Assert.Equal("Valid code.", result.Message);
        }

        [Theory]
        [InlineData("", "123456")]
        [InlineData("test@example.com", "")]
        public async Task ValidateVerificationCode_ReturnsFailed_WhenInputMissing(string email, string code)
        {
            var result = await _service.ValidateVerificationCode(new ValidateVerificationCodeRequest
            {
                Email = email,
                Code = code
            }, TestServerCallContext.Create());

            Assert.False(result.Succeeded);
            Assert.Equal("Missing required information.", result.Error);
        }

        [Fact]
        public async Task ValidateVerificationToken_ReturnsSuccess_WhenTokenIsValid()
        {
            var token = "valid-token";

            _jwtTokenServiceMock.Setup(x => x.ValidateTokenAsync(It.IsAny<ValidateRequest>()))
                .ReturnsAsync(new ValidateReply { IsTokenOk = true });

            var request = new ValidateVerificationTokenRequest { Token = token };

            var result = await _service.ValidateVerificationToken(request, TestServerCallContext.Create());

            Assert.True(result.Succeeded);
            Assert.Equal("Valid token", result.Message);
        }

        [Fact]
        public async Task ValidateVerificationToken_ReturnsFailed_WhenTokenIsInvalid()
        {
            _jwtTokenServiceMock.Setup(x => x.ValidateTokenAsync(It.IsAny<ValidateRequest>()))
                .ReturnsAsync(new ValidateReply { IsTokenOk = false });

            var result = await _service.ValidateVerificationToken(new ValidateVerificationTokenRequest { Token = "invalid" }, TestServerCallContext.Create());

            Assert.False(result.Succeeded);
            Assert.Equal("Invalid or expired verification token.", result.Error);
        }

        [Fact]
        public async Task ValidateVerificationToken_ReturnsFailed_WhenTokenIsMissing()
        {
            var result = await _service.ValidateVerificationToken(new ValidateVerificationTokenRequest { Token = "" }, TestServerCallContext.Create());

            Assert.False(result.Succeeded);
            Assert.Equal("Token is required.", result.Error);
        }
    }

    public class TestServerCallContext : ServerCallContext
    {
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null!;
        protected override string MethodCore => "TestMethod";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "Peer";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => new Metadata();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new AuthContext("Test", new Dictionary<string, List<AuthProperty>>());
        public static ServerCallContext Create() => new TestServerCallContext();
    }
}

