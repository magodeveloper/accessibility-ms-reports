using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Services.History;
using Reports.Application.Dtos;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;

namespace Reports.Tests.UnitTests.Services;

public class HistoryServiceTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly HistoryService _historyService;
    private bool _disposed;

    public HistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _historyService = new HistoryService(_context);
    }

    [Fact]
    public async Task CreateAsync_ValidHistory_ShouldCreateSuccessfully()
    {
        // Arrange
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _historyService.CreateAsync(historyDto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(100);
        result.AnalysisId.Should().Be(200);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        var result = await _historyService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithHistory_ShouldReturnAllHistory()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 100, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 200, CreatedAt = DateTime.UtcNow },
            new History { UserId = 3, AnalysisId = 300, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _historyService.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(h => h.UserId == 1);
        result.Should().Contain(h => h.UserId == 2);
        result.Should().Contain(h => h.UserId == 3);
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUserId_ShouldReturnHistory()
    {
        // Arrange
        var userId = 100;
        var histories = new[]
        {
            new History { UserId = userId, AnalysisId = 1, CreatedAt = DateTime.UtcNow },
            new History { UserId = userId, AnalysisId = 2, CreatedAt = DateTime.UtcNow },
            new History { UserId = 200, AnalysisId = 3, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _historyService.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_NonExistingUserId_ShouldReturnEmpty()
    {
        // Act
        var result = await _historyService.GetByUserIdAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_ExistingAnalysisId_ShouldReturnHistory()
    {
        // Arrange
        var analysisId = 100;
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = analysisId, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = analysisId, CreatedAt = DateTime.UtcNow },
            new History { UserId = 3, AnalysisId = 200, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _historyService.GetByAnalysisIdAsync(analysisId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.AnalysisId == analysisId);
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_NonExistingAnalysisId_ShouldReturnEmpty()
    {
        // Act
        var result = await _historyService.GetByAnalysisIdAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ExistingHistory_ShouldReturnTrue()
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
        var result = await _historyService.DeleteAsync(history.Id);

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var deletedHistory = await _context.History.FindAsync(history.Id);
        deletedHistory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingHistory_ShouldReturnFalse()
    {
        // Act
        var result = await _historyService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllAsync_WithHistory_ShouldReturnTrue()
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
        var result = await _historyService.DeleteAllAsync();

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var remainingHistory = await _context.History.ToListAsync();
        remainingHistory.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllAsync_WhenEmpty_ShouldReturnFalse()
    {
        // Act
        var result = await _historyService.DeleteAllAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithCreatedAt_ShouldMaintainTimestamp()
    {
        // Arrange
        var specificDate = new DateTime(2024, 12, 19, 15, 30, 45, DateTimeKind.Utc);
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = specificDate
        };

        // Act
        var result = await _historyService.CreateAsync(historyDto);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(specificDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Service_ShouldHandleMultipleOperationsSequentially()
    {
        // Arrange
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert - Create
        var created = await _historyService.CreateAsync(historyDto);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);

        // Act & Assert - Get All
        var allHistory = await _historyService.GetAllAsync();
        allHistory.Should().HaveCount(1);

        // Act & Assert - Get by User ID
        var historyByUser = await _historyService.GetByUserIdAsync(100);
        historyByUser.Should().HaveCount(1);

        // Act & Assert - Get by Analysis ID
        var historyByAnalysis = await _historyService.GetByAnalysisIdAsync(200);
        historyByAnalysis.Should().HaveCount(1);

        // Act & Assert - Delete
        var deleted = await _historyService.DeleteAsync(created.Id);
        deleted.Should().BeTrue();

        // Verify deletion
        var finalHistory = await _historyService.GetAllAsync();
        finalHistory.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_MultipleHistoriesForSameUser_ShouldCreateAll()
    {
        // Arrange
        var userId = 100;
        var histories = new[]
        {
            new HistoryDto { UserId = userId, AnalysisId = 1, CreatedAt = DateTime.UtcNow },
            new HistoryDto { UserId = userId, AnalysisId = 2, CreatedAt = DateTime.UtcNow.AddMinutes(5) },
            new HistoryDto { UserId = userId, AnalysisId = 3, CreatedAt = DateTime.UtcNow.AddMinutes(10) }
        };

        // Act
        var results = new List<HistoryDto>();
        foreach (var history in histories)
        {
            var result = await _historyService.CreateAsync(history);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(h => h.UserId == userId);

        var allUserHistory = await _historyService.GetByUserIdAsync(userId);
        allUserHistory.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(999, 888)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public async Task CreateAsync_WithVariousIds_ShouldCreateSuccessfully(int userId, int analysisId)
    {
        // Arrange
        var historyDto = new HistoryDto
        {
            UserId = userId,
            AnalysisId = analysisId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _historyService.CreateAsync(historyDto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.AnalysisId.Should().Be(analysisId);
        result.Id.Should().BeGreaterThan(0);
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
