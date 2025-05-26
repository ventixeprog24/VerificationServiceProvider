using Microsoft.Extensions.Options;
using VerificationServiceProvider.Factories;
using VerificationServiceProvider.Models;

namespace Tests.Factories
{
    // Tester genererade till stor del med hjälp av AI

    public class VerificationEmailFactory_Tests
    {
        private readonly VerificationEmailFactory _factory;
        private readonly EmailVerificationOptions _options = new()
        {
            SenderAddress = "no-reply@example.com",
            FrontendUrl = "https://frontend.example.com",
            VerificationPath = "/verify",
            DomainUrl = "example.com"
        };

        public VerificationEmailFactory_Tests()
        {
            var optionsMock = Options.Create(_options);
            _factory = new VerificationEmailFactory(optionsMock);
        }

        [Fact]
        public void Create_ShouldReturnEmailRequestWithCorrectProperties()
        {
            // Arrange
            var model = new VerificationEmailContentModel
            {
                Email = "user@example.com",
                Code = "123456",
                Token = "token123"
            };

            var expectedUrl = $"{_options.FrontendUrl}{_options.VerificationPath}?email={model.Email}&token={model.Token}";

            // Act
            var emailRequest = _factory.CreateVerificationEmail(model);

            // Assert
            Assert.NotNull(emailRequest);
            Assert.Single(emailRequest.Recipients);
            Assert.Equal(model.Email, emailRequest.Recipients[0]);
            Assert.Equal(_options.SenderAddress, emailRequest.SenderAddress);
            Assert.Equal($"Verification code: {model.Code}", emailRequest.Subject);

            // Assert: Validate plain text content
            Assert.Contains(model.Code, emailRequest.PlainText);
            Assert.Contains(model.Email, emailRequest.PlainText);
            Assert.Contains(model.Token, emailRequest.PlainText);
            Assert.Contains(_options.FrontendUrl, emailRequest.PlainText);
            Assert.Contains(_options.VerificationPath, emailRequest.PlainText);
            Assert.Contains(_options.DomainUrl, emailRequest.PlainText);
            Assert.Contains(expectedUrl, emailRequest.PlainText); 

            // Assert: Ensure the verification link is correctly embedded in the HTML
            Assert.Contains(model.Code, emailRequest.Html);
            Assert.Contains(model.Email, emailRequest.Html);
            Assert.Contains(model.Token, emailRequest.Html);
            Assert.Contains(_options.FrontendUrl, emailRequest.Html);
            Assert.Contains(_options.VerificationPath, emailRequest.Html);
            Assert.Contains(_options.DomainUrl, emailRequest.Html);

            // Assert: Ensure code block styling is present
            var expectedAnchor = $"<a href='{expectedUrl}'";
            Assert.Contains(expectedAnchor, emailRequest.Html);
            Assert.Contains($"<div style='display: flex; justify-content: center;", emailRequest.Html);
            Assert.Contains(model.Code, emailRequest.Html);
        }

        [Fact]
        public void Create_ShouldThrowArgumentNullException_WhenModelIsNull()
        {
            // Arrange
            VerificationEmailContentModel? model = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _factory.CreateVerificationEmail(model!));
            Assert.Equal("model", exception.ParamName);
        }
    }
}
