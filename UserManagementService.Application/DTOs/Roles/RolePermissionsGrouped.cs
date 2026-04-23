namespace UserManagementService.Application.DTOs.Roles;

public class RolePermissionActionDto
{
    public Guid Id { get; set; }
    public string ActionName { get; set; } = default!;
}

public class RolePagePermissionsDto
{
    public Guid PageId { get; set; }
    public string PageName { get; set; } = default!;
    public List<RolePermissionActionDto> Permissions { get; set; } = new();
}

public class RoleAppPermissionsDto
{
    public Guid AppId { get; set; }
    public string AppName { get; set; } = default!;
    public List<RolePagePermissionsDto> Pages { get; set; } = new();
}

public class RolePermissionsGrouped
{
    public List<RoleAppPermissionsDto> Apps { get; set; } = new();
}
