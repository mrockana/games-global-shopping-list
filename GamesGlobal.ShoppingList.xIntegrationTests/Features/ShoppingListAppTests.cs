using System.Net.Http.Headers;
using System.Net.Http.Json;
using GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;
using GamesGlobal.ShoppingList.Application.Identity.Features.Login;
using GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.xIntegrationTests.Features;

[CollectionDefinition(nameof(ShoppingListAppTests), DisableParallelization = true)]
public sealed class ShoppingListAppTestsCollection
{
}

[Collection(nameof(ShoppingListAppTests))]
public sealed class ShoppingListAppTests : IClassFixture<GamesGlobalWebApiFactory>
{
    private static readonly SessionLoginCommand LoginRequest = new(
        Username: "johndoe@example.gamesglobal",
        Password: "123Abc123@");

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
        var itemName = $"item-{Guid.NewGuid():N}";
        var createResult = await _apiClient.PostAsJsonAsync("/create-shopping-item", new
        {
            Name = itemName,
            Description = "Integration-test item",
        });
        createResult.EnsureSuccessStatusCode();

        var result = await _apiClient.GetAsync("/shopping-items");
        result.EnsureSuccessStatusCode();

        var shoppingItems = await result.Content.ReadFromJsonAsync<IList<GetShoppingItemResponse>>();

        Assert.NotNull(shoppingItems);
        Assert.Contains(shoppingItems, item =>
            string.Equals(item.Name, itemName, StringComparison.Ordinal)
            && string.Equals(item.Description, "Integration-test item", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SignupUser_WithValidRequest_OkWithCorrect()
    {
        var email = $"signup-{Guid.NewGuid():N}@email.com";
        var signupRequest = new SignupCommand(
            ConfirmPassword: "123Abc123@",
            Password: "123Abc123@",
            Email: email,
            FirstName: "Mr",
            LastName: "Test");

        var result = await _apiClient.PostAsJsonAsync("/identity/signup", signupRequest);
        result.EnsureSuccessStatusCode();

        var signupResponse = await result.Content.ReadFromJsonAsync<SignupResponse>();

        Assert.Equal(signupRequest.Email, signupResponse!.Email);
        Assert.Equal(signupRequest.FirstName, signupResponse.FirstName);
        Assert.Equal(signupRequest.LastName, signupResponse.LastName);
        Assert.NotEqual(default, signupResponse.UserId);

        var persistedUser = await _factory.IdentityDbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == signupRequest.Email);

        Assert.NotNull(persistedUser);
        Assert.Equal(signupResponse.UserId, persistedUser.UserId);
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
}
