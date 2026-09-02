using AutoFixture;
using GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly IIdentityRepository _repository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserHashGenerator _hashedTokenGenerator;
    private readonly IOptions<IdentityModuleOptions> _options;

    public RefreshTokenCommandHandlerTests()
    {
        _repository = IdentityFixtures.Repository;
        _jwtTokenGenerator = IdentityFixtures.JwtTokenGenerator;
        _hashedTokenGenerator = IdentityFixtures.HashedTokenGenerator;
        _options = IdentityFixtures.IdentittyModuleOptions;
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ReturnsForbiddenError()
    {
        // Arrange
        _repository.GetSingleAsync(Arg.Any<Specification<RefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var handler = new RefreshTokenCommandHandler(_repository, _jwtTokenGenerator, _options, _hashedTokenGenerator);
        var command = new RefreshTokenCommand(IdentityFixtures.HashedToken, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsForbiddenError()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new RefreshToken
        {
            LoginSessionId = _fixture.Create<long>(),
            UserId = _fixture.Create<long>(),
        };

        _repository.GetSingleAsync(Arg.Any<Specification<RefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
                .Returns((User?)null);

        var handler = new RefreshTokenCommandHandler(_repository, _jwtTokenGenerator, _options, _hashedTokenGenerator);
        var command = new RefreshTokenCommand(IdentityFixtures.HashedToken, sessionId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        // Arrange
        var userId = _fixture.Create<long>();
        var sessionId = Guid.NewGuid();
        var session = new RefreshToken
        {
            LoginSessionId = _fixture.Create<long>(),
            UserId = userId,
        };

        var user = new User
        {
            UserId = userId,
            Email = _fixture.Create<string>(),
            FirstName = _fixture.Create<string>(),
            LastName = _fixture.Create<string>(),
            Password = _fixture.Create<string>(),
        };

        _repository.GetSingleAsync(Arg.Any<Specification<RefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
                .Returns(user);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var handler = new RefreshTokenCommandHandler(_repository, _jwtTokenGenerator, _options, _hashedTokenGenerator);
        var command = new RefreshTokenCommand(IdentityFixtures.HashedToken, sessionId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
    }

    [Fact]
    public async Task Handle_ValidSessionAndUser_ReturnsRefreshedToken()
    {
        // Arrange
        var user = new User
        {
            UserId = _fixture.Create<long>(),
            UserCode = Guid.NewGuid(),
            Email = IdentityFixtures.Email,
            Roles = new List<Role>(),
        };

        var loginSession = new RefreshToken
        {
            LoginSessionId = _fixture.Create<long>(),
            UserId = user.UserId,
            Token = IdentityFixtures.HashedToken,
            ExpiryDate = DateTime.UtcNow.AddMinutes(-10),
        };

        _repository.GetSingleAsync(Arg.Any<Specification<RefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(loginSession);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
                .Returns(user);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        _hashedTokenGenerator.GenerateHashedToken(user)
            .Returns(IdentityFixtures.HashedToken);

        _jwtTokenGenerator.Generate(user.Email!, user.UserCode, Arg.Any<Permissions>())
            .Returns(IdentityFixtures.JWT);

        var handler = new RefreshTokenCommandHandler(_repository, _jwtTokenGenerator, _options, _hashedTokenGenerator);
        var command = new RefreshTokenCommand(IdentityFixtures.HashedToken, user.UserCode);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(IdentityFixtures.JWT, result.Value.Token);
        Assert.Equal(_options.Value.JwtExpiresInMinutes, result.Value.ExpiresInMinutes);
        Assert.Equal(IdentityFixtures.HashedToken, result.Value.RefreshToken);
        Assert.Equal(_options.Value.RefreshTokenExpiresInMinutes, result.Value.RefreshTokenExpiresInMinutes);

        await _repository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        _hashedTokenGenerator.Received(1).GenerateHashedToken(user);
        _jwtTokenGenerator.Received(1).Generate(user.Email!, user.UserCode, Arg.Any<Permissions>());
    }

    [Fact]
    public async Task Handle_WhenSessionBelongsToDifferentUser_ReturnsForbiddenError()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new RefreshToken
        {
            LoginSessionId = _fixture.Create<long>(),
            UserId = 1,
        };

        var user = new User
        {
            UserId = 321,
            Email = IdentityFixtures.Email,
            Roles = new List<Role>(),
        };

        _repository.GetSingleAsync(Arg.Any<Specification<RefreshToken>>(), Arg.Any<CancellationToken>())
            .Returns(session);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new RefreshTokenCommandHandler(_repository, _jwtTokenGenerator, _options, _hashedTokenGenerator);
        var command = new RefreshTokenCommand(IdentityFixtures.HashedToken, sessionId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
    }

    [Theory]
    [ClassData(typeof(NullAndEmptyStringTestData))]
    public void Validation_WhenRefreshTokenIsNullOrEmpty_ReturnsInvalid(string refreshToken)
    {
        // Arrange
        var validator = new RefreshTokenValidation();
        var command = new RefreshTokenCommand(refreshToken, Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(RefreshTokenCommand.RefreshToken), StringComparison.InvariantCulture));
    }
}
