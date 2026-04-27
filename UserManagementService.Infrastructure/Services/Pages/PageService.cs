using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.Pages;

public class PageService : IPageService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserDisplayNameResolver _resolver;

    public PageService(ApplicationDbContext context, IUserDisplayNameResolver resolver)
    {
        _context = context;
        _resolver = resolver;
    }

    public async Task<PageListResponse> GetPagesAsync(
        Guid? appId,  // ✅ CHANGED: Nullable Guid
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pages.Include(p => p.App).AsQueryable();  // ✅ Include App navigation & start with ALL pages

        // ✅ Apply appId filter ONLY if provided
        if (appId.HasValue)
        {
            query = query.Where(p => p.AppId == appId.Value);
        }

        // ✅ Apply Filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(p => 
                EF.Functions.ILike(p.Name, $"%{trimmedSearch}%") || 
                EF.Functions.ILike(p.Code, $"%{trimmedSearch}%") || 
                (p.Description != null && EF.Functions.ILike(p.Description, $"%{trimmedSearch}%")));
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // ✅ Include deleted only if requested (Super Admin)
        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        // ✅ Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ Apply Sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),
            "code" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.Code)
                : query.OrderByDescending(p => p.Code),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.DisplayOrder)
                : query.OrderByDescending(p => p.DisplayOrder)
        };

        // ✅ Apply Pagination
        var pages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pageDtos = pages.Select(MapToDto).ToList();

        return new PageListResponse
        {
            Pages = pageDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
    public async Task<PageDto> GetPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages
            .Include(p => p.App)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", id);
        }

        return MapToDto(page);
    }

    public async Task<PageDto> CreatePageAsync(
        CreatePageRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var createdByName = await _resolver.ResolveAsync(createdBy, cancellationToken);

        // ✅ Validate App exists
        var app = await _context.Apps.FindAsync(request.AppId);
        if (app == null)
        {
            throw new NotFoundException("App", request.AppId);
        }

        // ✅ Auto-generate Page Code from Name (SAME pattern as AppService)
        var code = GeneratePageCode(request.Name);

        // ✅ Check if Code already exists within this App
        if (await _context.Pages.AnyAsync(p => p.AppId == request.AppId && p.Code == code, cancellationToken))
        {
            throw new ConflictException($"A page with code '{code}' already exists within this app.");
        }

        var page = new Page
        {
            AppId = request.AppId,
            Name = request.Name,
            Code = code,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = createdByName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Pages.Add(page);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(page);
    }

    public async Task<PageDto> UpdatePageAsync(
        Guid id,
        UpdatePageRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);

        var page = await _context.Pages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", id);
        }

        // ✅ If Name changed, regenerate Code
        if (page.Name != request.Name)
        {
            var newCode = GeneratePageCode(request.Name);
            if (await _context.Pages.AnyAsync(p => p.AppId == page.AppId && p.Code == newCode && p.Id != id, cancellationToken))
            {
                throw new ConflictException($"A page with code '{newCode}' already exists within this app.");
            }
            page.Code = newCode;
        }

        page.Name = request.Name;
        page.Description = request.Description;
        page.DisplayOrder = request.DisplayOrder;
        page.IsActive = request.IsActive;
        page.UpdatedBy = updatedByName;
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(page);
    }

    public async Task<bool> DeletePageAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var deletedByName = await _resolver.ResolveAsync(deletedBy, cancellationToken);

        var page = await _context.Pages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", id);
        }

        // ✅ Soft Delete
        page.IsDeleted = true;
        page.DeletedAt = DateTime.UtcNow;
        page.DeletedBy = deletedByName;
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<PageDto> TogglePageStatusAsync(
        Guid id,
        bool isActive,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);

        var page = await _context.Pages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null)
        {
            throw new NotFoundException("Page", id);
        }

        page.IsActive = isActive;
        page.UpdatedBy = updatedByName;
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(page);
    }

    // ✅ Helper: Map Entity to DTO
    private static PageDto MapToDto(Page page)
    {
        return new PageDto
        {
            Id = page.Id,
            AppId = page.AppId,
            AppName = page.App?.Name,
            Name = page.Name,
            Code = page.Code,
            Description = page.Description,
            DisplayOrder = page.DisplayOrder,
            IsActive = page.IsActive,
            CreatedAt = page.CreatedAt,
            CreatedBy = page.CreatedBy,
            UpdatedAt = page.UpdatedAt,
            UpdatedBy = page.UpdatedBy,
            IsDeleted = page.IsDeleted,
            DeletedAt = page.DeletedAt,
            DeletedBy = page.DeletedBy
        };
    }

    // ✅ Helper: Generate Page Code from Name (SAME pattern as AppService.GenerateAppCode)
    private static string GeneratePageCode(string pageName)
    {
        // Remove special characters and split by spaces
        var words = pageName
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