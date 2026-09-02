using AutoFixture;
using GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class AddRoleCommandHandlerTests
{
    private readonly IIdentityRepository _repository;

    public AddRoleCommandHandlerTests()
    {
        _repository = IdentityFixtures.Repository;
    }

    public static IEnumerable<object?[]> InvalidPermissionsRequestData =>
                    new List<object?[]>
                    {
                        new object?[] { null },
                        new object?[] { new List<AdddRolePermission>() },
                    };

    [Fact]
    public async Task Handle_RoleAlreadyExists_ReturnsApplicationException()
    {
        // Arrange
        string roleName = IdentityFixtures.Fixture.Create<string>();

        Role existingRole = new Role { Name = roleName };
        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(existingRole);

        var handler = new AddRoleCommandHandler(_repository);
        var command = new AddRoleCommand(
            RoleName: roleName,
            Permissions: new List<AdddRolePermission> { new AdddRolePermission(1) });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Contains("Role with the same name already exists",
            result.Error?.Message,
            StringComparison.InvariantCulture);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        // Arrange
        string roleName = IdentityFixtures.Fixture.Create<string>();

        // Simulate no existing role
        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        // Simulate insert returns a new role
        var insertedRole = new Role
        {
            RoleId = 123,
            Name = roleName,
            RolePermissions = new List<RolePermission>(),
        };
        _repository.Insert(Arg.Any<Role>()).Returns(insertedRole);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var handler = new AddRoleCommandHandler(_repository);
        var command = new AddRoleCommand(
            RoleName: roleName,
            Permissions: new List<AdddRolePermission> { new AdddRolePermission(1) });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to add role.", result.Error?.Message);
    }

    [Fact]
    public async Task Handle_ValidRequest_AddsRoleSuccessfully()
    {
        // Arrange
        string roleName = IdentityFixtures.Fixture.Create<string>();
        long roleId = IdentityFixtures.Fixture.Create<long>();
        long firstPermission = 1;
        long secondPermission = 2;

        var permissions = new List<AdddRolePermission>
    {
        new AdddRolePermission(firstPermission),
        new AdddRolePermission(secondPermission),
    };

        // Simulate no existing role
        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        // Simulate insert returns a new role with permissions
        var insertedRole = new Role
        {
            RoleId = roleId,
            Name = roleName,
            RolePermissions = new List<RolePermission>
        {
            new RolePermission { Permission = (Permissions)1, PermissionName = "Permission1" },
            new RolePermission { Permission = (Permissions)2, PermissionName = "Permission2" },
        },
        };
        _repository.Insert(Arg.Any<Role>()).Returns(insertedRole);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new AddRoleCommandHandler(_repository);
        var command = new AddRoleCommand(
            RoleName: roleName,
            Permissions: permissions);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(roleId, result.Value.RoleId);
        Assert.Equal(insertedRole.Name, result.Value.Name);
        Assert.NotNull(result.Value.Permissions);
        Assert.Equal(2, result.Value.Permissions.Count);
        Assert.Contains(result.Value.Permissions, p => p.Permission == (Permissions)firstPermission);
        Assert.Contains(result.Value.Permissions, p => p.Permission == (Permissions)secondPermission);
    }

    [Theory]
    [ClassData(typeof(NullAndEmptyStringTestData))]
    public void Validation_RoleNameIsNullOrEmpty_ReturnsInvalid(string roleName)
    {
        // Arrange
        var validator = new AddRoleCommandValidation();
        var command = new AddRoleCommand(
            RoleName: roleName,
            Permissions: new List<AdddRolePermission> { new AdddRolePermission(1) });

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(AddRoleCommand.RoleName), StringComparison.InvariantCulture));
    }

    [Theory]
    [MemberData(nameof(InvalidPermissionsRequestData))]
    public void Validation_PermissionsIsNullOrEmpty_ReturnsInvalid(IList<AdddRolePermission> permissions)
    {
        // Arrange
        var validator = new AddRoleCommandValidation();
        var command = new AddRoleCommand(
            RoleName: "ValidRoleName",
            Permissions: permissions);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(AddRoleCommand.Permissions), StringComparison.InvariantCulture));
    }
}
