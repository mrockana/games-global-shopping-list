using AutoFixture;
using GamesGlobal.ShoppingList.Application.Common.Pagination;
using GamesGlobal.ShoppingList.Application.Identity.Features.GetUsers;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class GetUsersQueryHandlerTests
{
    private readonly IIdentityRepository _repository;
    private readonly Fixture _fixture;

    public GetUsersQueryHandlerTests()
    {
        _repository = IdentityFixtures.Repository;
        _fixture = IdentityFixtures.Fixture;
    }

    [Fact]
    public async Task Handle_NoUsersFound_ReturnsNotFoundError()
    {
        // Arrange
        _repository.GetAsync<User>(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<User>>(new List<User>()));

        var handler = new GetUsersQueryHandler(_repository);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.NotNull(result.Error);
        Assert.IsType<DomainNotFoundException>(result.Error);
        Assert.Equal("No users found.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_UsersExist_ReturnsUsersWithRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { RoleId = 1, Name = _fixture.Create<string>() },
            new Role { RoleId = 2, Name = _fixture.Create<string>() },
        };

        var users = new List<User>
        {
            new User
            {
                UserId = _fixture.Create<long>(),
                FirstName = _fixture.Create<string>(),
                LastName = _fixture.Create<string>(),
                Email = _fixture.Create<string>(),
                Roles = roles,
            },
            new User
            {
                UserId = _fixture.Create<long>(),
                FirstName = _fixture.Create<string>(),
                LastName = _fixture.Create<string>(),
                Email = _fixture.Create<string>(),
                Roles = new List<Role>(),
            },
        };

        _repository.CountAsync<User>(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(users.Count));

        _repository.GetAsync<User>(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<User>>(users));

        var handler = new GetUsersQueryHandler(_repository);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.IsType<PaginatedResults<IList<GetUsersQueryResponse>>>(result.Value);
        Assert.Equal(2, result.Value.Data.Count);

        var firstUser = result.Value.Data[0];
        Assert.Equal(users[0].UserId, firstUser.UserId);
        Assert.Equal(users[0].FirstName, firstUser.FirstName);
        Assert.Equal(users[0].LastName, firstUser.LastName);
        Assert.Equal(users[0].Email, firstUser.Email);
        Assert.Equal(2, firstUser.Roles.Count);

        var secondUser = result.Value.Data[result.Value.Data.Count - 1];
        Assert.Empty(secondUser.Roles);
    }
}
