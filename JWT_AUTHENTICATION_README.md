# JWT Authentication Implementation

## Overview
This document describes the JWT-based authentication implementation for the Task Team Management System.

## Features Implemented

### 1. **JWT Token Generation and Validation**
- `JwtTokenService` handles token creation and validation
- Tokens include user claims: ID, Email, Full Name, Role
- Configurable token expiry (default: 60 minutes)

### 2. **Password Hashing**
- `PasswordHasher` uses PBKDF2 with SHA256
- Salt-based hashing for secure password storage
- 100,000 iterations for enhanced security

### 3. **Login Endpoint**
- **POST** `/auth/login`
- Validates credentials and returns JWT token
- Returns user information along with token

### 4. **Updated Authorization**
- `AuthorizationService` now validates JWT tokens from Authorization header
- Backward compatible with X-User-Id header for testing
- Extracts user information from JWT claims

### 5. **Swagger Integration**
- Bearer token authentication in Swagger UI
- "Authorize" button to add JWT token
- All endpoints automatically include Authorization header

## Configuration

### appsettings.json
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

**Important:** Change the `SecretKey` in production to a secure random string.

## Usage

### 1. Login
```bash
POST /auth/login
Content-Type: application/json

{
  "email": "admin@demo.com",
  "password": "Admin123!"
}
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

### 2. Using the Token
Include the token in the Authorization header for all authenticated requests:

```bash
GET /tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Testing with Swagger
1. Click the "Authorize" button in Swagger UI
2. Enter: `Bearer {your-token-here}`
3. Click "Authorize"
4. All requests will now include the token

## Default Test Users

| Email | Password | Role |
|-------|----------|------|
| admin@demo.com | Admin123! | Admin |
| manager@demo.com | Manager123! | Manager |
| employee@demo.com | Employee123! | Employee |

## Security Features

### Password Hashing
- Uses PBKDF2 with SHA256
- 16-byte salt (randomly generated per password)
- 32-byte hash output
- 100,000 iterations

### JWT Security
- HMAC-SHA256 signing algorithm
- Token expiry validation
- Issuer and Audience validation
- No clock skew tolerance

### Password Requirements
When creating or updating users, passwords should:
- Be at least 8 characters (not enforced yet, but recommended)
- Include uppercase and lowercase letters
- Include numbers and special characters

## API Endpoints

### Authentication
- `POST /auth/login` - Login and get JWT token (Anonymous)

### Users (Protected)
- `GET /users` - Get all users (Admin, Manager)
- `GET /users/{id}` - Get user by ID (Admin, Manager)
- `POST /users` - Create user (Admin)
- `PUT /users/{id}` - Update user (Admin)
- `DELETE /users/{id}` - Delete user (Admin)

### Teams (Protected)
- All team endpoints require authentication
- Role-based access control applies

### Tasks (Protected)
- All task endpoints require authentication
- Employees can only see/modify their assigned tasks
- Managers and Admins have full access

## Migration Notes

### Database Update Required
Since passwords are now hashed, you need to reset the database:

```bash
# Drop and recreate the database
dotnet ef database drop --force
dotnet ef database update
```

Or manually delete the database and run the application in Development mode, which will automatically seed the database with hashed passwords.

### Backward Compatibility
The system maintains backward compatibility with the `X-User-Id` header for testing purposes. This will be removed in future versions.

## Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "User authentication required. Please provide a valid JWT token in the Authorization header."
}
```

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403,
  "detail": "User does not have the required role(s). Required: Admin, Manager"
}
```

## Future Enhancements

1. **Refresh Tokens** - Implement refresh token mechanism for token renewal
2. **Password Reset** - Add forgot password and reset functionality
3. **Email Verification** - Verify user email addresses
4. **Two-Factor Authentication** - Add 2FA support
5. **Token Revocation** - Implement token blacklisting for logout
6. **Password Policy** - Enforce stronger password requirements
7. **Rate Limiting** - Add rate limiting for login attempts
8. **Audit Logging** - Log authentication attempts and failures

## Troubleshooting

### Token Expired
If you receive a 401 error, your token may have expired. Login again to get a new token.

### Invalid Token
Ensure you're including "Bearer " before the token in the Authorization header.

### Password Not Working
If you're having trouble logging in after the update, the database needs to be reset with hashed passwords.

## Development vs Production

### Development
- Uses default JWT secret key
- Swagger UI enabled
- Detailed error messages

### Production
- **Must** change JWT SecretKey to a secure random string
- Store secrets in environment variables or Azure Key Vault
- Disable detailed error messages
- Enable HTTPS only
- Consider token expiry time (shorter for higher security)
