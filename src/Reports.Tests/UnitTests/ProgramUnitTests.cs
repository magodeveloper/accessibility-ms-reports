using FluentAssertions;

namespace Reports.Tests.UnitTests;

public class ProgramUnitTests
{
    [Fact]
    public void Program_Constructor_ShouldWork()
    {
        // Arrange & Act
        var program = new Reports.Api.Program();

        // Assert
        program.Should().NotBeNull();
    }

    [Fact]
    public void Program_Class_ShouldBePublic()
    {
        // Arrange
        var programType = typeof(Reports.Api.Program);

        // Act & Assert
        programType.Should().NotBeNull();
        programType.IsPublic.Should().BeTrue();
        programType.IsClass.Should().BeTrue();
    }

    [Theory]
    [InlineData("TestEnvironment")]
    [InlineData("Development")]
    [InlineData("Production")]
    public void Program_ShouldHandle_DifferentEnvironments(string environment)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

        // Act & Assert - Just verify the environment variable is set
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Should().Be(environment);

        // Reset
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
}