using AutoFixture;
using GamesGlobal.ShoppingList.Application.Identity.Features.GetRoles;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class GetRolesQueryHandlerTests
{
    private readonly IIdentityRepository _repository;
    private readonly Fixture _fixture;

    public GetRolesQueryHandlerTests()
    {
        _repository = IdentityFixtures.Repository;
        _fixture = IdentityFixtures.Fixture;
    }

    [Fact]
    public async Task Handle_RolesExist_ReturnsAllRolesWithPermissions()
    {
        // Arrange
        // Mock roles and permissions
        var role1 = new Role
        {
            RoleId = 1,
            Name = "Admin",
            RolePermissions = new List<RolePermission>
            {
                new RolePermission { Permission = Permissions.ShoppingItemsSelfReadWrite, PermissionName = _fixture.Create<string>() },
                new RolePermission { Permission = Permissions.UserRolesAndPermissionsReadWrite, PermissionName = _fixture.Create<string>() },
            },
        };
        var role2 = new Role
        {
            RoleId = 3,
            Name = "Guest",
            RolePermissions = null, // No permissions
        };

        IList<Role> roles = new List<Role>
        {
            role1,
            role2,
        };

        _repository.GetAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(roles));

        var handler = new GetRolesQueryHandler(_repository);
        var query = new GetRolesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);

        var admin = result.Value.FirstOrDefault(r => r.RoleId == 1);
        Assert.NotNull(admin);
        Assert.Equal("Admin", admin.Name);
        Assert.Equal(2, admin.Permissions.Count(p => p.Enabled));

        var guest = result.Value.FirstOrDefault(r => r.RoleId == 3);
        Assert.NotNull(guest);
        Assert.Equal("Guest", guest.Name);
        Assert.DoesNotContain(guest.Permissions, p => p.Enabled);
    }
}
