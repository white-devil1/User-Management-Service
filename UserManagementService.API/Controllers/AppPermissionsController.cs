using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Policy = "SuperAdminOnly")]
[Authorize]
public class AppPermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AppPermissionsController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ✅ Endpoint 1: Get All Permissions (Grouped by App → Page → Permission)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<GroupedPermissionResponse>>>
        GetPermissions(
        [FromQuery] Guid? appId,
        [FromQuery] Guid? pageId,
        [FromQuery] bool? isEnabled)
    {
        var command = new GetGroupedPermissionsCommand
        {
            AppId = appId,
            PageId = pageId,
            IsEnabled = isEnabled
        };
        var result = await _mediator.Send(command);

        if (result.Apps.Count == 0)
        {
            return Ok(ApiResponse<GroupedPermissionResponse>.Ok(
                result, "No permissions found"));
        }

        return Ok(ApiResponse<GroupedPermissionResponse>.Ok(
            result, "Permissions fetched successfully"));
    }

    // ✅ Get Permission by ID (kept for backward compat)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppPermissionDto>>>
        GetPermissionById(Guid id)
    {
        var result = await _mediator.Send(
            new GetAppPermissionByIdCommand { Id = id });
        return Ok(ApiResponse<AppPermissionDto>.Ok(
            result, "Permission retrieved successfully"));
    }

    // ✅ Endpoint 2: Toggle Single Permission Status
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<TogglePermissionResponseDto>>>
        TogglePermissionStatus(
        Guid id, [FromBody] ToggleAppPermissionRequest request)
    {
        var command = new TogglePermissionStatusCommand
        {
            Id = id,
            IsEnabled = request.IsEnabled,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<TogglePermissionResponseDto>.Ok(
            result, "Permission updated successfully"));
    }

    // ✅ Endpoint: Export Permissions as Excel
    [HttpGet("export")]
    public async Task<IActionResult> ExportPermissions(
        [FromQuery] Guid? appId,
        [FromQuery] Guid? pageId)
    {
        var rows = await _mediator.Send(new ExportPermissionsCommand
        {
            AppId  = appId,
            PageId = pageId
        });

        using var workbook  = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Permissions");

        // Header row
        string[] headers = ["App", "App Code", "Page", "Page Code", "Action", "Action Code", "Permission Name", "Permission Code"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        for (int r = 0; r < rows.Count; r++)
        {
            var row = rows[r];
            int excelRow = r + 2;
            worksheet.Cell(excelRow, 1).Value = row.AppName;
            worksheet.Cell(excelRow, 2).Value = row.AppCode;
            worksheet.Cell(excelRow, 3).Value = row.PageName;
            worksheet.Cell(excelRow, 4).Value = row.PageCode;
            worksheet.Cell(excelRow, 5).Value = row.ActionName;
            worksheet.Cell(excelRow, 6).Value = row.ActionCode;
            worksheet.Cell(excelRow, 7).Value = row.PermissionName;
            worksheet.Cell(excelRow, 8).Value = row.PermissionCode;

            if (r % 2 == 1)
            {
                worksheet.Row(excelRow).Cells(1, 8)
                    .Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Freeze header
        worksheet.SheetView.FreezeRows(1);

        // Table border
        var dataRange = worksheet.Range(1, 1, rows.Count + 1, 8);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder  = XLBorderStyleValues.Hair;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"permissions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ✅ Endpoint 3: Bulk Toggle Permission Status
    [HttpPatch("bulk-status")]
    public async Task<ActionResult<ApiResponse<BulkTogglePermissionResponse>>>
        BulkTogglePermissionStatus(
        [FromBody] BulkTogglePermissionRequest request)
    {
        var command = new BulkTogglePermissionStatusCommand
        {
            PermissionStatuses = request.PermissionStatuses,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<BulkTogglePermissionResponse>.Ok(
            result, $"{result.UpdatedCount} permissions updated successfully"));
    }
}
