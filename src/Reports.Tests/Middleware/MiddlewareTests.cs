using Moq;
using Xunit;
using System.IO;
using System.Text.Json;
using FluentAssertions;
using Reports.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Reports.Application.Services.UserContext;

namespace Reports.Tests.Middleware;

/// <summary>
/// Tests unitarios para los Middlewares del microservicio Reports
/// </summary>
public class MiddlewareTests
{
    #region GatewaySecretValidationMiddleware Tests

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithValidSecret_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object, environment.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "test-secret-123";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithMissingSecret_Returns403()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object, environment.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("Forbidden");
        responseBody.Should().Contain("Direct access to microservice is not allowed");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WithInvalidSecret_Returns403()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Gateway:Secret"]).Returns("test-secret-123");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object, environment.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "wrong-secret";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("Invalid Gateway secret");
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_WhenSecretNotConfigured_SkipsValidation()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Gateway:Secret"]).Returns((string?)null);
        configuration.Setup(c => c["GATEWAY_SECRET"]).Returns((string?)null);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Development");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object, environment.Object);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GatewaySecretValidationMiddleware_ReadsSecretFromAlternativeConfig()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<GatewaySecretValidationMiddleware>>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Gateway:Secret"]).Returns((string?)null);
        configuration.Setup(c => c["GATEWAY_SECRET"]).Returns("env-secret-456");

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Production");

        var middleware = new GatewaySecretValidationMiddleware(next, logger, configuration.Object, environment.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Gateway-Secret"] = "env-secret-456";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    #endregion

    #region UserContextMiddleware Tests

    [Fact]
    public async Task UserContextMiddleware_WithValidHeaders_PopulatesUserContext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "123";
        context.Request.Headers["X-User-Email"] = "test@example.com";
        context.Request.Headers["X-User-Role"] = "Admin";
        context.Request.Headers["X-User-Name"] = "Test User";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        nextCalled.Should().BeTrue();
        userContext.UserId.Should().Be(123);
        userContext.Email.Should().Be("test@example.com");
        userContext.Role.Should().Be("Admin");
        userContext.UserName.Should().Be("Test User");
    }

    [Fact]
    public async Task UserContextMiddleware_WithMissingHeaders_LeavesUserContextEmpty()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        nextCalled.Should().BeTrue();
        userContext.UserId.Should().Be(0);
        userContext.Email.Should().Be(string.Empty);
        userContext.Role.Should().Be(string.Empty);
        userContext.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WithInvalidUserId_DoesNotPopulateContext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "invalid-id";
        context.Request.Headers["X-User-Email"] = "test@example.com";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        nextCalled.Should().BeTrue();
        userContext.UserId.Should().Be(0);
    }

    [Fact]
    public async Task UserContextMiddleware_WithPartialHeaders_PopulatesAvailableFields()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "456";
        context.Request.Headers["X-User-Email"] = "partial@example.com";
        // No se incluyen X-User-Role ni X-User-Name

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        nextCalled.Should().BeTrue();
        userContext.UserId.Should().Be(456);
        userContext.Email.Should().Be("partial@example.com");
        userContext.Role.Should().Be(string.Empty);
        userContext.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WithEmptyHeaders_UsesEmptyStrings()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();
        var userContext = new UserContext();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "789";
        context.Request.Headers["X-User-Email"] = "";
        context.Request.Headers["X-User-Role"] = "";
        context.Request.Headers["X-User-Name"] = "";

        // Act
        await middleware.InvokeAsync(context, userContext);

        // Assert
        nextCalled.Should().BeTrue();
        userContext.UserId.Should().Be(789);
        userContext.Email.Should().Be(string.Empty);
        userContext.Role.Should().Be(string.Empty);
        userContext.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task UserContextMiddleware_WhenExceptionOccurs_ContinuesProcessing()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<UserContextMiddleware>>();

        // Crear un mock de IUserContext que lance excepción
        var mockUserContext = new Mock<IUserContext>();
        var middleware = new UserContextMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "123";

        // Act
        await middleware.InvokeAsync(context, mockUserContext.Object);

        // Assert
        // El middleware debe continuar incluso si hay error
        nextCalled.Should().BeTrue();
    }

    #endregion

    #region Extension Methods Tests

    [Fact]
    public void UseGatewaySecretValidation_RegistersMiddleware()
    {
        // Arrange
        var appBuilder = new Mock<IApplicationBuilder>();
        appBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appBuilder.Object);

        // Act
        var result = appBuilder.Object.UseGatewaySecretValidation();

        // Assert
        result.Should().NotBeNull();
        appBuilder.Verify(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    [Fact]
    public void UseUserContext_RegistersMiddleware()
    {
        // Arrange
        var appBuilder = new Mock<IApplicationBuilder>();
        appBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appBuilder.Object);

        // Act
        var result = appBuilder.Object.UseUserContext();

        // Assert
        result.Should().NotBeNull();
        appBuilder.Verify(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    #endregion
}
