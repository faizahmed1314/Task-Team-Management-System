# Role-Based Access Control (RBAC) Implementation

## Overview
This document describes the role-based access control implementation for the Task Team Management System.

## Roles and Permissions

### 1. Admin Role
**Full system access including:**
- ? Create, Read, Update, Delete Users
- ? Create, Read, Update, Delete Teams
- ? Create, Read, Update, Delete Tasks
- ? View all tasks
- ? Update any task

### 2. Manager Role
**Task and user/team management:**
- ? Create Tasks
- ? Update any Task (full details)
- ? Delete Tasks
- ? View all Tasks
- ? View all Users
- ? View all Teams
- ? Cannot manage Users (create/update/delete)
- ? Cannot manage Teams (create/update/delete)

### 3. Employee Role
**Limited access to assigned tasks:**
- ? View assigned Tasks only
- ? Update Status of assigned Tasks
- ? Update full details of assigned Tasks
- ? Cannot view tasks not assigned to them
- ? Cannot create Tasks
- ? Cannot delete Tasks
- ? Cannot view/manage Users
- ? Cannot view/manage Teams

## API Endpoints Authorization Matrix

| Endpoint | Method | Admin | Manager | Employee |
|----------|--------|-------|---------|----------|
| `/users` | POST | ? | ? | ? |
| `/users` | GET | ? | ? | ? |
| `/users/{id}` | GET | ? | ? | ? |
| `/users/{id}` | PUT | ? | ? | ? |
| `/users/{id}` | DELETE | ? | ? | ? |
| `/teams` | POST | ? | ? | ? |
| `/teams` | GET | ? | ? | ? |
| `/teams/{id}` | GET | ? | ? | ? |
| `/teams/{id}` | PUT | ? | ? | ? |
| `/teams/{id}` | DELETE | ? | ? | ? |
| `/tasks` | POST | ? | ? | ? |
| `/tasks` | GET | ? (all) | ? (all) | ? (assigned only) |
| `/tasks/{id}` | GET | ? (any) | ? (any) | ? (assigned only) |
| `/tasks/{id}` | PUT | ? (any) | ? (any) | ? (assigned only) |
| `/tasks/{id}` | DELETE | ? | ? | ? |
| `/tasks/{id}/status` | PATCH | ? (any) | ? (any) | ? (assigned only) |

## Authentication

### Current Implementation
The system uses a header-based authentication for development:
- **Header Name:** `X-User-Id`
- **Header Value:** User ID (integer)

### Example Usage
```bash
# Admin request (User ID 1)
curl -H "X-User-Id: 1" http://localhost:5000/users

# Manager request (User ID 2)
curl -H "X-User-Id: 2" http://localhost:5000/tasks

# Employee request (User ID 3)
curl -H "X-User-Id: 3" http://localhost:5000/tasks
```

### Production Considerations
?? **Important:** Replace the header-based authentication with proper JWT token authentication in production.

## Implementation Details

### Authorization Service
Located at: `TaskTeamManagementSystem\Authorization\AuthorizationService.cs`

Key methods:
- `GetCurrentUserAsync()` - Retrieves the current user from request context
- `IsAuthorized()` - Checks if user has required role(s)
- `CanUpdateTaskAsync()` - Special authorization for task updates

### Authorization Filter
Located at: `TaskTeamManagementSystem\Authorization\AuthorizationFilter.cs`

Provides reusable methods:
- `AuthorizeAsync()` - General role-based authorization
- `AuthorizeTaskUpdateAsync()` - Task-specific authorization (checks ownership for employees)

## Special Features

### Employee Task Filtering
When an employee calls `/tasks`, the system automatically:
1. Filters tasks to show only assigned tasks
2. Ignores any `assignedToUserId` query parameter
3. Forces filtering by the employee's user ID

### Employee Task Status Updates
Employees have a dedicated endpoint `/tasks/{id}/status` (PATCH) to update only the status of their assigned tasks, making it easier than providing all task details.

## Error Responses

### 401 Unauthorized
Returned when no authentication header is provided or user is not found:
```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "User authentication required. Please provide X-User-Id header."
}
```

### 403 Forbidden
Returned when user doesn't have required role:
```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "User does not have the required role(s). Required: Admin"
}
```

Returned when employee tries to access non-assigned task:
```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "You do not have permission to update this task"
}
```

## Testing the Implementation

### Setup Test Users
Ensure you have users with different roles in your database:
```sql
-- Admin user (ID: 1)
-- Manager user (ID: 2)
-- Employee user (ID: 3)
```

### Test Scenarios

#### 1. Admin Can Manage Users
```bash
# Create user (should succeed)
curl -X POST -H "X-User-Id: 1" -H "Content-Type: application/json" \
  -d '{"user":{"fullName":"Test User","email":"test@example.com","password":"password","role":2}}' \
  http://localhost:5000/users
```

#### 2. Manager Cannot Manage Users
```bash
# Create user (should fail with 403)
curl -X POST -H "X-User-Id: 2" -H "Content-Type: application/json" \
  -d '{"user":{"fullName":"Test User","email":"test@example.com","password":"password","role":2}}' \
  http://localhost:5000/users
```

#### 3. Manager Can Create Tasks
```bash
# Create task (should succeed)
curl -X POST -H "X-User-Id: 2" -H "Content-Type: application/json" \
  -d '{"task":{...}}' \
  http://localhost:5000/tasks
```

#### 4. Employee Can Only View Assigned Tasks
```bash
# Get all tasks (will only return tasks assigned to user ID 3)
curl -H "X-User-Id: 3" http://localhost:5000/tasks
```

#### 5. Employee Can Update Task Status
```bash
# Update task status (only for assigned tasks)
curl -X PATCH -H "X-User-Id: 3" -H "Content-Type: application/json" \
  -d '{"status":1}' \
  http://localhost:5000/tasks/1/status
```

## Files Created/Modified

### Created Files:
1. `TaskTeamManagementSystem\Authorization\IAuthorizationService.cs`
2. `TaskTeamManagementSystem\Authorization\AuthorizationService.cs`
3. `TaskTeamManagementSystem\Authorization\AuthorizationFilter.cs`
4. `TaskTeamManagementSystem\Tasks\UpdateTaskStatus\UpdateTaskStatusCommand.cs`
5. `TaskTeamManagementSystem\Tasks\UpdateTaskStatus\UpdateTaskStatusHandler.cs`
6. `TaskTeamManagementSystem\Tasks\UpdateTaskStatus\UpdateTaskStatusValidator.cs`
7. `TaskTeamManagementSystem\Tasks\UpdateTaskStatus\UpdateTaskStatusEndpoint.cs`

### Modified Files:
1. `TaskTeamManagementSystem\Program.cs` - Added authorization service registration
2. All User endpoints (Create, Update, Delete, Get, GetById)
3. All Team endpoints (Create, Update, Delete, Get, GetById)
4. All Task endpoints (Create, Update, Delete, Get, GetById)

## Future Enhancements

1. **JWT Authentication**: Replace header-based auth with JWT tokens
2. **Claims-Based Authorization**: Add more granular permissions
3. **Audit Logging**: Track who accessed/modified what
4. **Rate Limiting**: Add per-role rate limits
5. **API Keys**: For external system integration
6. **OAuth2/OpenID Connect**: For enterprise SSO integration
