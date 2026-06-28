using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Interfaces;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Controller for structured productivity reports and PDF export.
/// All endpoints require authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// GET /api/reports/productivity — returns JSON grouped report.
    /// </summary>
    [HttpGet("productivity")]
    public async Task<IActionResult> GetProductivityReport(
        [FromQuery] Guid userProfileId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct)
    {
        var report = await _reportService.GenerateProductivityReportAsync(
            userProfileId, startDate, endDate, ct);

        return Ok(report);
    }

    /// <summary>
    /// GET /api/reports/productivity/pdf — streams a PDF download.
    /// </summary>
    [HttpGet("productivity/pdf")]
    public async Task<IActionResult> GetProductivityPdf(
        [FromQuery] Guid userProfileId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct)
    {
        var pdfBytes = await _reportService.GenerateProductivityPdfAsync(
            userProfileId, startDate, endDate, ct);

        var fileName = $"productivity-report-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
