using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reports.Api.Controllers;
using Reports.Application.Services.History;
using Reports.Application.Dtos;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;

namespace Reports.Tests.Controllers;

public class HistoryControllerTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly HistoryService _historyService;
    private readonly HistoryController _controller;
    private bool _disposed;

    public HistoryControllerTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _historyService = new HistoryService(_context);
        _controller = new HistoryController(_historyService);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAll_WithHistory_ShouldReturnOk()
    {
        // Arrange
        var history = new History
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserId_ExistingUserId_ShouldReturnOk()
    {
        // Arrange
        var userId = 300;
        var history = new History
        {
            UserId = userId,
            AnalysisId = 400,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByUserId(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByUserId_NonExistingUserId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetByUserId(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByAnalysisId_ExistingAnalysisId_ShouldReturnOk()
    {
        // Arrange
        var analysisId = 500;
        var history = new History
        {
            UserId = 600,
            AnalysisId = analysisId,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByAnalysisId_NonExistingAnalysisId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetByAnalysisId(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_ValidHistory_ShouldReturnOk()
    {
        // Arrange
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(historyDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ExistingHistory_ShouldReturnOk()
    {
        // Arrange
        var history = new History
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(history.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_NonExistingHistory_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAll_WithHistory_ShouldReturnOk()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 100, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 200, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteAll_WhenEmpty_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByUserId_MultipleHistory_ShouldReturnAll()
    {
        // Arrange
        var userId = 700;
        var histories = new[]
        {
            new History { UserId = userId, AnalysisId = 100, CreatedAt = DateTime.UtcNow },
            new History { UserId = userId, AnalysisId = 200, CreatedAt = DateTime.UtcNow },
            new History { UserId = 800, AnalysisId = 300, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByUserId(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByAnalysisId_MultipleHistory_ShouldReturnAll()
    {
        // Arrange
        var analysisId = 900;
        var histories = new[]
        {
            new History { UserId = 100, AnalysisId = analysisId, CreatedAt = DateTime.UtcNow },
            new History { UserId = 200, AnalysisId = analysisId, CreatedAt = DateTime.UtcNow },
            new History { UserId = 300, AnalysisId = 1000, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task GetByUserId_VariousIds_ShouldHandleGracefully(int userId)
    {
        // Act
        var result = await _controller.GetByUserId(userId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task GetByAnalysisId_VariousIds_ShouldHandleGracefully(int analysisId)
    {
        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Controller_ShouldHandleMultipleOperationsSequentially()
    {
        // Arrange
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert - Create
        var createResult = await _controller.Create(historyDto);
        createResult.Should().BeOfType<OkObjectResult>();

        // Get the created history's ID from the database
        var createdHistory = await _context.History.FirstAsync(h => h.UserId == 100);

        // Act & Assert - Get All
        var getAllResult = await _controller.GetAll();
        getAllResult.Should().BeOfType<OkObjectResult>();

        // Act & Assert - Get by User ID
        var getByUserResult = await _controller.GetByUserId(100);
        getByUserResult.Should().BeOfType<OkObjectResult>();

        // Act & Assert - Delete
        var deleteResult = await _controller.Delete(createdHistory.Id);
        deleteResult.Should().BeOfType<OkObjectResult>();

        // Verify deletion
        var getAllAfterDeleteResult = await _controller.GetAll();
        getAllAfterDeleteResult.Should().BeOfType<NotFoundObjectResult>();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
