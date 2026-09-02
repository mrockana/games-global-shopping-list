using AutoFixture;
using FluentValidation.TestHelper;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Identity.Features.UpdateRolePermissions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.xUnitTests.Helpers;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class UpdateRolePermissionsCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly IIdentityRepository _repository;
    private readonly ILogger<UpdateRolePermissionsCommandHandler> _logger;

    public UpdateRolePermissionsCommandHandlerTests()
    {
        _logger = Substitute.For<ILogger<UpdateRolePermissionsCommandHandler>>();
        _repository = IdentityFixtures.Repository;
        _fixture = IdentityFixtures.Fixture;
    }

    public static TheoryData<IList<Permission>?> EmptyPermissionsData => new()
    {
        new List<Permission>(),
    };

    public static TheoryData<IList<long>?> NullOrEmptyRoleIdsData => new()
    {
        null as List<long>,
        new List<long>(),
    };

    [Fact]
    public async Task Handle_RoleNotFound_ShouldReturnNotFoundException()
    {
        // Arrange
        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var handler = new UpdateRolePermissionsCommandHandler(_repository, _logger);

        var permissions = new List<Permission>
        {
            new Permission(1),
            new Permission(2),
        };

        var command = new UpdateRolePermissionsCommand(
            RoleId: _fixture.Create<long>(),
            RoleName: _fixture.Create<string>(),
            Permissions: permissions);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.AssertIsErrorResult<UpdateRolePermissionsResponse>(
            typeof(DomainNotFoundException),
            "Role Not Found");
    }

    [Fact]
    public async Task Handle_WhenThereIsNewPermissionsToAdd_AddsPermissionsAndReturnsUpdatedRole()
    {
        // Arrange
        long roleId = 10L;
        RolePermission existingPermission = new RolePermission
        {
            RolePermissionId = 1,
            RoleId = roleId,
            Permission = Permissions.ShoppingItemsSelfReadOnly,
            PermissionName = Permissions.ShoppingItemsSelfReadOnly.GetDescription(),
        };
        string oldRole = "OldRoleName";

        var role = new Role
        {
            RoleId = roleId,
            Name = oldRole,
            RolePermissions = new List<RolePermission> { existingPermission },
        };

        // New permission to add
        var newPermission = new Permission((long)Permissions.UserRolesAndPermissionsReadWrite);

        var command = new UpdateRolePermissionsCommand(
            RoleId: roleId,
            RoleName: "TestRole", // Should update name
            Permissions: new List<Permission>
            {
                new Permission((long)Permissions.ShoppingItemsSelfReadOnly), // already exists
                newPermission, // new
            });

        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(role);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new UpdateRolePermissionsCommandHandler(_repository, _logger);

        // Act
        Result<UpdateRolePermissionsResponse>? result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(roleId, result.Value.RoleId);

        var returnedPermissionValues = result.Value.Permissions.Select(p => (long)p.Permission).Order().ToList();
        Assert.Equal(
            new List<long>
            {
                (long)Permissions.ShoppingItemsSelfReadOnly,
                (long)Permissions.UserRolesAndPermissionsReadWrite,
            },
            returnedPermissionValues);

        // Makes sure the name change happened and was logged
        AssertionHelper.AssertLoggerShouldLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Updating Role Name from {oldRole} to {command.RoleName}");

        // Makes we logged the new added permission
        AssertionHelper.AssertLoggerShouldLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Added new Role Permission: {Permissions.UserRolesAndPermissionsReadWrite.GetDescription()}");

        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenThereArePermissionsToRemove_RemovesPermissionsAndReturnsUpdatedRole()
    {
        // Arrange
        long roleId = 20L;
        var permissionToKeep = Permissions.ShoppingItemsSelfReadOnly;
        var permissionToRemove = Permissions.UserRolesAndPermissionsReadWrite;
        string oldRole = "OldRoleName";

        var existingPermission1 = new RolePermission
        {
            RolePermissionId = 1,
            RoleId = roleId,
            Permission = permissionToKeep,
            PermissionName = permissionToKeep.ToString(),
        };

        var existingPermission2Remove = new RolePermission
        {
            RolePermissionId = 2,
            RoleId = roleId,
            Permission = permissionToRemove,
            PermissionName = permissionToRemove.ToString(),
        };

        var role = new Role
        {
            RoleId = roleId,
            Name = oldRole,
            RolePermissions = new List<RolePermission> { existingPermission1, existingPermission2Remove },
        };

        // Command only includes permissionToKeep, Permissions not included here will be removed.
        var command = new UpdateRolePermissionsCommand(
            RoleId: roleId,
            RoleName: string.Empty,
            Permissions: new List<Permission>
            {
                new Permission((long)permissionToKeep),
            });

        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(role);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new UpdateRolePermissionsCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(roleId, result.Value.RoleId);

        var returnedPermissionValues = result.Value.Permissions.Select(p => (long)p.Permission).ToList();
        Assert.Single(returnedPermissionValues);
        Assert.Contains((long)permissionToKeep, returnedPermissionValues);
        Assert.DoesNotContain((long)permissionToRemove, returnedPermissionValues);

        // Verify if we logged the removed permission
        AssertionHelper.AssertLoggerShouldLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Removed the following Role Permissions: {existingPermission2Remove.RolePermissionId.ToString()}");

        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());

        // Makes sure the name change did not happened and nothing was logged
        AssertionHelper.AssertLoggerShouldNotLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Updating Role Name from {oldRole} to {command.RoleName}");
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        // Arrange
        long roleId = 30L;
        var permission = Permissions.ShoppingItemsSelfReadOnly;

        var existingPermission = new RolePermission
        {
            RolePermissionId = 1,
            RoleId = roleId,
            Permission = permission,
            PermissionName = permission.GetDescription(),
        };

        var role = new Role
        {
            RoleId = roleId,
            Name = "RoleName",
            RolePermissions = new List<RolePermission> { existingPermission },
        };

        var command = new UpdateRolePermissionsCommand(
            RoleId: roleId,
            RoleName: "RoleName",
            Permissions: new List<Permission>
            {
            new Permission((long)permission),
            });

        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(role);

        // Simulate SaveAsync returns 0 and SavedSuccessful returns false
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var handler = new UpdateRolePermissionsCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to update Role Permissions.", result.Error?.Message);
        Assert.Null(result.Value);

        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoChangesNeeded_ReturnsSuccessWithoutModifications()
    {
        // Arrange
        long roleId = 40L;
        var permission = Permissions.ShoppingItemsSelfReadOnly;
        string roleName = "UnchangedRole";

        var existingPermission = new RolePermission
        {
            RolePermissionId = 1,
            RoleId = roleId,
            Permission = permission,
            PermissionName = permission.GetDescription(),
        };

        var role = new Role
        {
            RoleId = roleId,
            Name = roleName,
            RolePermissions = new List<RolePermission> { existingPermission },
        };

        var command = new UpdateRolePermissionsCommand(
            RoleId: roleId,
            RoleName: roleName, // Same as current
            Permissions: new List<Permission>
            {
                new Permission((long)permission),
            });

        _repository.GetSingleAsync(Arg.Any<Specification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(role);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new UpdateRolePermissionsCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(roleId, result.Value.RoleId);
        Assert.Equal(roleName, result.Value.Name);

        var returnedPermissionValues = result.Value.Permissions.Select(p => (long)p.Permission).ToList();
        Assert.Single(returnedPermissionValues);
        Assert.Contains((long)permission, returnedPermissionValues);

        // No insert or delete should be called
        _repository.DidNotReceive().Insert<RolePermission>(Arg.Any<RolePermission>());
        _repository.DidNotReceive().DeleteRange(Arg.Any<IEnumerable<RolePermission>>());

        // No log for name change or permission add/remove
        AssertionHelper.AssertLoggerShouldNotLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Updating Role Name from {roleName} to {command.RoleName}");
        AssertionHelper.AssertLoggerShouldNotLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Added new Role Permission: {permission.GetDescription()}");
        AssertionHelper.AssertLoggerShouldNotLogMessage<UpdateRolePermissionsCommandHandler>(
            logger: _logger,
            message: $"Removed the following Role Permissions: {existingPermission.RolePermissionId}");

        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [ClassData(typeof(EmptyOrNegativePrimaryKeyId))]
    public void Validation_WhenInvalidRoleId_ShouldHaveValidationError(long roleId)
    {
        // Arrange
        var validator = new UpdateRolePermissionsValidation();

        var command = new UpdateRolePermissionsCommand(
            RoleId: roleId,
            RoleName: "RoleName",
            Permissions: new List<Permission>
            {
            new Permission(4),
            });

        // Act
        var result = validator.Validate(command);

        // Assert
        result.AssertValidationShouldHaveError(nameof(command.RoleId));
    }

    [Theory]
    [MemberData(nameof(EmptyPermissionsData))]
    public void Validation_PermissionsIsNullOrEmpty_ShouldHaveValidationError(IList<Permission>? permissions)
    {
        // Arrange
        var validator = new UpdateRolePermissionsValidation();
        var command = new UpdateRolePermissionsCommand(
            RoleId: 1,
            RoleName: "TestRole",
            Permissions: permissions!);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Permissions);
    }
}