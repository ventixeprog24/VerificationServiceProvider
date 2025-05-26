using Microsoft.Extensions.Caching.Memory;
using Moq;
using VerificationServiceProvider.Helpers;
using VerificationServiceProvider.Models;

namespace Tests.Helpers
{
    // Tester genererade till stor del med hjälp av AI

    public class VerificationCacheHandler_Tests
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly VerificationCacheHandler _handler;

        public VerificationCacheHandler_Tests()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _handler = new VerificationCacheHandler(_cacheMock.Object);
        }

        [Fact]
        public void SaveVerificationCode_ShouldCallCacheSet_WithCorrectParameters()
        {
            // Arrange
            var model = new SaveVerificationCodeModel
            {
                Email = "Test@Email.com",
                Code = "123456",
                ValidFor = TimeSpan.FromMinutes(5)
            };

            var cacheEntryMock = new Mock<ICacheEntry>();

            _cacheMock
                .Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            // Act
            _handler.SaveVerificationCode(model);

            // Assert
            var expectedKey = model.Email.ToLowerInvariant();

            _cacheMock.Verify(c => c.CreateEntry(expectedKey), Times.Once);
            cacheEntryMock.VerifySet(e => e.Value = model.Code, Times.Once);
            cacheEntryMock.VerifySet(e => e.AbsoluteExpirationRelativeToNow = model.ValidFor, Times.Once);
        }

        [Fact]
        public void ValidateVerificationCode_ShouldReturnTrueAndRemoveCache_WhenCodeMatches()
        {
            // Arrange
            var email = "user@example.com";
            var code = "code123";
            var key = email.ToLowerInvariant();

            object cachedCode = code;

            _cacheMock.Setup(c => c.TryGetValue(key, out cachedCode!))
                .Returns(true);

            _cacheMock.Setup(c => c.Remove(key));

            var model = new CodeValidationModel { Email = email, Code = code };

            // Act
            var result = _handler.ValidateVerificationCode(model);

            // Assert
            Assert.True(result);
            _cacheMock.Verify(c => c.Remove(key), Times.Once);
        }

        [Fact]
        public void ValidateVerificationCode_ShouldReturnFalse_WhenCodeDoesNotMatch()
        {
            // Arrange
            var email = "user@example.com";
            var storedCode = "code123";
            var attemptedCode = "wrongcode";
            var key = email.ToLowerInvariant();

            object cachedCode = storedCode;

            _cacheMock.Setup(c => c.TryGetValue(key, out cachedCode!))
                .Returns(true);

            var model = new CodeValidationModel { Email = email, Code = attemptedCode };

            // Act
            var result = _handler.ValidateVerificationCode(model);

            // Assert
            Assert.False(result);
            _cacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void ValidateVerificationCode_ShouldReturnFalse_WhenKeyNotFound()
        {
            // Arrange
            var email = "user@example.com";
            var key = email.ToLowerInvariant();

            object cachedCode = null!;

            _cacheMock.Setup(c => c.TryGetValue(key, out cachedCode!))
                .Returns(false);

            var model = new CodeValidationModel { Email = email, Code = "anycode" };

            // Act
            var result = _handler.ValidateVerificationCode(model);

            // Assert
            Assert.False(result);
            _cacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Never);
        }
    }
}
