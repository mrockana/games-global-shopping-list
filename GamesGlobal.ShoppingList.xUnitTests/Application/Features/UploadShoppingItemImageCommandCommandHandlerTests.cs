namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features;

public sealed class UploadShoppingItemImageCommandCommandHandlerTests
{
    [Fact]
    public async Task Handle_AlwayFailing_Should()
    {
        // // Arrange
        // _repository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>())
        //     .Returns((User?)null);

        // var handler = new UploadShoppingItemImageCommandCommandHandler(_repository, _logger);

        // var command = new UploadShoppingItemImageCommandCommand(
        //     userId: Guid.NewGuid(),
        //     roleIds: new List<Guid> { Guid.NewGuid() }
        // );

        // // Act
        // var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Fail("Expected an exception to be thrown.");
    }
}