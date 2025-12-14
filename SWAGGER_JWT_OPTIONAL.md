# Optional: Swagger JWT Configuration

If you want to enable JWT authentication in Swagger UI (with the "Authorize" button), you can update your `Program.cs` file.

## Option 1: Using Swashbuckle.AspNetCore 6.x (Recommended)

First, downgrade Swashbuckle to a stable version that works with .NET 10:

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
```

Then update Program.cs:

```csharp
using Microsoft.OpenApi.Models;

// ... existing code ...

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Team Management API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

## Option 2: Manual Token Entry (Current Implementation)

The current implementation works but requires manual token entry:

1. Login via `/auth/login` endpoint
2. Copy the token from the response
3. For each request, add header manually:
   - Header Name: `Authorization`
   - Header Value: `Bearer YOUR_TOKEN_HERE`

## Option 3: Use Postman or Insomnia

These API testing tools have better JWT support:

### Postman Setup:
1. Create a login request to `/auth/login`
2. Save the token from response
3. In Collection/Folder settings, add:
   - Type: Bearer Token
   - Token: `{{auth_token}}`
4. Use a pre-request script to auto-extract token:
```javascript
pm.test("Save token", function () {
    var jsonData = pm.response.json();
    pm.environment.set("auth_token", jsonData.token);
});
```

### Insomnia Setup:
1. Create a login request
2. In other requests, set Auth to "Bearer Token"
3. Use environment variable for token

## Testing the Implementation

### 1. Test Login
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@demo.com",
    "password": "Admin123!"
  }'
```

Expected Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@demo.com",
  "fullName": "mehmet",
  "role": "Admin",
  "userId": 1
}
```

### 2. Test Protected Endpoint
```bash
TOKEN="your_token_here"

curl -X GET http://localhost:5000/tasks \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Test Without Token (Should Fail)
```bash
curl -X GET http://localhost:5000/tasks
```

Expected Error:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "User authentication required. Please provide a valid JWT token in the Authorization header."
}
```

### 4. Test With Expired Token (After 60 minutes)
Should return 401 Unauthorized

### 5. Test With Invalid Token
```bash
curl -X GET http://localhost:5000/tasks \
  -H "Authorization: Bearer invalid_token"
```

Should return 401 Unauthorized

## Troubleshooting

### Issue: OpenApi.Models namespace not found
**Solution**: Use Swashbuckle.AspNetCore 6.x instead of 10.x

```bash
dotnet add package Swashbuckle.AspNetCore --version 6.8.1
dotnet restore
dotnet build
```

### Issue: Token not accepted
**Checklist**:
- [ ] Token starts with "Bearer " (with space)
- [ ] Token is not expired (60 minutes default)
- [ ] JWT SecretKey matches in configuration
- [ ] Issuer and Audience match configuration

### Issue: Password verification fails after migration
**Solution**: Reset database to use hashed passwords
```bash
dotnet ef database drop --force
dotnet ef database update
```

## Alternative: Keep Current Setup

The current implementation works perfectly without Swagger JWT support. You can:

1. Use Swagger only for API documentation
2. Test authentication using:
   - Postman
   - Insomnia
   - curl
   - Custom front-end application

The JWT implementation is complete and functional - Swagger integration is purely optional for convenience!
