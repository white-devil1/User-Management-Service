#!/usr/bin/env python3
"""
Batch update all handlers with proper error handling and activity logging.
This script manually writes each handler file with correct formatting.
"""

import os
from pathlib import Path

def read_file(path):
    with open(path, 'r') as f:
        return f.read()

def write_file(path, content):
    with open(path, 'w') as f:
        f.write(content)

# Define all handlers that need updating with their templates
HANDLERS_TO_UPDATE = {
    'Apps/DeleteAppCommandHandler.cs': '''using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class DeleteAppCommandHandler
    : IRequestHandler<DeleteAppCommand, bool>
{
    private readonly IAppService _appService;
    private readonly ILogPublisher _logPublisher;

    public DeleteAppCommandHandler(
        IAppService appService, ILogPublisher logPublisher)
    { _appService = appService; _logPublisher = logPublisher; }

    public async Task<bool> Handle(
        DeleteAppCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _appService.DeleteAppAsync(
                request.Id, request.DeletedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 22,  // AppDeleted
                EntityType = 2,   // App
                EntityId = request.Id.ToString(),
                Description = "App was deleted",
                UserId = request.DeletedBy,
                IsSuccess = true
            });
            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 22,  // AppDeleted
                EntityType = 2,   // App
                EntityId = request.Id.ToString(),
                Description = "App deletion failed",
                UserId = request.DeletedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
''',
    'Apps/ToggleAppStatusCommandHandler.cs': '''using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class ToggleAppStatusCommandHandler
    : IRequestHandler<ToggleAppStatusCommand, AppDto>
{
    private readonly IAppService _appService;
    private readonly ILogPublisher _logPublisher;

    public ToggleAppStatusCommandHandler(
        IAppService appService, ILogPublisher logPublisher)
    { _appService = appService; _logPublisher = logPublisher; }

    public async Task<AppDto> Handle(
        ToggleAppStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _appService.ToggleAppStatusAsync(
                request.Id, request.IsActive, request.UpdatedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 21,  // AppUpdated (status toggle)
                EntityType = 2,   // App
                EntityId = result.Id.ToString(),
                Description = $"App '{result.Name}' was {(request.IsActive ? "activated" : "deactivated")}",
                UserId = request.UpdatedBy,
                IsSuccess = true
            });
            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 21,  // AppUpdated (status toggle)
                EntityType = 2,   // App
                EntityId = request.Id.ToString(),
                Description = "App status toggle failed",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
''',
}

base_dir = Path('/workspace/UserManagementService.Application/Handlers')

for handler_path, template in HANDLERS_TO_UPDATE.items():
    full_path = base_dir / handler_path
    if full_path.exists():
        original = read_file(full_path)
        if 'IsSuccess = false' not in original:
            write_file(full_path, template)
            print(f"✓ Updated: {handler_path}")
        else:
            print(f"- Already updated: {handler_path}")
    else:
        print(f"✗ Not found: {handler_path}")

print("\nBatch update complete!")
