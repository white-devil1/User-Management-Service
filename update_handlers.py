#!/usr/bin/env python3
"""
Script to update all MediatR handlers with proper error handling and activity logging.
This script will:
1. Add ILogPublisher dependency where missing
2. Wrap service calls in try-catch blocks
3. Log activity for both success and failure scenarios
4. Ensure IsSuccess flag is set correctly
"""

import os
import re
from pathlib import Path

# Define handler patterns and their corresponding activity types
HANDLER_PATTERNS = {
    # Apps handlers
    'CreateAppCommandHandler': {'action_type': 20, 'entity_type': 2, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'App'},
    'UpdateAppCommandHandler': {'action_type': 21, 'entity_type': 2, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'App'},
    'DeleteAppCommandHandler': {'action_type': 22, 'entity_type': 2, 'entity_id_field': 'request.Id', 'description': 'App was deleted', 'event_name': 'App'},
    'ToggleAppStatusCommandHandler': {'action_type': 21, 'entity_type': 2, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'App'},
    'GetAppByIdCommandHandler': {'skip_log': True},
    'ListAppsCommandHandler': {'skip_log': True},
    
    # Pages handlers
    'CreatePageCommandHandler': {'action_type': 30, 'entity_type': 3, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'Page'},
    'UpdatePageCommandHandler': {'action_type': 31, 'entity_type': 3, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'Page'},
    'DeletePageCommandHandler': {'action_type': 32, 'entity_type': 3, 'entity_id_field': 'request.Id', 'description': 'Page was deleted', 'event_name': 'Page'},
    'TogglePageStatusCommandHandler': {'action_type': 31, 'entity_type': 3, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'Page'},
    'GetPageByIdCommandHandler': {'skip_log': True},
    'ListPagesCommandHandler': {'skip_log': True},
    
    # Users handlers
    'CreateUserCommandHandler': {'action_type': 0, 'entity_type': 0, 'entity_id_field': 'user.Id', 'description_field': 'user.Email', 'event_name': 'User', 'special': 'user_create'},
    'UpdateUserCommandHandler': {'action_type': 1, 'entity_type': 0, 'entity_id_field': 'user.Id', 'description_field': 'user.Email', 'event_name': 'User', 'special': 'user_update'},
    'DeleteUserCommandHandler': {'action_type': 2, 'entity_type': 0, 'entity_id_field': 'request.Id', 'event_name': 'User'},
    'RestoreUserCommandHandler': {'action_type': 3, 'entity_type': 0, 'entity_id_field': 'request.Id', 'event_name': 'User'},
    'ToggleUserStatusCommandHandler': {'action_type': 1, 'entity_type': 0, 'entity_id_field': 'request.Id', 'event_name': 'User'},
    'GetUserByIdCommandHandler': {'skip_log': True},
    'ListUsersCommandHandler': {'skip_log': True},
    'GetAvailableRolesCommandHandler': {'skip_log': True},
    
    # Roles handlers
    'CreateRoleCommandHandler': {'action_type': 10, 'entity_type': 1, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'Role'},
    'UpdateRoleCommandHandler': {'action_type': 11, 'entity_type': 1, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'Role'},
    'DeleteRoleCommandHandler': {'action_type': 12, 'entity_type': 1, 'entity_id_field': 'request.Id', 'event_name': 'Role'},
    'GetRoleByIdCommandHandler': {'skip_log': True},
    'ListRolesCommandHandler': {'skip_log': True},
    'AssignPermissionsCommandHandler': {'action_type': 13, 'entity_type': 1, 'entity_id_field': 'request.RoleId', 'event_name': 'Role'},
    
    # AppActions handlers
    'CreateAppActionCommandHandler': {'action_type': 40, 'entity_type': 4, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'AppAction'},
    'UpdateAppActionCommandHandler': {'action_type': 41, 'entity_type': 4, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'AppAction'},
    'DeleteAppActionCommandHandler': {'action_type': 42, 'entity_type': 4, 'entity_id_field': 'request.Id', 'event_name': 'AppAction'},
    'ToggleAppActionStatusCommandHandler': {'action_type': 41, 'entity_type': 4, 'entity_id_field': 'result.Id', 'description_field': 'result.Name', 'event_name': 'AppAction'},
    'GetAppActionByIdCommandHandler': {'skip_log': True},
    'ListAppActionsCommandHandler': {'skip_log': True},
    
    # AppPermissions handlers
    'GetAppPermissionByIdCommandHandler': {'skip_log': True},
    'ListAppPermissionsCommandHandler': {'skip_log': True},
    'ToggleAppPermissionCommandHandler': {'action_type': 50, 'entity_type': 5, 'entity_id_field': 'request.Id', 'event_name': 'AppPermission'},
    
    # Auth handlers - already handled specially
    'LoginCommandHandler': {'skip': True},
    'RegisterCommandHandler': {'skip': True},
    'ForgotPasswordCommandHandler': {'skip': True},
    'ResetPasswordCommandHandler': {'skip': True},
    'VerifyOtpCommandHandler': {'skip': True},
    'ChangePasswordCommandHandler': {'skip': True},
    'AdminResetPasswordCommandHandler': {'skip': True},
    'RefreshTokenCommandHandler': {'skip': True},
}

def get_handler_info(filename):
    """Extract handler name from filename"""
    match = re.search(r'(\w+Handler)\.cs$', filename)
    if match:
        return match.group(1)
    return None

def needs_update(filepath):
    """Check if file needs update"""
    with open(filepath, 'r') as f:
        content = f.read()
    
    # Skip if already has proper error handling with try-catch and IsSuccess
    if 'try' in content and 'IsSuccess = false' in content and 'IsSuccess = true' in content:
        return False
    
    # Skip read-only handlers (Get/List)
    handler_name = get_handler_info(filepath)
    if handler_name and handler_name in HANDLER_PATTERNS:
        if HANDLER_PATTERNS[handler_name].get('skip_log'):
            return False
        if HANDLER_PATTERNS[handler_name].get('skip'):
            return False
    
    # Check if it's a command handler that modifies data
    if 'CommandHandler' in filepath and 'IRequestHandler' in content:
        return True
    
    return False

def add_log_publisher_dependency(content, handler_name):
    """Add ILogPublisher dependency if not present"""
    if 'ILogPublisher' in content:
        return content
    
    # Find the class definition
    class_match = re.search(r'public class \w+\s*:.*?\n\{', content, re.DOTALL)
    if not class_match:
        return content
    
    # Check existing fields
    fields_section = content[class_match.start():class_match.end()+500]
    
    # Add field declaration
    if 'private readonly' in fields_section:
        # Insert after last field
        last_field_match = list(re.finditer(r'private readonly [^;]+;', fields_section))[-1]
        insert_pos = class_match.start() + last_field_match.end()
        content = content[:insert_pos] + '\n    private readonly ILogPublisher _logPublisher;' + content[insert_pos:]
    
    # Add constructor parameter
    ctor_match = re.search(r'public ' + handler_name + r'\(([^)]*)\)', content, re.DOTALL)
    if ctor_match:
        params = ctor_match.group(1).strip()
        if params and not params.endswith(','):
            params += ','
        new_params = params + '\n        ILogPublisher logPublisher'
        content = content.replace(ctor_match.group(0), f'public {handler_name}({new_params})')
        
        # Add assignment in constructor body
        ctor_body_match = re.search(r'\{\s*([^}]*)\}', content[ctor_match.end():], re.DOTALL)
        if ctor_body_match:
            body_start = ctor_match.end() + ctor_body_match.start()
            body_content = ctor_body_match.group(1)
            if '_logPublisher' not in body_content:
                # Find last assignment or add before closing brace
                if body_content.strip():
                    last_semicolon = body_content.rfind(';')
                    if last_semicolon != -1:
                        insert_in_body = body_content[:last_semicolon+1] + '\n        _logPublisher = logPublisher;'
                        content = content[:body_start] + insert_in_body + body_content[last_semicolon+1:]
    
    # Add using statement if not present
    if 'using UserManagementService.Application.Events;' not in content:
        content = 'using UserManagementService.Application.Events;\n' + content
    
    return content

def wrap_with_try_catch_and_logging(content, handler_info):
    """Wrap the Handle method with try-catch and activity logging"""
    
    # Find the Handle method
    handle_match = re.search(
        r'public async Task<[^>]+> Handle\([^)]+\)\s*\{',
        content,
        re.DOTALL
    )
    
    if not handle_match:
        return content
    
    method_start = handle_match.start()
    brace_start = handle_match.end() - 1
    
    # Find matching closing brace
    brace_count = 1
    pos = brace_start + 1
    while pos < len(content) and brace_count > 0:
        if content[pos] == '{':
            brace_count += 1
        elif content[pos] == '}':
            brace_count -= 1
        pos += 1
    
    if brace_count != 0:
        return content  # Could not find matching braces
    
    method_end = pos
    
    # Extract method body
    method_body = content[brace_start+1:method_end-1]
    
    # Skip if already has try-catch
    if 'try' in method_body and 'catch' in method_body:
        return content
    
    # Find the main service call(s)
    service_call_match = re.search(r'var result = await _\w+Service\.\w+Async\([^;]+\);', method_body, re.DOTALL)
    
    if not service_call_match:
        # Try to find any service call
        service_call_match = re.search(r'(await _\w+Service\.\w+Async\([^;]+\);)', method_body, re.DOTALL)
    
    if not service_call_match:
        return content  # No service call found
    
    action_type = handler_info.get('action_type', 0)
    entity_type = handler_info.get('entity_type', 0)
    event_name = handler_info.get('event_name', 'Entity')
    description = handler_info.get('description')
    description_field = handler_info.get('description_field')
    entity_id_field = handler_info.get('entity_id_field')
    special = handler_info.get('special')
    
    # Build success log
    if description:
        desc_str = f'"{description}"'
    elif description_field:
        desc_str = f'$"{event_name} {{{{{description_field}}}}} was created"'
    else:
        desc_str = f'$"{event_name} operation completed"'
    
    if entity_id_field:
        id_str = entity_id_field
    else:
        id_str = 'request.Id?.ToString() ?? "unknown"'
    
    # Determine user field
    user_field = 'request.CreatedBy' if 'CreatedBy' in method_body else \
                 'request.UpdatedBy' if 'UpdatedBy' in method_body else \
                 'request.UserId' if 'UserId' in method_body else \
                 'null'
    
    success_log = f'''
        _logPublisher.PublishActivity(new ActivityLogEvent
        {{
            ActionType = {action_type},
            EntityType = {entity_type},
            EntityId = {id_str},
            Description = {desc_str},
            UserId = {user_field},
            IsSuccess = true
        }});
'''
    
    # Build failure log
    failure_log = f'''
            _logPublisher.PublishActivity(new ActivityLogEvent
            {{
                ActionType = {action_type},
                EntityType = {entity_type},
                EntityId = {id_str},
                Description = {desc_str.replace('"', "'")},
                UserId = {user_field},
                IsSuccess = false,
                FailureReason = ex.Message
            }});
            throw;
'''
    
    # Wrap service call with try-catch
    service_call = service_call_match.group(0)
    indented_service_call = '    ' + service_call.replace('\n', '\n    ')
    indented_success_log = success_log.replace('\n', '\n    ').strip()
    indented_failure_log = failure_log.replace('\n', '\n            ').strip()
    
    new_method_body = method_body.replace(
        service_call,
        f'''try
        {{
{indented_service_call}
{indented_success_log}
        return result;
        }}
        catch (Exception ex)
        {{
{indented_failure_log}'''
    )
    
    # Replace the method body
    content = content[:brace_start+1] + new_method_body + content[method_end-1:]
    
    return content

def process_handler_file(filepath):
    """Process a single handler file"""
    handler_name = get_handler_info(filepath)
    if not handler_name:
        return False
    
    handler_info = HANDLER_PATTERNS.get(handler_name, {})
    
    if handler_info.get('skip') or handler_info.get('skip_log'):
        return False
    
    with open(filepath, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Add ILogPublisher if needed
    if 'ILogPublisher' not in content:
        content = add_log_publisher_dependency(content, handler_name)
    
    # Check if needs try-catch wrapping
    if 'try' not in content or 'IsSuccess = false' not in content:
        content = wrap_with_try_catch_and_logging(content, handler_info)
    
    # Only write if changed
    if content != original_content:
        with open(filepath, 'w') as f:
            f.write(content)
        return True
    
    return False

def main():
    base_dir = Path('/workspace/UserManagementService.Application/Handlers')
    updated_count = 0
    skipped_count = 0
    
    for filepath in base_dir.rglob('*Handler.cs'):
        if needs_update(str(filepath)):
            try:
                if process_handler_file(str(filepath)):
                    print(f"✓ Updated: {filepath.name}")
                    updated_count += 1
                else:
                    print(f"- Skipped (no changes needed): {filepath.name}")
                    skipped_count += 1
            except Exception as e:
                print(f"✗ Error processing {filepath.name}: {e}")
        else:
            print(f"- Skipped (read-only or already updated): {filepath.name}")
            skipped_count += 1
    
    print(f"\n=== Summary ===")
    print(f"Updated: {updated_count} files")
    print(f"Skipped: {skipped_count} files")

if __name__ == '__main__':
    main()
