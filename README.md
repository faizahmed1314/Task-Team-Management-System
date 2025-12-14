# Task Team Management System

A comprehensive task management system built with .NET 10, featuring JWT authentication, role-based authorization, and RESTful API endpoints for managing users, teams, and tasks.

## ?? Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

## ? Features

- **JWT Authentication** - Secure token-based authentication
- **Role-Based Authorization** - Admin, Manager, and Employee roles with different permissions
- **User Management** - CRUD operations for user accounts
- **Team Management** - Create and manage teams
- **Task Management** - Full task lifecycle management with status tracking
- **Password Security** - PBKDF2-SHA256 password hashing with 100,000 iterations
- **RESTful API** - Clean, well-documented API endpoints
- **Swagger Documentation** - Interactive API documentation
- **CQRS Pattern** - Using MediatR for command and query separation
- **FluentValidation** - Request validation
- **Entity Framework Core** - Database access with Code-First approach

## ?? Technology Stack

- **.NET 10** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 10.0.1** - ORM
- **SQL Server** - Database
- **Carter 10.0.0** - Minimal API framework
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **JWT Bearer Authentication** - Secure authentication
- **Swashbuckle** - API documentation
- **Mapster** - Object mapping

## ?? Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or Developer Edition)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (Optional, for database management)
- [Git](https://git-scm.com/downloads)

### Verify Installation

```bash
# Check .NET version
dotnet --version

# Should output: 10.x.x or higher
```

## ?? Installation

### 1. Clone the Repository

```bash
git clone https://github.com/faizahmed1314/Task-Team-Management-System.git
cd Task-Team-Management-System
```

### 2. Restore NuGet Packages

```bash
# Navigate to the main project directory
cd TaskTeamManagementSystem

# Restore packages
dotnet restore
```

### 3. Build the Solution

```bash
# Build the entire solution
dotnet build

# Or build from the solution root
cd ..
dotnet build
```

## ?? Configuration

### 1. Update Connection String

Edit `TaskTeamManagementSystem/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=TaskManagementDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
  }
}
```

**Connection String Options:**

**For SQL Server (Integrated Security):**
```json
"Database": "Server=localhost;Database=TaskManagementDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
```

**For SQL Server (Username/Password):**
```json
"Database": "Server=localhost;Database=TaskManagementDb;User Id=your_username;Password=your_password;Encrypt=False;TrustServerCertificate=True"
```

**For SQL Server Express:**
```json
"Database": "Server=localhost\\SQLEXPRESS;Database=TaskManagementDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
```

**For LocalDB:**
```json
"Database": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
```

### 2. JWT Configuration

The JWT settings are already configured in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
    "Issuer": "TaskTeamManagementSystem",
    "Audience": "TaskTeamManagementSystemUsers",
    "ExpiryInMinutes": "60"
  }
}
```

?? **IMPORTANT**: Change the `SecretKey` before deploying to production!

**Generate a secure key:**
```bash
# Using PowerShell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)

# Using OpenSSL
openssl rand -base64 64
```

## ?? Database Setup

### Option 1: Automatic Setup (Recommended for Development)

The application automatically creates and seeds the database when running in Development mode:

```bash
cd TaskTeamManagementSystem
dotnet run
```

The database will be created with initial seed data automatically.

### Option 2: Manual Setup Using EF Core Migrations

```bash
# Navigate to project directory
cd TaskTeamManagementSystem

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef

# Create the database and apply migrations
dotnet ef database update
```

### Option 3: Reset Database (if exists)

If you need to reset the database:

```bash
cd TaskTeamManagementSystem

# Drop the existing database
dotnet ef database drop --force

# Recreate and apply migrations
dotnet ef database update
```

### Verify Database Creation

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Look for `TaskManagementDb` database
4. Check tables: `Users`, `Teams`, `Tasks`

## ?? Running the Application

### Using .NET CLI

```bash
# Navigate to project directory
cd TaskTeamManagementSystem

# Run the application
dotnet run
```

The application will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### Using Visual Studio

1. Open `TaskTeamManagementSystem.sln` in Visual Studio 2022
2. Press `F5` or click the "Run" button
3. The application will launch with the browser

### Using Visual Studio Code

1. Open the project folder in VS Code
2. Press `F5` or go to Run ? Start Debugging
3. Select `.NET Core` if prompted

### Watch Mode (Auto-reload on changes)

```bash
dotnet watch run
```

## ?? API Documentation

Once the application is running, access the Swagger UI:

**Swagger URL:** `https://localhost:5001/swagger`

### API Endpoints Overview

#### Authentication
- `POST /auth/login` - User login (returns JWT token)

#### Users
- `GET /users` - Get all users (Admin, Manager)
- `GET /users/{id}` - Get user by ID (Admin, Manager)
- `POST /users` - Create user (Admin)
- `PUT /users/{id}` - Update user (Admin)
- `DELETE /users/{id}` - Delete user (Admin)

#### Teams
- `GET /teams` - Get all teams
- `GET /teams/{id}` - Get team by ID
- `POST /teams` - Create team (Admin, Manager)
- `PUT /teams/{id}` - Update team (Admin, Manager)
- `DELETE /teams/{id}` - Delete team (Admin)

#### Tasks
- `GET /tasks` - Get all tasks (filtered by role)
- `GET /tasks/{id}` - Get task by ID
- `POST /tasks` - Create task (Admin, Manager)
- `PUT /tasks/{id}` - Update task (Admin, Manager, assigned Employee)
- `DELETE /tasks/{id}` - Delete task (Admin, Manager)
- `PATCH /tasks/{id}/status` - Update task status (Admin, Manager, assigned Employee)

## ?? Authentication

### Default Test Credentials

The system comes with pre-seeded users:

| Role | Email | Password | Description |
|------|-------|----------|-------------|
| **Admin** | admin@demo.com | Admin123! | Full system access |
| **Manager** | manager@demo.com | Manager123! | Manage teams and tasks |
| **Employee** | employee@demo.com | Employee123! | View/update assigned tasks |

### Authentication Flow

#### 1. Login to Get Token

**Request:**
```bash
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@demo.com",
    "password": "Admin123!"
  }'
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

#### 2. Use Token in Requests

**Include the token in the Authorization header:**

```bash
curl -X GET https://localhost:5001/tasks \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**In Swagger:**
1. Click "Authorize" button (??) at the top
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"
4. All requests will now include the token

**Token Details:**
- **Expiry:** 60 minutes (configurable)
- **Algorithm:** HMAC-SHA256
- **Claims:** User ID, Email, Name, Role

## ?? Testing

### Quick Test Script (PowerShell)

```powershell
# 1. Login
$loginBody = @{
    email = "admin@demo.com"
    password = "Admin123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "https://localhost:5001/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body $loginBody

$token = $loginResponse.token
Write-Host "Token: $token"

# 2. Get all tasks
$headers = @{
    "Authorization" = "Bearer $token"
}

$tasks = Invoke-RestMethod -Uri "https://localhost:5001/tasks" `
  -Method Get `
  -Headers $headers

Write-Host "Tasks: $($tasks | ConvertTo-Json)"
```

### Quick Test Script (Bash)

```bash
#!/bin/bash

# 1. Login
TOKEN=$(curl -s -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.com","password":"Admin123!"}' \
  | jq -r '.token')

echo "Token: $TOKEN"

# 2. Get all tasks
curl -X GET https://localhost:5001/tasks \
  -H "Authorization: Bearer $TOKEN"
```

### Using Postman

1. **Import Collection:**
   - Create a new collection: "Task Management System"
   
2. **Setup Environment:**
   - Create variable: `baseUrl` = `https://localhost:5001`
   - Create variable: `token` = (will be set automatically)

3. **Login Request:**
   - Method: POST
   - URL: `{{baseUrl}}/auth/login`
   - Body (JSON):
     ```json
     {
       "email": "admin@demo.com",
       "password": "Admin123!"
     }
     ```
   - Tests (auto-save token):
     ```javascript
     pm.test("Save token", function () {
         var jsonData = pm.response.json();
         pm.environment.set("token", jsonData.token);
     });
     ```

4. **Other Requests:**
   - Authorization: Bearer Token
   - Token: `{{token}}`

## ?? Project Structure

```
Task-Team-Management-System/
??? BuildingBlocks/
?   ??? BuildingBlocks/
?       ??? Behavior/           # MediatR behaviors
?       ??? CQRS/               # Command/Query interfaces
?
??? TaskTeamManagementSystem/
?   ??? Application/
?   ?   ??? Data/               # Database context interfaces
?   ?
?   ??? Authentication/         # JWT & Password hashing
?   ?   ??? JwtSettings.cs
?   ?   ??? JwtTokenService.cs
?   ?   ??? PasswordHasher.cs
?   ?
?   ??? Authorization/          # Authorization logic
?   ?   ??? AuthorizationService.cs
?   ?   ??? AuthorizationFilter.cs
?   ?   ??? IAuthorizationService.cs
?   ?
?   ??? Auth/
?   ?   ??? Login/              # Login endpoint & handler
?   ?
?   ??? Domain/
?   ?   ??? Models/             # Domain entities
?   ?       ??? User.cs
?   ?       ??? Team.cs
?   ?       ??? TaskItem.cs
?   ?       ??? Role.cs (enum)
?   ?
?   ??? Infrastructure/
?   ?   ??? Data/               # Database implementation
?   ?       ??? ApplicationDbContext.cs
?   ?       ??? Extensions/
?   ?           ??? InitialData.cs
?   ?           ??? DatabaseExtensions.cs
?   ?
?   ??? Users/                  # User endpoints & handlers
?   ?   ??? CreateUser/
?   ?   ??? GetUsers/
?   ?   ??? GetUserById/
?   ?   ??? UpdateUser/
?   ?   ??? DeleteUser/
?   ?
?   ??? Teams/                  # Team endpoints & handlers
?   ?   ??? CreateTeam/
?   ?   ??? GetTeams/
?   ?   ??? GetTeamById/
?   ?   ??? UpdateTeam/
?   ?   ??? DeleteTeam/
?   ?
?   ??? Tasks/                  # Task endpoints & handlers
?   ?   ??? CreateTask/
?   ?   ??? GetTasks/
?   ?   ??? GetTaskById/
?   ?   ??? UpdateTask/
?   ?   ??? DeleteTask/
?   ?   ??? UpdateTaskStatus/
?   ?
?   ??? Migrations/             # EF Core migrations
?   ??? Program.cs              # Application entry point
?   ??? appsettings.json        # Configuration
?   ??? TaskTeamManagementSystem.csproj
?
??? Documentation/
    ??? JWT_AUTHENTICATION_README.md
    ??? JWT_AUTHENTICATION_SUMMARY.md
    ??? QUICK_START_JWT.md
    ??? SWAGGER_JWT_OPTIONAL.md
    ??? JWT_AUTHENTICATION_CHECKLIST.md
```

## ?? Troubleshooting

### Issue: Database Connection Failed

**Error:** "Cannot open database"

**Solutions:**
1. Verify SQL Server is running:
   ```bash
   # Check SQL Server service
   Get-Service -Name "MSSQL*"
   ```

2. Test connection string in SSMS

3. Check firewall settings

4. Ensure correct server name in connection string

### Issue: Migration Failed

**Error:** "Unable to create migrations"

**Solutions:**
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Remove Migrations folder and recreate
Remove-Item .\Migrations -Recurse -Force
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Issue: Port Already in Use

**Error:** "Address already in use"

**Solutions:**
1. Change ports in `Properties/launchSettings.json`:
   ```json
   {
     "applicationUrl": "https://localhost:5002;http://localhost:5003"
   }
   ```

2. Or kill process using the port:
   ```bash
   # Windows
   netstat -ano | findstr :5001
   taskkill /PID <PID> /F
   
   # Linux/Mac
   lsof -i :5001
   kill -9 <PID>
   ```

### Issue: JWT Token Not Working

**Error:** "Unauthorized" when using token

**Checklist:**
- [ ] Token includes "Bearer " prefix
- [ ] Token is not expired (60 min default)
- [ ] Authorization header is correctly set
- [ ] JWT SecretKey matches in appsettings.json

### Issue: Password Not Working After Setup

**Error:** "Invalid email or password"

**Solution:** Database needs to be reset with hashed passwords:
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Issue: Swagger Not Loading

**Error:** 404 on /swagger

**Solutions:**
1. Ensure Development environment:
   ```bash
   $env:ASPNETCORE_ENVIRONMENT="Development"
   dotnet run
   ```

2. Check `Program.cs` has:
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }
   ```

### Issue: Build Errors

**Error:** Package restore failed

**Solutions:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild
dotnet clean
dotnet build
```

## ?? Security Best Practices

### For Development
- ? Default JWT secret is acceptable
- ? Swagger UI is enabled
- ? Detailed error messages are shown

### For Production
- ?? **MUST CHANGE** JWT SecretKey to a secure random string
- ?? Store secrets in environment variables or Azure Key Vault
- ?? Disable Swagger UI
- ?? Enable HTTPS only
- ?? Set appropriate CORS policies
- ?? Enable rate limiting
- ?? Add logging and monitoring
- ?? Use shorter token expiry (15-30 minutes)

### Production Configuration Example

```json
{
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com/api",
    "ExpiryInMinutes": "30"
  }
}
```

Set via environment variables:
```bash
export JwtSettings__SecretKey="your-secure-64-char-random-string"
export JwtSettings__ExpiryInMinutes="30"
```

## ?? Additional Documentation

- **JWT Authentication Guide:** [JWT_AUTHENTICATION_README.md](JWT_AUTHENTICATION_README.md)
- **Quick Start Guide:** [QUICK_START_JWT.md](QUICK_START_JWT.md)
- **Implementation Summary:** [JWT_AUTHENTICATION_SUMMARY.md](JWT_AUTHENTICATION_SUMMARY.md)
- **Swagger Integration:** [SWAGGER_JWT_OPTIONAL.md](SWAGGER_JWT_OPTIONAL.md)
- **Implementation Checklist:** [JWT_AUTHENTICATION_CHECKLIST.md](JWT_AUTHENTICATION_CHECKLIST.md)

## ?? Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ????? Author

**Faiz Ahmed**
- GitHub: [@faizahmed1314](https://github.com/faizahmed1314)

## ?? Support

If you encounter any issues or have questions:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review the additional documentation files
3. Open an issue on GitHub
4. Contact the maintainer

## ?? Acknowledgments

- Built with [.NET 10](https://dotnet.microsoft.com/)
- Uses [Carter](https://github.com/CarterCommunity/Carter) for minimal APIs
- Authentication with [JWT Bearer](https://jwt.io/)
- Documentation with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

---

**Happy Coding! ??**

For quick start instructions, see [QUICK_START_JWT.md](QUICK_START_JWT.md)
