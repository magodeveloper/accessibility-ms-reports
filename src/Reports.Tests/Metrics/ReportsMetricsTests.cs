using Prometheus;
using FluentAssertions;
using Reports.Api.Metrics;

namespace Reports.Tests.Metrics;

public class ReportsMetricsTests
{
    // Para evitar colisiones entre tests, usamos un fixture que limpia las métricas
    public ReportsMetricsTests()
    {
        // Las métricas de Prometheus son singleton, por lo que necesitamos
        // trabajar con valores incrementales en los tests
    }

    [Fact]
    public void ReportsGeneratedTotal_ShouldBeCounter()
    {
        // Assert
        ReportsMetrics.ReportsGeneratedTotal.Should().NotBeNull();
        ReportsMetrics.ReportsGeneratedTotal.Should().BeOfType<Counter>();
    }

    [Fact]
    public void ReportsQueriesTotal_ShouldBeCounter()
    {
        // Assert
        ReportsMetrics.ReportsQueriesTotal.Should().NotBeNull();
        ReportsMetrics.ReportsQueriesTotal.Should().BeOfType<Counter>();
    }

    [Fact]
    public void ReportsDeletionsTotal_ShouldBeCounter()
    {
        // Assert
        ReportsMetrics.ReportsDeletionsTotal.Should().NotBeNull();
        ReportsMetrics.ReportsDeletionsTotal.Should().BeOfType<Counter>();
    }

    [Fact]
    public void HistoryAccessTotal_ShouldBeCounter()
    {
        // Assert
        ReportsMetrics.HistoryAccessTotal.Should().NotBeNull();
        ReportsMetrics.HistoryAccessTotal.Should().BeOfType<Counter>();
    }

    [Fact]
    public void ReportGenerationDuration_ShouldBeHistogram()
    {
        // Assert
        ReportsMetrics.ReportGenerationDuration.Should().NotBeNull();
        ReportsMetrics.ReportGenerationDuration.Should().BeOfType<Histogram>();
    }

    [Fact]
    public void ReportsQueryDuration_ShouldBeHistogram()
    {
        // Assert
        ReportsMetrics.ReportsQueryDuration.Should().NotBeNull();
        ReportsMetrics.ReportsQueryDuration.Should().BeOfType<Histogram>();
    }

    [Fact]
    public void ReportsSizeBytes_ShouldBeGauge()
    {
        // Assert
        ReportsMetrics.ReportsSizeBytes.Should().NotBeNull();
        ReportsMetrics.ReportsSizeBytes.Should().BeOfType<Gauge>();
    }

    [Fact]
    public void ReportsByFormat_ShouldBeGauge()
    {
        // Assert
        ReportsMetrics.ReportsByFormat.Should().NotBeNull();
        ReportsMetrics.ReportsByFormat.Should().BeOfType<Gauge>();
    }

    [Theory]
    [InlineData("pdf", true)]
    [InlineData("html", true)]
    [InlineData("json", true)]
    [InlineData("excel", true)]
    [InlineData("PDF", false)]
    [InlineData("HTML", false)]
    public void RecordReportGenerated_WithDifferentFormatsAndStatus_ShouldIncrementCounter(string format, bool success)
    {
        // Arrange
        var counterBefore = GetCounterValue(ReportsMetrics.ReportsGeneratedTotal, format.ToLower(), success ? "success" : "failure");

        // Act
        ReportsMetrics.RecordReportGenerated(format, success);

        // Assert
        var counterAfter = GetCounterValue(ReportsMetrics.ReportsGeneratedTotal, format.ToLower(), success ? "success" : "failure");
        counterAfter.Should().BeGreaterThan(counterBefore);
    }

    [Theory]
    [InlineData("get_all")]
    [InlineData("get_by_analysis")]
    [InlineData("get_by_format")]
    [InlineData("get_by_date")]
    public void RecordQuery_WithDifferentOperations_ShouldIncrementCounter(string operation)
    {
        // Arrange
        var counterBefore = GetCounterValue(ReportsMetrics.ReportsQueriesTotal, operation);

        // Act
        ReportsMetrics.RecordQuery(operation);

        // Assert
        var counterAfter = GetCounterValue(ReportsMetrics.ReportsQueriesTotal, operation);
        counterAfter.Should().BeGreaterThan(counterBefore);
    }

    [Fact]
    public void ReportsDeletionsTotal_WhenIncremented_ShouldIncrease()
    {
        // Arrange
        var counterBefore = GetCounterValueNoLabels(ReportsMetrics.ReportsDeletionsTotal);

        // Act
        ReportsMetrics.ReportsDeletionsTotal.Inc();

        // Assert
        var counterAfter = GetCounterValueNoLabels(ReportsMetrics.ReportsDeletionsTotal);
        counterAfter.Should().BeGreaterThan(counterBefore);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("456")]
    [InlineData("789")]
    public void HistoryAccessTotal_WithDifferentUserIds_ShouldIncrementCounter(string userId)
    {
        // Arrange
        var counterBefore = GetCounterValue(ReportsMetrics.HistoryAccessTotal, userId);

        // Act
        ReportsMetrics.HistoryAccessTotal.WithLabels(userId).Inc();

        // Assert
        var counterAfter = GetCounterValue(ReportsMetrics.HistoryAccessTotal, userId);
        counterAfter.Should().BeGreaterThan(counterBefore);
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData("html")]
    [InlineData("json")]
    [InlineData("excel")]
    public void MeasureReportGeneration_WithFormat_ShouldReturnDisposableTimer(string format)
    {
        // Act
        var timer = ReportsMetrics.MeasureReportGeneration(format);

        // Assert
        timer.Should().NotBeNull();
        timer.Should().BeAssignableTo<IDisposable>();

        // Dispose to complete measurement
        timer.Dispose();
    }

    [Theory]
    [InlineData("get_all")]
    [InlineData("get_by_analysis")]
    public void MeasureQuery_WithOperation_ShouldReturnDisposableTimer(string operation)
    {
        // Act
        var timer = ReportsMetrics.MeasureQuery(operation);

        // Assert
        timer.Should().NotBeNull();
        timer.Should().BeAssignableTo<IDisposable>();

        // Dispose to complete measurement
        timer.Dispose();
    }

    [Fact]
    public async Task MeasureReportGeneration_WhenDisposed_ShouldRecordHistogram()
    {
        // Arrange
        var format = "pdf";
        var histogramBefore = GetHistogramCount(ReportsMetrics.ReportGenerationDuration, format);

        // Act
        using (ReportsMetrics.MeasureReportGeneration(format))
        {
            // Simulate some work
            await Task.Delay(10);
        }

        // Assert
        var histogramAfter = GetHistogramCount(ReportsMetrics.ReportGenerationDuration, format);
        histogramAfter.Should().BeGreaterThan(histogramBefore);
    }

    [Fact]
    public async Task MeasureQuery_WhenDisposed_ShouldRecordHistogram()
    {
        // Arrange
        var operation = "get_all";
        var histogramBefore = GetHistogramCount(ReportsMetrics.ReportsQueryDuration, operation);

        // Act
        using (ReportsMetrics.MeasureQuery(operation))
        {
            // Simulate some work
            await Task.Delay(5);
        }

        // Assert
        var histogramAfter = GetHistogramCount(ReportsMetrics.ReportsQueryDuration, operation);
        histogramAfter.Should().BeGreaterThan(histogramBefore);
    }

    [Fact]
    public void ReportsSizeBytes_WhenSetAndIncremented_ShouldUpdateValue()
    {
        // Act
        ReportsMetrics.ReportsSizeBytes.Set(1024);
        var valueAfterSet = GetGaugeValueNoLabels(ReportsMetrics.ReportsSizeBytes);

        ReportsMetrics.ReportsSizeBytes.Inc(512);
        var valueAfterInc = GetGaugeValueNoLabels(ReportsMetrics.ReportsSizeBytes);

        // Assert
        valueAfterSet.Should().Be(1024);
        valueAfterInc.Should().Be(1536);
    }

    [Theory]
    [InlineData("pdf", 10)]
    [InlineData("html", 20)]
    [InlineData("json", 15)]
    public void ReportsByFormat_WithDifferentFormats_ShouldSetCorrectValues(string format, double count)
    {
        // Act
        ReportsMetrics.ReportsByFormat.WithLabels(format).Set(count);

        // Assert
        var value = GetGaugeValue(ReportsMetrics.ReportsByFormat, format);
        value.Should().Be(count);
    }

    [Fact]
    public void RecordReportGenerated_WithUpperCaseFormat_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var format = "PDF";
        var counterBefore = GetCounterValue(ReportsMetrics.ReportsGeneratedTotal, "pdf", "success");

        // Act
        ReportsMetrics.RecordReportGenerated(format, success: true);

        // Assert - should be stored as lowercase
        var counterAfter = GetCounterValue(ReportsMetrics.ReportsGeneratedTotal, "pdf", "success");
        counterAfter.Should().BeGreaterThan(counterBefore);
    }

    [Fact]
    public async Task MeasureReportGeneration_WithUpperCaseFormat_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var format = "JSON";
        var histogramBefore = GetHistogramCount(ReportsMetrics.ReportGenerationDuration, "json");

        // Act
        using (ReportsMetrics.MeasureReportGeneration(format))
        {
            await Task.Delay(5);
        }

        // Assert
        var histogramAfter = GetHistogramCount(ReportsMetrics.ReportGenerationDuration, "json");
        histogramAfter.Should().BeGreaterThan(histogramBefore);
    }

    [Fact]
    public void RecordReportGenerated_MultipleCallsSameFormat_ShouldAccumulate()
    {
        // Arrange
        var format = "excel";
        var counterBefore = GetCounterValue(ReportsMetrics.ReportsGeneratedTotal, format, "success");

        // Act
        ReportsMetrics.RecordReportGenerated(format, success: true);
        ReportsMetrics.RecordReportGenerated(format, success: true);
        ReportsMetrics.RecordReportGenerated(format, success: true);

        // Assert
        var counterAfter = GetCounterValue(ReportsMetrics.ReportsGeneratedTotal, format, "success");
        (counterAfter - counterBefore).Should().BeGreaterOrEqualTo(3);
    }

    // Helper methods to get metric values for assertions
    private static double GetCounterValue(Counter counter, params string[] labels)
    {
        var metric = counter.WithLabels(labels);
        return metric.Value;
    }

    private static double GetCounterValueNoLabels(Counter counter)
    {
        return counter.Value;
    }

    private static double GetGaugeValue(Gauge gauge, params string[] labels)
    {
        var metric = gauge.WithLabels(labels);
        return metric.Value;
    }

    private static double GetGaugeValueNoLabels(Gauge gauge)
    {
        return gauge.Value;
    }

    private static double GetHistogramCount(Histogram histogram, params string[] labels)
    {
        var metric = histogram.WithLabels(labels);
        return metric.Count;
    }
}
