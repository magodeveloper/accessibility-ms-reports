using System.Text.Json.Serialization;

namespace Reports.Domain.Entities
{
    public enum ReportFormat
    {
        [JsonStringEnumMemberName("pdf")]
        Pdf,
        [JsonStringEnumMemberName("html")]
        Html,
        [JsonStringEnumMemberName("json")]
        Json,
        [JsonStringEnumMemberName("excel")]
        Excel
    }
}