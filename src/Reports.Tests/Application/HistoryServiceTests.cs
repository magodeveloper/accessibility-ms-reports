using FluentAssertions;
using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Services.History;

namespace Reports.Tests.Application;

public class HistoryServiceTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly HistoryService _service;
    private bool _disposed;

    public HistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _context.Database.EnsureCreated();
        _service = new HistoryService(_context);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleHistories_ShouldReturnAllHistories()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 200, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(h => h.UserId == 1);
        result.Should().Contain(h => h.UserId == 2);
    }

    [Fact]
    public async Task GetAllAsync_WithNoHistories_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUserId_ShouldReturnUserHistories()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 100, AnalysisId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 100, AnalysisId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 200, AnalysisId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByUserIdAsync(100);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(h => h.UserId == 100).Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistingUserId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetByUserIdAsync(9999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_WithExistingAnalysisId_ShouldReturnAnalysisHistories()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 500, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 500, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 3, AnalysisId = 600, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByAnalysisIdAsync(500);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(h => h.AnalysisId == 500).Should().BeTrue();
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_WithNonExistingAnalysisId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetByAnalysisIdAsync(9999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAndReturnHistory()
    {
        // Arrange
        var dto = new HistoryDto
        {
            UserId = 300,
            AnalysisId = 400
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be(300);
        result.AnalysisId.Should().Be(400);

        // Verify it was saved to database
        var savedHistory = await _context.History.FindAsync(result.Id);
        savedHistory.Should().NotBeNull();
        savedHistory!.UserId.Should().Be(300);
        savedHistory.AnalysisId.Should().Be(400);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteHistory()
    {
        // Arrange
        var history = new History
        {
            UserId = 1,
            AnalysisId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();
        var historyId = history.Id;

        // Act
        var result = await _service.DeleteAsync(historyId);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from database
        var deletedHistory = await _context.History.FindAsync(historyId);
        deletedHistory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllAsync_WithExistingHistories_ShouldDeleteAllHistories()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new History { UserId = 3, AnalysisId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAllAsync();

        // Assert
        result.Should().BeTrue();

        // Verify all histories were deleted
        var remainingHistories = await _context.History.ToListAsync();
        remainingHistories.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllAsync_WithNoHistories_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteAllAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Service_ShouldHandleMultipleConcurrentOperations()
    {
        // Arrange
        var tasks = new List<Task<HistoryDto>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            int userId = i;
            tasks.Add(_service.CreateAsync(new HistoryDto
            {
                UserId = userId,
                AnalysisId = userId * 10
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.All(r => r.Id > 0).Should().BeTrue();

        var allHistories = await _service.GetAllAsync();
        allHistories.Should().HaveCount(10);
    }

    [Fact]
    public async Task Service_ShouldHandleComplexFiltering()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var histories = new[]
        {
            new History { UserId = 100, AnalysisId = 1, CreatedAt = baseTime.AddHours(-2), UpdatedAt = baseTime.AddHours(-2) },
            new History { UserId = 100, AnalysisId = 2, CreatedAt = baseTime.AddHours(-1), UpdatedAt = baseTime.AddHours(-1) },
            new History { UserId = 200, AnalysisId = 1, CreatedAt = baseTime.AddHours(-3), UpdatedAt = baseTime.AddHours(-3) },
            new History { UserId = 200, AnalysisId = 3, CreatedAt = baseTime, UpdatedAt = baseTime }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var user100Histories = await _service.GetByUserIdAsync(100);
        var analysis1Histories = await _service.GetByAnalysisIdAsync(1);

        // Assert
        user100Histories.Should().HaveCount(2);
        user100Histories.All(h => h.UserId == 100).Should().BeTrue();

        analysis1Histories.Should().HaveCount(2);
        analysis1Histories.All(h => h.AnalysisId == 1).Should().BeTrue();
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
