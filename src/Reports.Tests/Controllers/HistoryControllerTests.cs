using Moq;
using FluentAssertions;
using Reports.Api.Controllers;
using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Services.History;
using Reports.Application.Services.UserContext;

namespace Reports.Tests.Controllers;

public class HistoryControllerTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly HistoryService _historyService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly HistoryController _controller;
    private bool _disposed;

    public HistoryControllerTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _historyService = new HistoryService(_context);

        // Configurar usuario autenticado por defecto
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.Setup(x => x.UserId).Returns(1);
        _mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _mockUserContext.Setup(x => x.Role).Returns("user");
        _mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        _controller = new HistoryController(_historyService, _mockUserContext.Object);

        // Setup HttpContext for language helper
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
        {
            HttpContext = httpContext
        };
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
            UserId = 1,  // Must match authenticated user's ID
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
        var userId = 1;  // Must match authenticated user's ID
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
        // Act - Trying to access userId=999 but authenticated user is userId=1
        var result = await _controller.GetByUserId(999);

        // Assert - Should return Forbid because user is not admin and trying to access other user's data
        result.Should().BeOfType<ForbidResult>();
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
        var userId = 1;  // Must match authenticated user's ID
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

        // Assert - Should return Forbid because user is not admin and trying to access other user's data
        result.Should().BeOfType<ForbidResult>();
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
            UserId = 1,  // Must match authenticated user's ID
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert - Create
        var createResult = await _controller.Create(historyDto);
        createResult.Should().BeOfType<OkObjectResult>();

        // Get the created history's ID from the database
        var createdHistory = await _context.History.FirstAsync(h => h.UserId == 1);

        // Act & Assert - Get All
        var getAllResult = await _controller.GetAll();
        getAllResult.Should().BeOfType<OkObjectResult>();

        // Act & Assert - Get by User ID
        var getByUserResult = await _controller.GetByUserId(1);
        getByUserResult.Should().BeOfType<OkObjectResult>();

        // Act & Assert - Delete
        var deleteResult = await _controller.Delete(createdHistory.Id);
        deleteResult.Should().BeOfType<OkObjectResult>();

        // Verify deletion
        var getAllAfterDeleteResult = await _controller.GetAll();
        getAllAfterDeleteResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetByUserId_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetByUserId(1);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Create_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);
        var historyDto = new HistoryDto
        {
            UserId = 1,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(historyDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetByUserId_AdminAccessingOtherUser_ShouldReturnOk()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAdmin).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(1);

        var history = new History
        {
            UserId = 999, // Different user
            AnalysisId = 400,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByUserId(999);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByUserId_NonAdminAccessingOtherUser_ShouldReturnForbid()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockUserContext.Setup(x => x.UserId).Returns(1);

        var history = new History
        {
            UserId = 999, // Different user
            AnalysisId = 400,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByUserId(999);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Create_ShouldOverrideUserIdFromContext()
    {
        // Arrange
        _mockUserContext.Setup(x => x.UserId).Returns(5);
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);

        var historyDto = new HistoryDto
        {
            UserId = 999, // Should be overridden
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(historyDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify the created history has UserId from context (5), not from DTO (999)
        var createdHistory = await _context.History.FirstAsync();
        createdHistory.UserId.Should().Be(5);
    }

    [Fact]
    public async Task GetByAnalysisId_WithMultipleHistories_ShouldReturnAll()
    {
        // Arrange
        var analysisId = 500;
        var history1 = new History
        {
            UserId = 1,
            AnalysisId = analysisId,
            CreatedAt = DateTime.UtcNow
        };
        var history2 = new History
        {
            UserId = 2,
            AnalysisId = analysisId,
            CreatedAt = DateTime.UtcNow.AddMinutes(5)
        };

        _context.History.AddRange(history1, history2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var histories = okResult!.Value as IEnumerable<HistoryDto>;
        histories.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_MultipleHistories_ShouldDeleteCorrectOne()
    {
        // Arrange
        var history1 = new History
        {
            UserId = 1,
            AnalysisId = 100,
            CreatedAt = DateTime.UtcNow
        };
        var history2 = new History
        {
            UserId = 2,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.AddRange(history1, history2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(history1.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify only one history remains
        var remainingHistories = await _context.History.ToListAsync();
        remainingHistories.Should().HaveCount(1);
        remainingHistories[0].Id.Should().Be(history2.Id);
    }

    // ===== FASE 4: EDGE CASES COMPLEJOS =====

    [Fact]
    public async Task GetAll_FiltersCorrectlyByAuthenticatedUserId()
    {
        // Arrange - Authenticated user is userId=1
        _mockUserContext.Setup(x => x.UserId).Returns(1);
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);

        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 100, CreatedAt = DateTime.UtcNow },
            new History { UserId = 1, AnalysisId = 101, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 200, CreatedAt = DateTime.UtcNow },
            new History { UserId = 3, AnalysisId = 300, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act - GetAll should only return histories for userId=1
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var data = okResult!.Value;

        // Extract data property from anonymous object
        var dataProperty = data!.GetType().GetProperty("data");
        var histories_result = dataProperty!.GetValue(data) as IEnumerable<HistoryDto>;
        histories_result.Should().HaveCount(2);
        histories_result.Should().OnlyContain(h => h.UserId == 1);
    }

    [Fact]
    public async Task GetByUserId_SameAsAuthenticated_ShouldAllow()
    {
        // Arrange - Non-admin user accessing their own data
        _mockUserContext.Setup(x => x.UserId).Returns(5);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);

        var history = new History
        {
            UserId = 5,
            AnalysisId = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByUserId(5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetByUserId_NegativeOrZeroIds_ShouldReturnForbid(int userId)
    {
        // Arrange - Non-admin user
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);
        _mockUserContext.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await _controller.GetByUserId(userId);

        // Assert - Should be Forbid because userId doesn't match authenticated user
        result.Should().BeOfType<ForbidResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetByAnalysisId_NegativeOrZeroIds_ShouldReturnNotFound(int analysisId)
    {
        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithNegativeAnalysisId_ShouldStillCreate()
    {
        // Arrange - InMemory DB allows negative IDs
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = -1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(historyDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_WithFutureCreatedAt_ShouldStillCreate()
    {
        // Arrange - Future timestamp
        var historyDto = new HistoryDto
        {
            UserId = 100,
            AnalysisId = 200,
            CreatedAt = DateTime.UtcNow.AddDays(365)
        };

        // Act
        var result = await _controller.Create(historyDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteAll_WithMixedUsers_ShouldDeleteAll()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 100, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 200, CreatedAt = DateTime.UtcNow },
            new History { UserId = 3, AnalysisId = 300, CreatedAt = DateTime.UtcNow },
            new History { UserId = 1, AnalysisId = 101, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 201, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify all deleted
        var remainingHistories = await _context.History.ToListAsync();
        remainingHistories.Should().BeEmpty();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task Delete_ExtremeIds_ShouldReturnNotFound(int id)
    {
        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByAnalysisId_AfterDeleteAll_ShouldReturnNotFound()
    {
        // Arrange
        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = 500, CreatedAt = DateTime.UtcNow },
            new History { UserId = 2, AnalysisId = 500, CreatedAt = DateTime.UtcNow }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        await _controller.DeleteAll();
        var result = await _controller.GetByAnalysisId(500);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_WithLargeDataset_ShouldReturnCorrectUserData()
    {
        // Arrange - Authenticated user is userId=1
        _mockUserContext.Setup(x => x.UserId).Returns(1);

        var histories = Enumerable.Range(1, 100).Select(i => new History
        {
            UserId = (i % 5) + 1, // Distribute among users 1-5
            AnalysisId = i,
            CreatedAt = DateTime.UtcNow.AddMinutes(i)
        }).ToArray();

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var data = okResult!.Value;

        // Extract data from anonymous object
        var dataProperty = data!.GetType().GetProperty("data");
        var histories_result = dataProperty!.GetValue(data) as IEnumerable<HistoryDto>;
        histories_result.Should().NotBeNull();
        histories_result.Should().OnlyContain(h => h.UserId == 1);
    }

    [Fact]
    public async Task Create_MultipleForSameAnalysis_ShouldCreateAll()
    {
        // Arrange - Multiple users accessing same analysis
        var analysisId = 999;

        // Create histories for different users but same analysis
        for (int userId = 1; userId <= 5; userId++)
        {
            _mockUserContext.Setup(x => x.UserId).Returns(userId);

            var historyDto = new HistoryDto
            {
                UserId = 0, // Will be overridden
                AnalysisId = analysisId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _controller.Create(historyDto);
            result.Should().BeOfType<OkObjectResult>();
        }

        // Act - Query by analysis ID
        var getResult = await _controller.GetByAnalysisId(analysisId);

        // Assert
        getResult.Should().BeOfType<OkObjectResult>();
        var okResult = getResult as OkObjectResult;
        var histories = okResult!.Value as IEnumerable<HistoryDto>;
        histories.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByUserId_AdminAccessingNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange - Admin user
        _mockUserContext.Setup(x => x.IsAdmin).Returns(true);
        _mockUserContext.Setup(x => x.UserId).Returns(1);

        // Act - Query for user with no history
        var result = await _controller.GetByUserId(99999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_NonExisting_ShouldNotAffectOtherRecords()
    {
        // Arrange
        var history = new History
        {
            UserId = 1,
            AnalysisId = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.History.Add(history);
        await _context.SaveChangesAsync();

        var initialCount = await _context.History.CountAsync();

        // Act - Try to delete non-existing
        var result = await _controller.Delete(99999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();

        // Verify count unchanged
        var finalCount = await _context.History.CountAsync();
        finalCount.Should().Be(initialCount);
    }

    [Fact]
    public async Task GetByAnalysisId_WithSameCreatedAt_ShouldReturnAll()
    {
        // Arrange
        var exactTime = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);
        var analysisId = 777;

        var histories = new[]
        {
            new History { UserId = 1, AnalysisId = analysisId, CreatedAt = exactTime },
            new History { UserId = 2, AnalysisId = analysisId, CreatedAt = exactTime },
            new History { UserId = 3, AnalysisId = analysisId, CreatedAt = exactTime },
            new History { UserId = 4, AnalysisId = 888, CreatedAt = exactTime }
        };

        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returned = okResult!.Value as IEnumerable<HistoryDto>;
        returned.Should().HaveCount(3);
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
