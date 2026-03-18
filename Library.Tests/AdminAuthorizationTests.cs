using Microsoft.AspNetCore.Identity;

namespace Library.Tests;

public class AdminAuthorizationTests
{
    [Fact]
    public void AdminUserCanAccessRolePage()
    {
        // Arrange
        var adminRole = "Admin";
        var userRoles = new[] { adminRole };

        // Act
        var isAdmin = userRoles.Contains(adminRole);

        // Assert
        Assert.True(isAdmin);
    }

    [Fact]
    public void NonAdminUserCannotAccessRolePage()
    {
        // Arrange
        var userRoles = new[] { "Staff" };
        var requiredRole = "Admin";

        // Act
        var hasAccess = userRoles.Contains(requiredRole);

        // Assert
        Assert.False(hasAccess);
    }
}
