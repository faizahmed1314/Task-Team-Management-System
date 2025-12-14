using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tests.Authentication;

public class JwtTokenServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly IJwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        _jwtTokenService = new JwtTokenService(_jwtSettings);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = Role.Admin
        };

        // Act
        var token = _jwtTokenService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = Role.Manager
        };
        var token = _jwtTokenService.GenerateToken(user);

        // Act
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Email = "admin@test.com",
            FullName = "Admin User",
            Password = "hashedPassword",
            Role = Role.Admin
        };
        var token = _jwtTokenService.GenerateToken(user);

        // Act
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var claims = principal!.Claims.ToList();
        
        // Check for sub claim (standard JWT claim)
        var subClaim = claims.FirstOrDefault(c => c.Type == "sub" || 
                                                   c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be("42");
        
        // Check for email claim
        var emailClaim = claims.FirstOrDefault(c => c.Type == "email" || 
                                                     c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be("admin@test.com");
        
        // Check for name claim
        var nameClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
        nameClaim.Should().NotBeNull();
        nameClaim!.Value.Should().Be("Admin User");
        
        // Check for role claim
        var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be("Admin");
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtTokenService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var expiredSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = -1 // Expired
        };
        var expiredTokenService = new JwtTokenService(expiredSettings);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = Role.Employee
        };
        var expiredToken = expiredTokenService.GenerateToken(user);

        // Act
        var principal = _jwtTokenService.ValidateToken(expiredToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongSecretKey_ShouldReturnNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = Role.Admin
        };
        var token = _jwtTokenService.GenerateToken(user);

        var wrongSettings = new JwtSettings
        {
            SecretKey = "DifferentSecretKeyThatIsAtLeast32CharactersLong!!!!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        var wrongTokenService = new JwtTokenService(wrongSettings);

        // Act
        var principal = wrongTokenService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Manager)]
    [InlineData(Role.Employee)]
    public void GenerateToken_WithDifferentRoles_ShouldIncludeCorrectRole(Role role)
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = role
        };

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var roleClaim = principal!.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(role.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldCreateUniqueTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = Role.Admin
        };

        // Act
        var token1 = _jwtTokenService.GenerateToken(user);
        var token2 = _jwtTokenService.GenerateToken(user);

        // Assert
        token1.Should().NotBe(token2, "tokens should have unique JTI claims");
    }
}
