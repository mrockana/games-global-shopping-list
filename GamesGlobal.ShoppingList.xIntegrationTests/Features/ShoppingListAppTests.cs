using System.Net.Http.Headers;
using System.Net.Http.Json;
using GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;
using GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;
using GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;
using GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;
using GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImage;
using GamesGlobal.ShoppingList.Application.Identity.Features.Login;
using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Args;

namespace GamesGlobal.ShoppingList.xIntegrationTests.Features;

[Collection(nameof(ShoppingListAppTests))]
public sealed class ShoppingListAppTests : IClassFixture<GamesGlobalWebApiFactory>
{
    private static readonly SessionLoginCommand LoginRequest = new(
        Username: "johndoe@example.gamesglobal",
        Password: "123Abc123@");

    private static readonly int ShoppingItemId = 8;

    private static readonly byte[] OnePixelPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static Task<LoginResponse>? _loginResponseTask;
    private readonly HttpClient _apiClient;
    private readonly GamesGlobalWebApiFactory _factory;

    public ShoppingListAppTests(GamesGlobalWebApiFactory factory)
    {
        _factory = factory;
        _apiClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidRequest_OkWithCorrectLoginResponse()
    {
        var loginResponse = await GetLoginResponseAsync();

        Assert.NotEmpty(loginResponse.Token);
        Assert.NotEmpty(loginResponse.RefreshToken);
        Assert.True(loginResponse.ExpiresInMinutes > 0);
        Assert.True(loginResponse.RefreshTokenExpiresInMinutes > 0);
    }

    [Fact]
    public async Task GetShopping_WithValidRequest_OkWithItems()
    {
        var loginResponse = await GetLoginResponseAsync();

        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        var result = await _apiClient.GetAsync("/shopping-items");
        result.EnsureSuccessStatusCode();

        var shoppingItems = await result.Content.ReadFromJsonAsync<IList<GetShoppingItemResponse>>();

        Assert.NotNull(shoppingItems);
        Assert.NotEmpty(shoppingItems);
    }

    [Fact]
    public async Task CreateShoppingItem_WithValidRequest_CreatesPersistedItem()
    {
        var loginResponse = await GetLoginResponseAsync();
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var itemName = $"create-{Guid.NewGuid():N}";

        var result = await _apiClient.PostAsJsonAsync("/create-shopping-item", new
        {
            Name = itemName,
            Description = "Created by integration test",
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<CreateShoppingItemResponse>();

        Assert.NotNull(response);
        Assert.Equal(itemName, response.Name);
        Assert.Equal("Created by integration test", response.Description);
        Assert.True(response.ShoppingItemId > 0);

        var persistedItem = await _factory.ApplicationDbContext.ShoppingItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ShoppingItemId == response.ShoppingItemId);

        Assert.NotNull(persistedItem);
        Assert.Equal(itemName, persistedItem.Name);
        Assert.Equal("Created by integration test", persistedItem.Description);
        Assert.Equal(response.UserCode, persistedItem.UserCode);
    }

    [Fact]
    public async Task UpdateShoppingItem_WithValidRequest_UpdatesPersistedItem()
    {
        var loginResponse = await GetLoginResponseAsync();
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        var result = await _apiClient.PostAsJsonAsync("/update-shopping-item", new
        {
            ShoppingItemId = ShoppingItemId,
            Name = "Updated item",
            Description = "Updated by integration test",
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<UpdateShoppingItemResponse>();

        Assert.NotNull(response);
        Assert.Equal(ShoppingItemId, response.ShoppingItemId);
        Assert.Equal("Updated item", response.Name);
        Assert.Equal("Updated by integration test", response.Description);

        var persistedItem = await _factory.ApplicationDbContext.ShoppingItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ShoppingItemId == ShoppingItemId);

        Assert.NotNull(persistedItem);
        Assert.Equal("Updated item", persistedItem.Name);
        Assert.Equal("Updated by integration test", persistedItem.Description);
    }

    [Fact]
    public async Task UploadShoppingItemImage_WithValidRequest_FileObjectStore()
    {
        var loginResponse = await GetLoginResponseAsync();
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createResponse = await CreateShoppingItemAsync("Item with image", "Upload test item");
        var fileName = $"image-{Guid.NewGuid():N}.png";

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(OnePixelPng);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", fileName);

        var result = await _apiClient.PostAsync(
            new Uri($"/shopping-items/{createResponse.ShoppingItemId.ToString()}/upload-image", UriKind.Relative),
            form);
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<UploadShoppingItemImageResponse>();

        Assert.NotNull(response);
        Assert.Equal(createResponse.ShoppingItemId, response.ShoppingItemId);
        Assert.Equal(fileName, response.Name);
        Assert.Equal("image/png", response.MimeType);
        Assert.Equal(OnePixelPng.Length, response.Size);

        var objectName = $"{createResponse.UserCode}/{createResponse.ShoppingItemId.ToString()}/{fileName}";
        Assert.Equal(
            $"{_factory.FileObjectStoreUrl}/{GamesGlobalWebApiFactory.FileObjectStoreBucketName}/{objectName}",
            response.Url,
            StringComparer.Ordinal);

        var objectStat = await _factory.MinioClient.StatObjectAsync(new StatObjectArgs()
            .WithBucket(GamesGlobalWebApiFactory.FileObjectStoreBucketName)
            .WithObject(objectName));

        Assert.NotNull(objectStat);
        Assert.Equal(objectName, objectStat.ObjectName);
        Assert.Equal(OnePixelPng.Length, objectStat.Size);
        Assert.Equal("image/png", objectStat.ContentType);

        await _factory.MinioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(GamesGlobalWebApiFactory.FileObjectStoreBucketName)
            .WithObject(objectName));
    }

    [Fact]
    public async Task DeleteShoppingItem_WithValidRequest_DeletesPersistedItem()
    {
        var loginResponse = await GetLoginResponseAsync();
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createResponse = await CreateShoppingItemAsync("Item to delete", "Delete test item");

        var result = await _apiClient.DeleteAsync($"/shopping-item/{createResponse.ShoppingItemId.ToString()}");
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<DeleteShoppingItemResponse>();

        Assert.NotNull(response);
        Assert.Equal(createResponse.ShoppingItemId, response.ShoppingItemId);
        Assert.Equal(createResponse.Name, response.Name);
        Assert.Equal(createResponse.Description, response.Description);

        var persistedItem = await _factory.ApplicationDbContext.ShoppingItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ShoppingItemId == createResponse.ShoppingItemId);

        Assert.Null(persistedItem);
    }

    private Task<LoginResponse> GetLoginResponseAsync()
    {
        _loginResponseTask ??= LoginAsync();
        return _loginResponseTask;
    }

    private async Task<LoginResponse> LoginAsync()
    {
        var result = await _apiClient.PostAsJsonAsync("/identity/login", LoginRequest);
        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("The login endpoint returned an empty response.");
    }

    private async Task<CreateShoppingItemResponse> CreateShoppingItemAsync(string name, string description)
    {
        var result = await _apiClient.PostAsJsonAsync("/create-shopping-item", new
        {
            Name = name,
            Description = description,
        });
        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<CreateShoppingItemResponse>()
            ?? throw new InvalidOperationException("The create shopping item endpoint returned an empty response.");
    }
}
