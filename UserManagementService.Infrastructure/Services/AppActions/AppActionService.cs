using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.AppActions;

public class AppActionService : IAppActionService
{
    private readonly ApplicationDbContext _context;

    public AppActionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppActionListResponse> GetActionsAsync(
        Guid? appId,    // ✅ NEW: Optional filter by App
        Guid? pageId,   // ✅ CHANGED: Nullable Guid - optional
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        // ✅ Start with ALL actions, include Page for App filtering
        var query = _context.Actions
            .Include(a => a.Page)  // ✅ Needed to filter by AppId through Page
            .AsQueryable();

        // ✅ Apply appId filter ONLY if provided
        if (appId.HasValue)
        {
            query = query.Where(a => a.Page != null && a.Page.AppId == appId.Value);
        }

        // ✅ Apply pageId filter ONLY if provided
        if (pageId.HasValue)
        {
            query = query.Where(a => a.PageId == pageId.Value);
        }

        // ✅ Apply Filters
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.Name.Contains(search) || a.Code.Contains(search));
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
        var actions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var actionDtos = actions.Select(MapToDto).ToList();

        return new AppActionListResponse
        {
            Actions = actionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
    public async Task<AppActionDto> GetActionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
        {
            throw new NotFoundException("Action", id);
        }

        return MapToDto(action);
    }

    public async Task<AppActionDto> CreateActionAsync(
        CreateAppActionRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // ✅ Validate Page exists
        var page = await _context.Pages
            .Include(p => p.App)  // ← Need App for permission code generation
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", request.PageId);
        }

        // ✅ Auto-generate Action Code from Name (SAME pattern as App/Page)
        var code = GenerateActionCode(request.Name);

        // ✅ Check if Code already exists within this Page
        if (await _context.Actions.AnyAsync(a => a.PageId == request.PageId && a.Code == code, cancellationToken))
        {
            throw new ConflictException($"An action with code '{code}' already exists within this page.");
        }

        // ✅ Create Action
        var action = new AppAction
        {
            PageId = request.PageId,
            Name = request.Name,
            Code = code,
            Type = request.Type,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Actions.Add(action);

        // ✅ CRITICAL: AUTO-GENERATE PERMISSION
        // Format: {AppCode}_{PageCode}_{ActionCode}
        if (page.App == null)
        {
            throw new NotFoundException($"App not found for page {page.Id}");
        }
        var permissionCode = $"{page.App.Code}_{page.Code}_{action.Code}";
        var permissionName = $"{page.App.Name} - {page.Name} - {action.Name}";

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            AppId = page.AppId,
            PageId = page.Id,
            ActionId = action.Id,
            PermissionCode = permissionCode,
            Name = permissionName,
            IsEnabled = false,  // ⚠️ Default: INACTIVE (Super Admin must activate)
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Permissions.AddAsync(permission);

        // ✅ Save both Action and Permission in same transaction
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(action);
    }

    public async Task<AppActionDto> UpdateActionAsync(
        Guid id,
        UpdateAppActionRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
        {
            throw new NotFoundException("Action", id);
        }

        // ✅ If Name changed, regenerate Code and update Permission
        if (action.Name != request.Name)
        {
            var newCode = GenerateActionCode(request.Name);
            if (await _context.Actions.AnyAsync(a => a.PageId == action.PageId && a.Code == newCode && a.Id != id, cancellationToken))
            {
                throw new ConflictException($"An action with code '{newCode}' already exists within this page.");
            }
            action.Code = newCode;

            // ✅ Update Permission Code and Name
            var page = await _context.Pages
                .Include(p => p.App)
                .FirstOrDefaultAsync(p => p.Id == action.PageId, cancellationToken);

            if (page != null && page.App != null)
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.ActionId == action.Id, cancellationToken);

                if (permission != null)
                {
                    permission.PermissionCode = $"{page.App.Code}_{page.Code}_{action.Code}";
                    permission.Name = $"{page.App.Name} - {page.Name} - {action.Name}";
                    permission.UpdatedBy = updatedBy;
                    permission.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        action.Name = request.Name;
        action.Description = request.Description;
        action.Type = request.Type;
        action.DisplayOrder = request.DisplayOrder;
        action.IsActive = request.IsActive;
        action.UpdatedBy = updatedBy;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(action);
    }

    public async Task<bool> DeleteActionAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
        {
            throw new NotFoundException("Action", id);
        }

        // ✅ Soft Delete
        action.IsDeleted = true;
        action.DeletedAt = DateTime.UtcNow;
        action.DeletedBy = deletedBy;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AppActionDto> ToggleActionStatusAsync(
        Guid id,
        bool isActive,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
        {
            throw new NotFoundException("Action", id);
        }

        action.IsActive = isActive;
        action.UpdatedBy = updatedBy;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(action);
    }

    // ✅ Helper: Map Entity to DTO
    private static AppActionDto MapToDto(AppAction action)
    {
        return new AppActionDto
        {
            Id = action.Id,
            PageId = action.PageId,
            Name = action.Name,
            Code = action.Code,
            Type = action.Type,
            Description = action.Description,
            DisplayOrder = action.DisplayOrder,
            IsActive = action.IsActive,
            CreatedAt = action.CreatedAt,
            CreatedBy = action.CreatedBy,
            UpdatedAt = action.UpdatedAt,
            UpdatedBy = action.UpdatedBy,
            IsDeleted = action.IsDeleted,
            DeletedAt = action.DeletedAt,
            DeletedBy = action.DeletedBy
        };
    }

    // ✅ Helper: Generate Action Code from Name (SAME pattern as App/Page)
    private static string GenerateActionCode(string actionName)
    {
        var words = actionName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var code = string.Concat(words.Select(w =>
            w.Length <= 3 ? w.ToUpper() : w.Substring(0, 3).ToUpper()));

        return code.Length > 10 ? code.Substring(0, 10) : code;
    }
}