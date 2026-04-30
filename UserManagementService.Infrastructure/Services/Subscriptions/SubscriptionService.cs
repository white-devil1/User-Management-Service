using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Subscriptions;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Subscriptions;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserDisplayNameResolver _resolver;

    public SubscriptionService(ApplicationDbContext context, IUserDisplayNameResolver resolver)
    {
        _context = context;
        _resolver = resolver;
    }

    public async Task<SubscriptionListResponse> GetSubscriptionsAsync(
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Subscriptions
            .Include(s => s.SubscriptionApps)
                .ThenInclude(sa => sa.App)
            .AsQueryable();

        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (!string.IsNullOrEmpty(search))
        {
            var trimmed = search.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(trimmed) ||
                (s.Description != null && s.Description.ToLower().Contains(trimmed)));
        }

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy.ToLower() switch
        {
            "price" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(s => s.Price)
                : query.OrderByDescending(s => s.Price),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt),
            _ => sortOrder.ToLower() == "asc"
                ? query.OrderBy(s => s.Name)
                : query.OrderByDescending(s => s.Name)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var allUserIds = items.SelectMany(s => new[] { s.CreatedBy, s.UpdatedBy, s.DeletedBy });
        var names = await BuildNameCacheAsync(allUserIds, cancellationToken);

        return new SubscriptionListResponse
        {
            Subscriptions = items.Select(s => MapToDto(s, names)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<SubscriptionDto> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sub = await _context.Subscriptions
            .Include(s => s.SubscriptionApps)
                .ThenInclude(sa => sa.App)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (sub == null)
            throw new NotFoundException("Subscription", id);

        var names = await BuildNameCacheAsync(new[] { sub.CreatedBy, sub.UpdatedBy, sub.DeletedBy }, cancellationToken);
        return MapToDto(sub, names);
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(
        CreateSubscriptionRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        var nameTrimmed = request.Name.Trim();
        if (await _context.Subscriptions.AnyAsync(s => s.Name.ToLower() == nameTrimmed.ToLower(), cancellationToken))
            throw new ConflictException($"A subscription with name '{nameTrimmed}' already exists.");

        await ValidateAppIdsAsync(request.AppIds, cancellationToken);

        var sub = new Subscription
        {
            Name = nameTrimmed,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency.ToUpper(),
            IsActive = request.IsActive,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            SubscriptionApps = request.AppIds
                .Distinct()
                .Select(appId => new SubscriptionApp { AppId = appId })
                .ToList()
        };

        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetSubscriptionByIdAsync(sub.Id, cancellationToken);
    }

    public async Task<SubscriptionDto> UpdateSubscriptionAsync(
        Guid id, UpdateSubscriptionRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var sub = await _context.Subscriptions
            .Include(s => s.SubscriptionApps)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (sub == null)
            throw new NotFoundException("Subscription", id);

        var nameTrimmed = request.Name.Trim();
        if (await _context.Subscriptions.AnyAsync(
                s => s.Name.ToLower() == nameTrimmed.ToLower() && s.Id != id,
                cancellationToken))
            throw new ConflictException($"A subscription with name '{nameTrimmed}' already exists.");

        await ValidateAppIdsAsync(request.AppIds, cancellationToken);

        sub.Name = nameTrimmed;
        sub.Description = request.Description;
        sub.Price = request.Price;
        sub.Currency = request.Currency.ToUpper();
        sub.IsActive = request.IsActive;
        sub.UpdatedBy = updatedBy;
        sub.UpdatedAt = DateTime.UtcNow;

        var desiredAppIds = request.AppIds.Distinct().ToHashSet();
        var existingAppIds = sub.SubscriptionApps.Select(sa => sa.AppId).ToHashSet();

        var toRemove = sub.SubscriptionApps.Where(sa => !desiredAppIds.Contains(sa.AppId)).ToList();
        foreach (var rem in toRemove)
            sub.SubscriptionApps.Remove(rem);

        foreach (var appId in desiredAppIds.Where(id => !existingAppIds.Contains(id)))
            sub.SubscriptionApps.Add(new SubscriptionApp { SubscriptionId = sub.Id, AppId = appId });

        await _context.SaveChangesAsync(cancellationToken);

        return await GetSubscriptionByIdAsync(sub.Id, cancellationToken);
    }

    public async Task<bool> DeleteSubscriptionAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (sub == null)
            throw new NotFoundException("Subscription", id);

        sub.IsDeleted = true;
        sub.DeletedAt = DateTime.UtcNow;
        sub.DeletedBy = deletedBy;
        sub.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SubscriptionDto> ToggleSubscriptionStatusAsync(
        Guid id, bool isActive, string updatedBy, CancellationToken cancellationToken = default)
    {
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (sub == null)
            throw new NotFoundException("Subscription", id);

        sub.IsActive = isActive;
        sub.UpdatedBy = updatedBy;
        sub.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetSubscriptionByIdAsync(sub.Id, cancellationToken);
    }

    private async Task ValidateAppIdsAsync(List<Guid> appIds, CancellationToken ct)
    {
        var distinct = appIds.Distinct().ToList();
        if (distinct.Count == 0)
            throw new BadRequestException("At least one App must be selected.");

        var existing = await _context.Apps
            .Where(a => distinct.Contains(a.Id) && !a.IsDeleted)
            .Select(a => a.Id)
            .ToListAsync(ct);

        var missing = distinct.Except(existing).ToList();
        if (missing.Count > 0)
            throw new BadRequestException(
                $"One or more selected Apps do not exist: {string.Join(", ", missing)}");
    }

    private async Task<Dictionary<string, string>> BuildNameCacheAsync(
        IEnumerable<string?> userIds, CancellationToken ct)
    {
        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct()!)
            cache[id!] = await _resolver.ResolveAsync(id, ct);
        return cache;
    }

    private static SubscriptionDto MapToDto(Subscription s, Dictionary<string, string> names) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        Price = s.Price,
        Currency = s.Currency,
        IsActive = s.IsActive,
        Apps = s.SubscriptionApps
            .Where(sa => sa.App != null)
            .Select(sa => new SubscriptionAppDto
            {
                Id = sa.App.Id,
                Name = sa.App.Name,
                Code = sa.App.Code
            })
            .ToList(),
        CreatedAt = s.CreatedAt,
        CreatedBy = s.CreatedBy != null && names.TryGetValue(s.CreatedBy, out var cb) ? cb : null,
        UpdatedAt = s.UpdatedAt,
        UpdatedBy = s.UpdatedBy != null && names.TryGetValue(s.UpdatedBy, out var ub) ? ub : null,
        IsDeleted = s.IsDeleted,
        DeletedAt = s.DeletedAt,
        DeletedBy = s.DeletedBy != null && names.TryGetValue(s.DeletedBy, out var db) ? db : null
    };
}
