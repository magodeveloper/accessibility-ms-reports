using Xunit;

namespace Reports.Tests.UnitTests
{
    public class LocalizationMethodTests
    {
        [Fact]
        public void Get_Method_Should_Return_Localized_Message()
        {
            // Arrange
            var key = "Error_InternalServer";
            var lang = "es";

            // Act
            var result = Reports.Application.Localization.Get(key, lang);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Get_Method_Should_Handle_English_Language()
        {
            // Arrange
            var key = "Error_InternalServer";
            var lang = "en";

            // Act
            var result = Reports.Application.Localization.Get(key, lang);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Get_Method_Should_Handle_Invalid_Key()
        {
            // Arrange
            var key = "NonExistentKey";
            var lang = "es";

            // Act
            var result = Reports.Application.Localization.Get(key, lang);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Get_Method_Should_Handle_Null_Language()
        {
            // Arrange
            var key = "Error_InternalServer";
            string? lang = null;

            // Act
            var result = Reports.Application.Localization.Get(key, lang);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}