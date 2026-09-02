using AutoFixture;
using GamesGlobal.ShoppingList.Application.Identity.Features.Login;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.RefreshToken;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class LoginCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly IIdentityRepository _repository;
    private readonly IJwtTokenGenerator _generateJwtTokenService;
    private readonly IRefreshTokenService _loginSessionCreator;
    private readonly IOptions<IdentityModuleOptions> _options;

    public LoginCommandHandlerTests()
    {
        _repository = IdentityFixtures.Repository;
        _generateJwtTokenService = IdentityFixtures.JwtTokenGenerator;
        _options = IdentityFixtures.IdentittyModuleOptions;
        _fixture = new Fixture();
        _loginSessionCreator = IdentityFixtures.LoginSessionCreator;
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new LoginCommandHandler(_repository, _generateJwtTokenService, _options, _loginSessionCreator);
        var command = new SessionLoginCommand(IdentityFixtures.Username, IdentityFixtures.Password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainUnauthorizedAccessException>(result.Error);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ShouldReturnUnauthorized()
    {
        // Arrange
        User user = new User
        {
            Created = DateTime.UtcNow,
            Email = IdentityFixtures.Username,
            LastName = _fixture.Create<string>(),
            UserId = _fixture.Create<long>(),
            FirstName = _fixture.Create<string>(),
        };

        user.SetUserPassword(IdentityFixtures.Password);

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var handler = new LoginCommandHandler(_repository, _generateJwtTokenService, _options, _loginSessionCreator);
        var command = new SessionLoginCommand(IdentityFixtures.Username, IdentityFixtures.AlternativePassword);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainUnauthorizedAccessException>(result.Error);
    }

    [Fact]
    public async Task Handle_WhenCreateLoginSessionFails_ShouldReturnApplicationException()
    {
        // Arrange
        User user = new User
        {
            Created = DateTime.UtcNow,
            Email = IdentityFixtures.Username,
            LastName = _fixture.Create<string>(),
            UserId = _fixture.Create<long>(),
            FirstName = _fixture.Create<string>(),
        };

        user.SetUserPassword(IdentityFixtures.Password);
        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _loginSessionCreator.CreateRefreshToken(_repository, user).Returns(Task.FromResult(((RefreshToken?)null, false)));

        var handler = new LoginCommandHandler(_repository, _generateJwtTokenService, _options, _loginSessionCreator);
        var command = new SessionLoginCommand(IdentityFixtures.Username, IdentityFixtures.Password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to create login session", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldReturnSessionLoginResponse()
    {
        // Arrange
        var user = new User
        {
            Created = DateTime.UtcNow,
            Email = IdentityFixtures.Username,
            LastName = _fixture.Create<string>(),
            UserId = _fixture.Create<long>(),
            FirstName = _fixture.Create<string>(),
            Roles = new List<Role>(),
        };
        user.SetUserPassword(IdentityFixtures.Password);

        var loginSession = new RefreshToken
        {
            Token = _fixture.Create<string>(),
        };

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _loginSessionCreator.CreateRefreshToken(_repository, user).Returns(Task.FromResult<(RefreshToken?, bool)>((loginSession, true)));
        _generateJwtTokenService.Generate(user.Email, user.UserCode, Arg.Any<Permissions>())
            .Returns("mocked-jwt-token");

        var handler = new LoginCommandHandler(_repository, _generateJwtTokenService, _options, _loginSessionCreator);
        var command = new SessionLoginCommand(IdentityFixtures.Username, IdentityFixtures.Password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal("mocked-jwt-token", result.Value.Token);
        Assert.Equal(_options.Value.JwtExpiresInMinutes, result.Value.ExpiresInMinutes);
        Assert.Equal(loginSession.Token, result.Value.RefreshToken);
        Assert.Equal(_options.Value.RefreshTokenExpiresInMinutes, result.Value.RefreshTokenExpiresInMinutes);
    }

    [Fact]
    public void Validation_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new LoginValidation();
        var command = new SessionLoginCommand(string.Empty, IdentityFixtures.Password);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Equals(nameof(SessionLoginCommand.Username), StringComparison.InvariantCulture));
    }

    [Fact]
    public void Validation_WhenUsernameIsInvalidEmail_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new LoginValidation();
        var command = new SessionLoginCommand(IdentityFixtures.InvalidEmail, IdentityFixtures.Password);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
        e.PropertyName.Equals(nameof(SessionLoginCommand.Username), StringComparison.InvariantCulture) &&
        e.ErrorMessage.Contains("Invalid email format", StringComparison.InvariantCulture));
    }

    [Fact]
    public void Validation_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new LoginValidation();
        var command = new SessionLoginCommand(IdentityFixtures.Username, string.Empty);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Equals(nameof(SessionLoginCommand.Password), StringComparison.InvariantCulture));
    }

    [Fact]
    public void Validation_WhenPasswordIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new LoginValidation();
        var shortPassword = "short";
        var command = new SessionLoginCommand(IdentityFixtures.Username, shortPassword);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(SessionLoginCommand.Password), StringComparison.InvariantCulture) &&
            e.ErrorMessage.Contains($"The length of '{nameof(SessionLoginCommand.Password)}' must be at least", StringComparison.InvariantCulture));
    }

    [Fact]
    public void Validation_WhenPasswordAndUsernameAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var validator = new LoginValidation();
        var command = new SessionLoginCommand(IdentityFixtures.Username, IdentityFixtures.Password);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
