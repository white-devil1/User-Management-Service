using System;
using System.Collections.Generic;
using System.Text;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Services;

    public interface IAppPermissionResolverService
    {
        Task<List<string>> GetUserPermissionsAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct = default);
    }
