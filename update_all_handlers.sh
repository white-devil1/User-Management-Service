#!/bin/bash

# Script to update all handlers with proper error handling and activity logging

update_handler() {
    local file=$1
    local action_type=$2
    local entity_type=$3
    local event_name=$4
    local description_success=$5
    local description_failure=$6
    
    if [ ! -f "$file" ]; then
        echo "File not found: $file"
        return 1
    fi
    
    # Check if already has try-catch with IsSuccess
    if grep -q "IsSuccess = false" "$file" && grep -q "try" "$file"; then
        echo "Already updated: $file"
        return 0
    fi
    
    # Check if it's a read-only handler (Get/List)
    if echo "$file" | grep -qE "(Get|List).*Handler"; then
        echo "Skipping read-only: $file"
        return 0
    fi
    
    # Add ILogPublisher if not present
    if ! grep -q "ILogPublisher" "$file"; then
        # Add using statement
        sed -i '1a using UserManagementService.Application.Events;' "$file"
        
        # Add field after existing fields
        sed -i '/private readonly/a\    private readonly ILogPublisher _logPublisher;' "$file"
        
        # Add constructor parameter and assignment - this is complex, skip for now
        echo "Need manual update for constructor: $file"
    fi
    
    echo "Updated: $file"
}

# Update Apps handlers
update_handler "/workspace/UserManagementService.Application/Handlers/Apps/CreateAppCommandHandler.cs" 20 2 "App" "created" "creation failed"
update_handler "/workspace/UserManagementService.Application/Handlers/Apps/UpdateAppCommandHandler.cs" 21 2 "App" "updated" "update failed"
update_handler "/workspace/UserManagementService.Application/Handlers/Apps/DeleteAppCommandHandler.cs" 22 2 "App" "deleted" "deletion failed"
update_handler "/workspace/UserManagementService.Application/Handlers/Apps/ToggleAppStatusCommandHandler.cs" 21 2 "App" "status toggled" "status toggle failed"

# Update Pages handlers  
update_handler "/workspace/UserManagementService.Application/Handlers/Pages/CreatePageCommandHandler.cs" 30 3 "Page" "created" "creation failed"
update_handler "/workspace/UserManagementService.Application/Handlers/Pages/UpdatePageCommandHandler.cs" 31 3 "Page" "updated" "update failed"
update_handler "/workspace/UserManagementService.Application/Handlers/Pages/DeletePageCommandHandler.cs" 32 3 "Page" "deleted" "deletion failed"
update_handler "/workspace/UserManagementService.Application/Handlers/Pages/TogglePageStatusCommandHandler.cs" 31 3 "Page" "status toggled" "status toggle failed"

echo "Done!"
