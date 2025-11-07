namespace Reports.Tests.Helpers;

/// <summary>
/// Helper class for timezone conversions in tests
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Gets the current time in Ecuador timezone (UTC-5)
    /// </summary>
    public static DateTime EcuadorNow => DateTime.UtcNow.AddHours(-5);

    /// <summary>
    /// Converts a UTC DateTime to Ecuador timezone (UTC-5)
    /// </summary>
    public static DateTime ToEcuadorTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        return utcDateTime.AddHours(-5);
    }
}
