using GamesGlobal.ShoppingList.Application.Identity.Features.UpdateUserRoles;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Roles;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using GamesGlobal.ShoppingList.xUnitTests.Helpers;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class UpdateUserRolesCommandHandlerTests
{
    private readonly IIdentityRepository _repository;
    private readonly ILogger<UpdateUserRolesCommandHandler> _logger;

    public UpdateUserRolesCommandHandlerTests()
    {
        _logger = Substitute.For<ILogger<UpdateUserRolesCommandHandler>>();
        _repository = IdentityFixtures.Repository;
    }

    public static TheoryData<IList<long>?> NullOrEmptyRoleIdsData => new()
    {
        null as List<long>,
        new List<long>(),
    };

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnApplicationException()
    {
        // Arrange
        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new UpdateUserRolesCommandHandler(_repository, _logger);

        var command = new UpdateUserRolesCommand(
            UserId: 123,
            RoleIds: new List<long> { 1, 2 });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_WhenThereIsNewRolesToAdd_ReturnsMoreAddedRoles()
    {
        // Arrange
        var newRole1 = new Role { RoleId = 2, Name = "Admin" };
        var newRole2 = new Role { RoleId = 3, Name = "Manager" };
        var nonExistentRoleId = 4L;

        // Existing user with only RoleId 1
        var existingRole = new Role { RoleId = 1, Name = "User" };
        var user = new User
        {
            UserId = 100,
            Roles = new List<Role> { existingRole },
        };

        // RoleIds that are missing from existing user.Roles will be added (2 (Admin) & 3 (Manager) will be added)
        // RoleId 4 does not exist and should not break the flow
        var requestedRoleIds = new List<long> { 1, 2, 3, nonExistentRoleId };
        var command = new UpdateUserRolesCommand(user.UserId, requestedRoleIds);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _repository.GetSingleAsync(Arg.Is<FindRoleById>(t => t.Id == newRole1.RoleId), Arg.Any<CancellationToken>())
            .Returns(newRole1);

        _repository.GetSingleAsync(Arg.Is<FindRoleById>(t => t.Id == newRole2.RoleId), Arg.Any<CancellationToken>())
            .Returns(newRole2);

        // Mock: GetSingleAsync for non-existent role should return null
        _repository.GetSingleAsync(Arg.Is<FindRoleById>(t => t.Id == nonExistentRoleId), Arg.Any<CancellationToken>())
    .Returns((Role?)null);

        // SaveAsync and SavedSuccessful
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new UpdateUserRolesCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(user.UserId, result.Value.UserId);

        // The user's roles should now include 1, 2, 3
        var returnedRoleIds = result.Value.Roles.Select(r => r.RoleId).Order().ToList();
        Assert.Equal(new List<long> { 1, 2, 3 }, returnedRoleIds);

        // Verify that we logged the two new roles added and nothing was logged for the non-existent role
        AssertionHelper.AssertLoggerShouldLogMessage<UpdateUserRolesCommandHandler>(logger: _logger, message: $"Adding new User Role: {newRole1.RoleId.ToString()}");
        AssertionHelper.AssertLoggerShouldLogMessage<UpdateUserRolesCommandHandler>(logger: _logger, message: $"Adding new User Role: {newRole2.RoleId.ToString()}");

        await _repository.Received(1).GetSingleAsync(Arg.Is<FindRoleById>(t => t.Id == newRole1.RoleId), Arg.Any<CancellationToken>());
        await _repository.Received(1).GetSingleAsync(Arg.Is<FindRoleById>(t => t.Id == newRole2.RoleId), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenThereIsRolesToRemove_ReturnsLessUpdatedRoles()
    {
        // Arrange
        var existingRole1 = new Role { RoleId = 2, Name = "Admin" };
        var existingRole2 = new Role { RoleId = 3, Name = "Manager" };

        // This is the role we will delete
        var roleToDelete = new Role { RoleId = 1, Name = "User" };
        var user = new User
        {
            UserId = 100,
            Roles = new List<Role> { roleToDelete, existingRole1, existingRole2 },
        };

        // RoleId that are not included in the request will be removed, (RoleId 1 is not include and should be removed)
        var requestedRoleIds = new List<long> { 2, 3 };
        var command = new UpdateUserRolesCommand(user.UserId, requestedRoleIds);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // SaveAsync and SavedSuccessful
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new UpdateUserRolesCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(user.UserId, result.Value.UserId);

        // The user's roles should now include 2, 3
        var returnedRoleIds = result.Value.Roles.Select(r => r.RoleId).Order().ToList();
        Assert.Equal(new List<long> { 2, 3 }, returnedRoleIds);

        AssertionHelper.AssertLoggerShouldLogMessage<UpdateUserRolesCommandHandler>(logger: _logger, message: $"Removed the following Role {roleToDelete.RoleId.ToString()} for User {user.UserId.ToString()}");
        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsError()
    {
        // Arrange
        var user = new User
        {
            UserId = 100,
            Roles = new List<Role> { new Role { RoleId = 1, Name = "User" } },
        };

        var command = new UpdateUserRolesCommand(
            UserId: user.UserId,
            RoleIds: new List<long> { 1 });

        _repository.GetSingleAsync(Arg.Any<FindUserById>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // Mock: SaveAsync returns a value, but SavedSuccessful returns false
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var handler = new UpdateUserRolesCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Contains("Failed to update user roles.", result.Error.Message, StringComparison.InvariantCulture);
        Assert.Null(result.Value);
    }

    [Theory]
    [MemberData(nameof(NullOrEmptyRoleIdsData))]
    public async Task Handle_RoleIdsIsNullOrEmpty_RemovesAllRoles(IList<long>? roleIds)
    {
        // Arrange
        var user = new User
        {
            UserId = 200,
            Roles = new List<Role>
            {
                new Role { RoleId = 1, Name = "User" },
                new Role { RoleId = 2, Name = "Admin" },
            },
        };

        var command = new UpdateUserRolesCommand(user.UserId, roleIds);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new UpdateUserRolesCommandHandler(_repository, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(user.UserId, result.Value.UserId);
        Assert.Empty(result.Value.Roles);
    }

    [Theory]
    [ClassData(typeof(EmptyOrNegativePrimaryKeyId))]
    public void Validation_WhenInvalidUserId_ShouldHaveValidationError(long userId)
    {
        // Arrange
        var validator = new UpdateUserRolesValidation();
        var command = new UpdateUserRolesCommand(userId, new List<long> { 1, 2 });

        // Act
        var result = validator.Validate(command);

        // Assert
        result.AssertValidationShouldHaveError(nameof(command.UserId));
    }
}