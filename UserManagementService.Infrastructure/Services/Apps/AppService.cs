using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

// ✅ ALIAS: Avoid conflict between namespace "Apps" and entity "App"
using AppEntity = UserManagementService.Domain.Entities.RBAC.App;

namespace UserManagementService.Infrastructure.Services.Apps;

public class AppService : IAppService
{
    private readonly ApplicationDbContext _context;

    public AppService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppListResponse> GetAppsAsync(
        string? search,
        string? code,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Apps.AsQueryable();

        // ✅ Apply Filters
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.Name.Contains(search) || a.Code.Contains(search));
        }

        if (!string.IsNullOrEmpty(code))
        {
            query = query.Where(a => a.Code == code);
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        // ✅ Include deleted only if requested (Super Admin)
        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        // ✅ Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ Apply Sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.Name)
                : query.OrderByDescending(a => a.Name),
            "code" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.Code)
                : query.OrderByDescending(a => a.Code),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.CreatedAt)
                : query.OrderByDescending(a => a.CreatedAt),
            _ => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.DisplayOrder)
                : query.OrderByDescending(a => a.DisplayOrder)
        };

        // ✅ Apply Pagination
        var apps = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var appDtos = apps.Select(MapToDto).ToList();

        return new AppListResponse
        {
            Apps = appDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AppDto> GetAppByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        return MapToDto(app);
    }

    public async Task<AppDto> CreateAppAsync(CreateAppRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        // ✅ Auto-generate App Code from Name
        var code = GenerateAppCode(request.Name);

        // ✅ Check if Code already exists
        if (await _context.Apps.AnyAsync(a => a.Code == code, cancellationToken))
        {
            throw new ConflictException($"An app with code '{code}' already exists.");
        }

        var app = new AppEntity
        {
            Name = request.Name,
            Code = code,
            Description = request.Description,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Apps.Add(app);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(app);
    }

    public async Task<AppDto> UpdateAppAsync(Guid id, UpdateAppRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        // ✅ If Name changed, regenerate Code
        if (app.Name != request.Name)
        {
            var newCode = GenerateAppCode(request.Name);
            if (await _context.Apps.AnyAsync(a => a.Code == newCode && a.Id != id, cancellationToken))
            {
                throw new ConflictException($"An app with code '{newCode}' already exists.");
            }
            app.Code = newCode;
        }

        app.Name = request.Name;
        app.Description = request.Description;
        app.Icon = request.Icon;
        app.DisplayOrder = request.DisplayOrder;
        app.IsActive = request.IsActive;
        app.UpdatedBy = updatedBy;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(app);
    }

    public async Task<bool> DeleteAppAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        // ✅ Soft Delete
        app.IsDeleted = true;
        app.DeletedAt = DateTime.UtcNow;
        app.DeletedBy = deletedBy;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AppDto> ToggleAppStatusAsync(Guid id, bool isActive, string updatedBy, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        app.IsActive = isActive;
        app.UpdatedBy = updatedBy;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(app);
    }

    // ✅ Helper: Map Entity to DTO (using alias AppEntity)
    private static AppDto MapToDto(AppEntity app)
    {
        return new AppDto
        {
            Id = app.Id,
            Name = app.Name,
            Code = app.Code,
            Description = app.Description,
            Icon = app.Icon,
            DisplayOrder = app.DisplayOrder,
            IsActive = app.IsActive,
            CreatedAt = app.CreatedAt,
            CreatedBy = app.CreatedBy,
            UpdatedAt = app.UpdatedAt,
            UpdatedBy = app.UpdatedBy,
            IsDeleted = app.IsDeleted,
            DeletedAt = app.DeletedAt,
            DeletedBy = app.DeletedBy
        };
    }

    // ✅ Helper: Generate App Code from Name (Industry Standard)
    private static string GenerateAppCode(string appName)
    {
        // Remove special characters and split by spaces
        var words = appName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Take first 2-3 letters of each word, uppercase
        var code = string.Concat(words.Select(w =>
            w.Length <= 3 ? w.ToUpper() : w.Substring(0, 3).ToUpper()));

        // Limit to 10 characters max
        return code.Length > 10 ? code.Substring(0, 10) : code;
    }
}