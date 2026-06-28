using Portfolio.Application.DTOs;

namespace Portfolio.Application.Interfaces;

/// <summary>
/// Service for generating structured productivity reports.
/// Groups tasks by status and compiles PDF documents.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates a structured JSON report grouped by task status.
    /// </summary>
    Task<ProductivityReportDto> GenerateProductivityReportAsync(
        Guid userProfileId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);

    /// <summary>
    /// Compiles the productivity report into a PDF byte array.
    /// </summary>
    Task<byte[]> GenerateProductivityPdfAsync(
        Guid userProfileId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);
}
