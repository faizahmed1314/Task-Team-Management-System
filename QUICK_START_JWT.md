# JWT Authentication - Quick Start Guide

## ?? Quick Start (5 Minutes)

### Step 1: Reset Database (Required!)
Since passwords are now hashed, you need to reset your database:

```bash
# Navigate to project directory
cd TaskTeamManagementSystem

# Drop existing database
dotnet ef database drop --force

# Apply migrations and seed data
dotnet ef database update
```

**OR** just run the application in Development mode and it will auto-initialize.

### Step 2: Run the Application
```bash
dotnet run
```

The API will start at: `https://localhost:5001` or `http://localhost:5000`

### Step 3: Login to Get Token

**Using curl:**
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@demo.com",
    "password": "Admin123!"
  }'
```

**Using PowerShell:**
```powershell
$body = @{
    email = "admin@demo.com"
    password = "Admin123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@demo.com",
  "fullName": "mehmet",
  "role": "Admin",
  "userId": 1
}
```

### Step 4: Use Token in API Calls

**Using curl:**
```bash
# Save your token
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Make authenticated request
curl -X GET https://localhost:5001/tasks \
  -H "Authorization: Bearer $TOKEN"
```

**Using PowerShell:**
```powershell
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-RestMethod -Uri "https://localhost:5001/tasks" `
  -Method Get `
  -Headers $headers
```

## ?? Test Credentials

| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| **Admin** | admin@demo.com | Admin123! | Full access to all resources |
| **Manager** | manager@demo.com | Manager123! | Manage teams and tasks |
| **Employee** | employee@demo.com | Employee123! | View/update own assigned tasks |

## ?? Quick Tests

### Test 1: Admin Login ?
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin123!"}'
```
**Expected**: 200 OK with token

### Test 2: Invalid Password ?
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"WrongPassword"}'
```
**Expected**: 401 Unauthorized

### Test 3: Get Tasks Without Token ?
```bash
curl -X GET https://localhost:5001/tasks
```
**Expected**: 401 Unauthorized

### Test 4: Get Tasks With Valid Token ?
```bash
TOKEN="your_token_here"
curl -X GET https://localhost:5001/tasks \
  -H "Authorization: Bearer $TOKEN"
```
**Expected**: 200 OK with tasks list

### Test 5: Employee Can Only See Their Tasks ?
```bash
# Login as employee
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@demo.com","password":"Employee123!"}'

# Use returned token
curl -X GET https://localhost:5001/tasks \
  -H "Authorization: Bearer $EMPLOYEE_TOKEN"
```
**Expected**: Only sees tasks assigned to them

## ?? Common Scenarios

### Create New User (Admin Only)
```bash
curl -X POST https://localhost:5001/users \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "user": {
      "fullName": "New User",
      "email": "newuser@demo.com",
      "password": "NewUser123!",
      "role": 2
    }
  }'
```
**Note**: Password will be automatically hashed!

### Get All Users (Admin/Manager)
```bash
curl -X GET https://localhost:5001/users \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

### Create Task (Admin/Manager)
```bash
curl -X POST https://localhost:5001/tasks \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "task": {
      "title": "New Task",
      "description": "Task description",
      "status": 0,
      "assignedToUserId": 3,
      "createdByUserId": 1,
      "teamId": 1,
      "dueDate": "2024-12-31T23:59:59"
    }
  }'
```

## ?? Swagger UI

1. Navigate to: `https://localhost:5001/swagger`
2. Login first via `/auth/login` endpoint
3. Copy the token from response
4. For protected endpoints, manually add header:
   - Name: `Authorization`
   - Value: `Bearer YOUR_TOKEN_HERE`

**Note**: Swagger doesn't have the "Authorize" button yet. See `SWAGGER_JWT_OPTIONAL.md` for how to add it.

## ?? Important Notes

### Token Expiry
- Default: **60 minutes**
- After expiry, login again to get new token
- No refresh token mechanism yet

### Password Security
- All passwords are hashed using PBKDF2-SHA256
- Never send unhashed passwords
- Old database entries won't work - reset required!

### Production Deployment
Before deploying to production:

1. **Change JWT Secret Key**:
   ```json
   {
     "JwtSettings": {
       "SecretKey": "YOUR_SECURE_RANDOM_KEY_HERE_AT_LEAST_32_CHARS"
     }
   }
   ```

2. **Use Environment Variables**:
   ```bash
   export JwtSettings__SecretKey="your-secret-key"
   export JwtSettings__ExpiryInMinutes="30"
   ```

3. **Enable HTTPS Only**

4. **Remove X-User-Id Fallback** (in AuthorizationService.cs)

## ?? What's Working

? JWT token generation and validation  
? Password hashing (PBKDF2-SHA256)  
? Login endpoint with credentials  
? Token-based authentication on all endpoints  
? Role-based authorization (Admin, Manager, Employee)  
? Backward compatible (X-User-Id header still works for testing)  
? Password hashing on user creation/update  
? Database seeding with hashed passwords  

## ?? Additional Resources

- Full Documentation: `JWT_AUTHENTICATION_README.md`
- Implementation Details: `JWT_AUTHENTICATION_SUMMARY.md`
- Swagger Integration: `SWAGGER_JWT_OPTIONAL.md`

## ?? Tips

1. **Token too long?** - Normal! JWT tokens can be 200+ characters
2. **401 Unauthorized?** - Check token expiry and "Bearer " prefix
3. **Can't login?** - Make sure database is reset with hashed passwords
4. **Testing?** - Use Postman/Insomnia for easier token management

## ?? Troubleshooting

### "Invalid email or password"
- Database not reset with hashed passwords
- Solution: `dotnet ef database drop --force && dotnet ef database update`

### "User authentication required"
- Missing or invalid Authorization header
- Solution: Add `Authorization: Bearer YOUR_TOKEN`

### "User does not have the required role"
- Logged in user doesn't have permission
- Solution: Login with appropriate role (Admin/Manager)

### Build errors
- Missing NuGet packages
- Solution: `dotnet restore` then `dotnet build`

---

**You're all set! ??**

Start testing with:
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin123!"}'
```
