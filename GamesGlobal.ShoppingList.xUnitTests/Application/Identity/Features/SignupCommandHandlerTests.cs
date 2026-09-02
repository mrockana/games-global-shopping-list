using AutoFixture;
using GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features;

public sealed class SignupCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly IIdentityRepository _repository;

    public SignupCommandHandlerTests()
    {
        _repository = IdentityFixtures.Repository;
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_UserAlreadyExists_ReturnsValidationError()
    {
        // Arrange
        var existingUser = IdentityFixtures.BasicUser;

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var handler = new SignupCommandHandler(_repository);

        var command = new SignupCommand(
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>(),
            Email: IdentityFixtures.Email,
            Password: IdentityFixtures.Password,
            ConfirmPassword: IdentityFixtures.Password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainValidationException>(result.Error);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        // Arrange
        var user = IdentityFixtures.BasicUser;

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _repository.Insert(user).Returns(user);

        var handler = new SignupCommandHandler(_repository);

        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var command = new SignupCommand(
                        FirstName: user.FirstName!,
                        LastName: user.LastName!,
                        Email: IdentityFixtures.Email,
                        Password: IdentityFixtures.Password,
                        ConfirmPassword: IdentityFixtures.Password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to save user", result.Error?.Message);
    }

    [Fact]
    public async Task Handle_SuccessfulRegistration_ReturnsResultAndSaves()
    {
        // Arrange
        var command = new SignupCommand(
            FirstName: IdentityFixtures.BasicUser.FirstName!,
            LastName: IdentityFixtures.BasicUser.LastName!,
            Email: IdentityFixtures.BasicUser.Email!,
            Password: IdentityFixtures.Password,
            ConfirmPassword: IdentityFixtures.Password);

        var userEntity = IdentityFixtures.BasicUser;

        var insertedUser = userEntity; // For this test, assume insert returns the same user

        _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _repository.Insert(Arg.Any<User>()).Returns(insertedUser);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var handler = new SignupCommandHandler(_repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal(insertedUser.UserId, result.Value.UserId);
        Assert.Equal(insertedUser.FirstName, result.Value.FirstName);
        Assert.Equal(insertedUser.LastName, result.Value.LastName);
        Assert.Equal(insertedUser.Email, result.Value.Email);

        await _repository.Received(1).SaveAsync(CancellationToken.None);
    }

    [Theory]
    [ClassData(typeof(InvalidEmailClassData))]
    public void Validation_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var validator = new SignupRequestValidation();
        var command = new SignupCommand(
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>(),
            Email: email,
            Password: IdentityFixtures.Password,
            ConfirmPassword: IdentityFixtures.Password);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
        e.PropertyName.Equals(nameof(SignupCommand.Email), StringComparison.InvariantCulture));
    }

    [Theory]
    [ClassData(typeof(InvalidPasswordTestData))]
    public void Validation_WhenPasswordIsNullEmptyOrTooShort_ShouldHaveValidationError(string newPassword)
    {
        // Arrange
        var validator = new SignupRequestValidation();
        var command = new SignupCommand(
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>(),
            Email: IdentityFixtures.Email,
            Password: newPassword,
            ConfirmPassword: newPassword);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(command.Password), StringComparison.InvariantCulture));
    }

    [Theory]
    [ClassData(typeof(NullAndEmptyStringTestData))]
    public void Validation_WhenFirstNameIsEmptyOrNUll_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var validator = new SignupRequestValidation();
        var command = new SignupCommand(
            FirstName: firstName,
            LastName: _fixture.Create<string>(),
            Email: IdentityFixtures.Email,
            Password: IdentityFixtures.Password,
            ConfirmPassword: IdentityFixtures.Password);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(command.FirstName), StringComparison.InvariantCulture));
    }

    [Theory]
    [ClassData(typeof(NullAndEmptyStringTestData))]
    public void Validation_WhenLastnameIsEmptyOrNUll_ShouldHaveValidationError(string lastname)
    {
        // Arrange
        var validator = new SignupRequestValidation();
        var command = new SignupCommand(
            FirstName: _fixture.Create<string>(),
            LastName: lastname,
            Email: IdentityFixtures.Email,
            Password: IdentityFixtures.Password,
            ConfirmPassword: IdentityFixtures.Password);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(command.LastName), StringComparison.InvariantCulture));
    }

    [Fact]
    public void Validation_WhenConfirmPasswordDoesNotMatchPassword_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new SignupRequestValidation();
        var command = new SignupCommand(
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>(),
            Email: IdentityFixtures.Email,
            Password: IdentityFixtures.Password,
            ConfirmPassword: IdentityFixtures.AlternativePassword);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Equals(nameof(command.ConfirmPassword), StringComparison.InvariantCulture));
    }
}