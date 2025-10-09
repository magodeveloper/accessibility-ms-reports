using Prometheus;

namespace Reports.Api.Metrics;

/// <summary>
/// Métricas personalizadas de negocio para el microservicio de Reports
/// Expone contadores, histogramas y gauges para monitoreo en Prometheus
/// </summary>
public static class ReportsMetrics
{
    // ============================================
    // CONTADORES (Counters)
    // ============================================

    /// <summary>
    /// Total de reportes generados
    /// Labels: format (pdf, html, json, excel), status (success, failure)
    /// </summary>
    public static readonly Counter ReportsGeneratedTotal = Prometheus.Metrics.CreateCounter(
        "reports_generated_total",
        "Total de reportes generados",
        new CounterConfiguration
        {
            LabelNames = new[] { "format", "status" }
        }
    );

    /// <summary>
    /// Total de consultas de reportes
    /// Labels: operation (get_all, get_by_analysis, get_by_format, get_by_date)
    /// </summary>
    public static readonly Counter ReportsQueriesTotal = Prometheus.Metrics.CreateCounter(
        "reports_queries_total",
        "Total de consultas de reportes realizadas",
        new CounterConfiguration
        {
            LabelNames = new[] { "operation" }
        }
    );

    /// <summary>
    /// Total de reportes eliminados
    /// </summary>
    public static readonly Counter ReportsDeletionsTotal = Prometheus.Metrics.CreateCounter(
        "reports_deletions_total",
        "Total de reportes eliminados"
    );

    /// <summary>
    /// Total de accesos al historial
    /// Labels: userId
    /// </summary>
    public static readonly Counter HistoryAccessTotal = Prometheus.Metrics.CreateCounter(
        "reports_history_access_total",
        "Total de accesos al historial de reportes",
        new CounterConfiguration
        {
            LabelNames = new[] { "user_id" }
        }
    );

    // ============================================
    // HISTOGRAMAS (Histograms)
    // ============================================

    /// <summary>
    /// Duración de generación de reportes en segundos
    /// Labels: format (pdf, html, json, excel)
    /// </summary>
    public static readonly Histogram ReportGenerationDuration = Prometheus.Metrics.CreateHistogram(
        "report_generation_duration_seconds",
        "Duración de generación de reportes en segundos",
        new HistogramConfiguration
        {
            LabelNames = new[] { "format" },
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) // 10ms a ~10s
        }
    );

    /// <summary>
    /// Duración de consultas de reportes en segundos
    /// Labels: operation
    /// </summary>
    public static readonly Histogram ReportsQueryDuration = Prometheus.Metrics.CreateHistogram(
        "reports_query_duration_seconds",
        "Duración de consultas de reportes en segundos",
        new HistogramConfiguration
        {
            LabelNames = new[] { "operation" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) // 1ms a ~1s
        }
    );

    // ============================================
    // GAUGES (Medidores)
    // ============================================

    /// <summary>
    /// Tamaño total de reportes en bytes
    /// </summary>
    public static readonly Gauge ReportsSizeBytes = Prometheus.Metrics.CreateGauge(
        "reports_total_size_bytes",
        "Tamaño total de reportes almacenados en bytes"
    );

    /// <summary>
    /// Número de reportes por formato
    /// Labels: format
    /// </summary>
    public static readonly Gauge ReportsByFormat = Prometheus.Metrics.CreateGauge(
        "reports_by_format",
        "Número de reportes por formato",
        new GaugeConfiguration
        {
            LabelNames = new[] { "format" }
        }
    );

    // ============================================
    // MÉTODOS DE UTILIDAD
    // ============================================

    /// <summary>
    /// Registra un reporte generado exitosamente
    /// </summary>
    public static void RecordReportGenerated(string format, bool success = true)
    {
        ReportsGeneratedTotal.WithLabels(format.ToLower(), success ? "success" : "failure").Inc();
    }

    /// <summary>
    /// Registra una consulta de reportes
    /// </summary>
    public static void RecordQuery(string operation)
    {
        ReportsQueriesTotal.WithLabels(operation).Inc();
    }

    /// <summary>
    /// Registra la duración de generación de un reporte
    /// </summary>
    public static IDisposable MeasureReportGeneration(string format)
    {
        return ReportGenerationDuration.WithLabels(format.ToLower()).NewTimer();
    }

    /// <summary>
    /// Registra la duración de una consulta
    /// </summary>
    public static IDisposable MeasureQuery(string operation)
    {
        return ReportsQueryDuration.WithLabels(operation).NewTimer();
    }
}
