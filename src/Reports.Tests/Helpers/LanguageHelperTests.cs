using Moq;
using Xunit;
using Reports.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Reports.Tests.Helpers
{
    public class LanguageHelperTests
    {
        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsNull_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues((string?)null));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsEmpty_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues(""));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsWhitespace_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("   "));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsSpanish_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("es"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsEnglish_ReturnsEn()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("en"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsSpanishWithRegion_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("es-ES"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderIsEnglishWithRegion_ReturnsEn()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("en-US"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderHasMultipleLanguages_ReturnsFirstSupported()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("en-US,es;q=0.9,fr;q=0.8"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderHasUnsupportedLanguage_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("fr"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenAcceptLanguageHeaderHasInvalidFormat_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("x"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Theory]
        [InlineData("es", "es")]
        [InlineData("en", "en")]
        [InlineData("ES", "es")]
        [InlineData("EN", "en")]
        [InlineData("es-ES", "es")]
        [InlineData("en-US", "en")]
        [InlineData("es-MX", "es")]
        [InlineData("en-GB", "en")]
        public void GetRequestLanguage_WithValidLanguageCodes_ReturnsCorrectLanguage(string input, string expected)
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues(input));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("fr")]
        [InlineData("de")]
        [InlineData("it")]
        [InlineData("pt")]
        [InlineData("zh")]
        [InlineData("ja")]
        [InlineData("ko")]
        [InlineData("ru")]
        public void GetRequestLanguage_WithUnsupportedLanguages_ReturnsEs(string language)
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues(language));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WhenExceptionThrown_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Throws(new Exception("Test exception"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WithComplexAcceptLanguageHeader_ReturnsFirstValidLanguage()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("fr-FR,fr;q=0.9,en-US;q=0.8,en;q=0.7,es;q=0.6"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result); // First supported language after fr
        }

        [Fact]
        public void GetRequestLanguage_WithSpacesInHeader_HandlesCorrectly()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("  es-ES  "));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WithSingleCharacterToken_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues("e"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WithExceptionInProcessing_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            // Simular una excepción durante el procesamiento
            mockHeaders.Setup(h => h["Accept-Language"]).Throws<InvalidOperationException>();
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }

        [Fact]
        public void GetRequestLanguage_WithMalformedHeader_ReturnsEs()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            mockHeaders.Setup(h => h["Accept-Language"]).Returns(new StringValues(";;;"));
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

            // Act
            var result = LanguageHelper.GetRequestLanguage(mockRequest.Object);

            // Assert
            Assert.Equal("es", result);
        }
    }
}
