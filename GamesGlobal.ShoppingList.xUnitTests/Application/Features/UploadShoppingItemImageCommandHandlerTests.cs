using System.Text;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Features;
using GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImage;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features;

public sealed class UploadShoppingItemImageCommandHandlerTests
{
    private const string BucketName = "shopping-item-images";

    private readonly IApplicationRepository _repository = Substitute.For<IApplicationRepository>();
    private readonly IIdentityRepository _identityRepository = Substitute.For<IIdentityRepository>();
    private readonly IFileObjectStoreService _fileObjectStoreService = Substitute.For<IFileObjectStoreService>();
    private readonly ILogger<UploadShoppingItemImageCommandHandler> _logger = Substitute.For<ILogger<UploadShoppingItemImageCommandHandler>>();
    private readonly FileObjectStoreOptions _options = new() { Url = "http://localhost:9000", BucketName = BucketName };
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsApplicationException()
    {
        var command = CreateCommand(Guid.NewGuid(), shoppingItemId: 1);
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await CreateHandler().Handle(command);

        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        await _fileObjectStoreService.DidNotReceiveWithAnyArgs().UploadObject(default!, default!, default!, default!, default, default);
    }

    [Fact]
    public async Task Handle_ShoppingItemDoesNotExist_ReturnsNotFoundException()
    {
        Guid userCode = Guid.NewGuid();
        var command = CreateCommand(userCode, shoppingItemId: 1);
        GivenUser(userCode);
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns((ShoppingItem?)null);

        var result = await CreateHandler().Handle(command);

        Assert.True(result.HasError);
        Assert.IsType<DomainNotFoundException>(result.Error);
        await _fileObjectStoreService.DidNotReceiveWithAnyArgs().UploadObject(default!, default!, default!, default!, default, default);
    }

    [Fact]
    public async Task Handle_ShoppingItemBelongsToAnotherUser_ReturnsForbiddenExceptionAndDoesNotUpload()
    {
        Guid userCode = Guid.NewGuid();
        var command = CreateCommand(userCode, shoppingItemId: 5);
        GivenUser(userCode);
        GivenShoppingItem(shoppingItemId: 5, ownerUserCode: Guid.NewGuid());

        var result = await CreateHandler().Handle(command);

        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
        await _fileObjectStoreService.DidNotReceiveWithAnyArgs().UploadObject(default!, default!, default!, default!, default, default);
    }

    [Fact]
    public async Task Handle_BucketDoesNotExist_CreatesBucketOnce()
    {
        Guid userCode = Guid.NewGuid();
        var command = CreateCommand(userCode, shoppingItemId: 5);
        GivenUser(userCode);
        GivenShoppingItem(shoppingItemId: 5, ownerUserCode: userCode);
        GivenSuccessfulSave();
        _fileObjectStoreService.BucketExists(BucketName, Arg.Any<CancellationToken>()).Returns(false);

        await CreateHandler().Handle(command);

        await _fileObjectStoreService.Received(1).CreateBucket(BucketName);
    }

    [Fact]
    public async Task Handle_BucketAlreadyExists_DoesNotCreateBucket()
    {
        Guid userCode = Guid.NewGuid();
        var command = CreateCommand(userCode, shoppingItemId: 5);
        GivenUser(userCode);
        GivenShoppingItem(shoppingItemId: 5, ownerUserCode: userCode);
        GivenSuccessfulSave();
        _fileObjectStoreService.BucketExists(BucketName, Arg.Any<CancellationToken>()).Returns(true);

        await CreateHandler().Handle(command);

        await _fileObjectStoreService.DidNotReceiveWithAnyArgs().CreateBucket(default!);
    }

    [Fact]
    public async Task Handle_ValidRequest_UploadsUnderUserScopedKeyAndReturnsResponse()
    {
        Guid userCode = Guid.NewGuid();
        var command = CreateCommand(userCode, shoppingItemId: 5, fileName: "brownie.png", contentType: "image/png");
        GivenUser(userCode);
        ShoppingItem item = GivenShoppingItem(shoppingItemId: 5, ownerUserCode: userCode);
        GivenSuccessfulSave();
        _fileObjectStoreService.BucketExists(BucketName, Arg.Any<CancellationToken>()).Returns(true);

        string expectedKey = $"{userCode}/5/brownie.png";

        var result = await CreateHandler().Handle(command);

        Assert.False(result.HasError);
        Assert.Equal(5, result.Value?.ShoppingItemId);
        Assert.Equal("brownie.png", result.Value?.Name);
        Assert.Equal("image/png", result.Value?.MimeType);
        Assert.Equal((int)command.Length, result.Value?.Size);
        Assert.Equal($"http://localhost:9000/{BucketName}/{expectedKey}", result.Value?.Url);

        await _fileObjectStoreService.Received(1).UploadObject(BucketName, expectedKey, command.Content, "image/png", command.Length, Arg.Any<CancellationToken>());
        Assert.Single(item.Documents);
        await _cacheService.Received(1).RemoveAsync(ShoppingItemCacheKeys.ForUser(userCode), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaveFails_RemovesUploadedObjectAndReturnsApplicationException()
    {
        Guid userCode = Guid.NewGuid();
        var command = CreateCommand(userCode, shoppingItemId: 5, fileName: "brownie.png");
        GivenUser(userCode);
        GivenShoppingItem(shoppingItemId: 5, ownerUserCode: userCode);
        _fileObjectStoreService.BucketExists(BucketName, Arg.Any<CancellationToken>()).Returns(true);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var result = await CreateHandler().Handle(command);

        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        await _fileObjectStoreService.Received(1).RemoveObject(BucketName, $"{userCode}/5/brownie.png", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("image/png", true)]
    [InlineData("image/jpeg", true)]
    [InlineData("image/gif", false)]
    [InlineData("image/jpg", false)]
    [InlineData("application/pdf", false)]
    [InlineData("text/plain", false)]
    public void Validation_ContentType_IsRestrictedToPngAndJpeg(string contentType, bool expectedIsValid)
    {
        var command = CreateCommand(Guid.NewGuid(), shoppingItemId: 1, contentType: contentType);

        var result = new UploadShoppingItemImageValidation(_options).Validate(command);

        Assert.Equal(expectedIsValid, result.IsValid);
    }

    private static UploadShoppingItemImageCommandRequest CreateCommand(
        Guid userCode,
        long shoppingItemId,
        string fileName = "brownie.png",
        string contentType = "image/png")
    {
        byte[] bytes = Encoding.UTF8.GetBytes("image-bytes");
        return new UploadShoppingItemImageCommandRequest(userCode, shoppingItemId, fileName, new MemoryStream(bytes), contentType, bytes.Length);
    }

    private UploadShoppingItemImageCommandHandler CreateHandler()
    {
        return new UploadShoppingItemImageCommandHandler(_repository, _identityRepository, _fileObjectStoreService, _options, _logger, _cacheService);
    }

    private void GivenUser(Guid userCode)
    {
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns(new User { UserCode = userCode });
    }

    private ShoppingItem GivenShoppingItem(long shoppingItemId, Guid ownerUserCode)
    {
        var item = new ShoppingItem { ShoppingItemId = shoppingItemId, UserCode = ownerUserCode, Name = "Brownie", Description = "Chocolate delight" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);
        return item;
    }

    private void GivenSuccessfulSave()
    {
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);
    }
}
