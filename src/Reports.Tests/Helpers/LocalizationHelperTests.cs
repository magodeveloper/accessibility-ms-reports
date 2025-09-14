using FluentAssertions;

namespace Reports.Tests.Helpers;

public class LocalizationHelperTests
{
    [Theory]
    [InlineData("Error_InternalServer", "es")]
    [InlineData("Error_InternalServer", "en")]
    [InlineData("Success_ReportCreated", "es")]
    [InlineData("Success_ReportCreated", "en")]
    [InlineData("Error_ReportNotFound", "es")]
    [InlineData("Error_ReportNotFound", "en")]
    public void Get_ShouldReturnLocalizedString_ForValidKeys(string key, string language)
    {
        // Act
        var result = Reports.Application.Localization.Get(key, language);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe(key); // Should return actual localized text, not the key
    }

    [Theory]
    [InlineData("NonExistentKey", "es")]
    [InlineData("NonExistentKey", "en")]
    [InlineData("", "es")]
    public void Get_ShouldHandleInvalidKeys_Gracefully(string key, string language)
    {
        // Act
        var result = Reports.Application.Localization.Get(key, language);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Error_InternalServer", "fr")]
    [InlineData("Error_InternalServer", "de")]
    [InlineData("Error_InternalServer", "")]
    public void Get_ShouldHandleUnsupportedLanguages_Gracefully(string key, string language)
    {
        // Act
        var result = Reports.Application.Localization.Get(key, language);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }
}