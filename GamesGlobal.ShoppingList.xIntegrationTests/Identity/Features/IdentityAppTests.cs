using System.Net.Http.Headers;
using System.Net.Http.Json;
using GamesGlobal.ShoppingList.Application.Common.Pagination;
using GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;
using GamesGlobal.ShoppingList.Application.Identity.Features.GetRoles;
using GamesGlobal.ShoppingList.Application.Identity.Features.GetUsers;
using GamesGlobal.ShoppingList.Application.Identity.Features.Login;
using GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;
using GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;
using GamesGlobal.ShoppingList.Application.Identity.Features.UpdateRolePermissions;
using GamesGlobal.ShoppingList.Application.Identity.Features.UpdateUserRoles;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.xIntegrationTests.Identity.Features;

[Collection(nameof(IdentityAppTests))]
public sealed class IdentityAppTests : IClassFixture<GamesGlobalWebApiFactory>
{
    private const string Password = "123Abc123@";
    private const string AdminEmail = "admin@example.gamesglobal";
    private readonly HttpClient _apiClient;

    public IdentityAppTests(GamesGlobalWebApiFactory factory)
    {
        _apiClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithSeededUser_ReturnsTokensAndPermissions()
    {
        var response = await LoginAsync("johndoe@example.gamesglobal");

        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
        Assert.True(response.ExpiresInMinutes > 0);
        Assert.True(response.RefreshTokenExpiresInMinutes > 0);
        Assert.Equal(Permissions.ShoppingItemsSelfReadWrite, response.Permissions);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsReplacementTokens()
    {
        var loginResponse = await LoginAsync("johndoe@example.gamesglobal");
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        var result = await _apiClient.PostAsJsonAsync("/identity/refresh-token", new
        {
            loginResponse.RefreshToken,
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<RefreshTokenResponse>();

        Assert.NotNull(response);
        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
        Assert.NotEqual(loginResponse.RefreshToken, response.RefreshToken);
        Assert.Equal(Permissions.ShoppingItemsSelfReadWrite, response.Permissions);
    }

    [Fact]
    public async Task Signup_WithValidRequest_CreatesUser()
    {
        var email = $"signup-{Guid.NewGuid():N}@example.gamesglobal";

        var result = await _apiClient.PostAsJsonAsync("/identity/signup", new
        {
            FirstName = "Integration",
            LastName = "Tester",
            Email = email,
            Password,
            ConfirmPassword = Password,
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<SignupResponse>();

        Assert.NotNull(response);
        Assert.True(response.UserId > 0);
        Assert.Equal("Integration", response.FirstName);
        Assert.Equal("Tester", response.LastName);
        Assert.Equal(email, response.Email);
    }

    [Fact]
    public async Task AddRole_WithAdminAuthorization_CreatesRoleWithPermissions()
    {
        await AuthorizeAsAdminAsync();
        var roleName = $"Integration Role {Guid.NewGuid():N}";

        var result = await _apiClient.PostAsJsonAsync("/identity/add-role", new
        {
            RoleName = roleName,
            Permissions = new[] { new { Value = (long)Permissions.ShoppingItemsSelfReadOnly } },
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<AddRoleResponse>();

        Assert.NotNull(response);
        Assert.True(response.RoleId > 0);
        Assert.Equal(roleName, response.Name);
        var permission = Assert.Single(response.Permissions);
        Assert.Equal(Permissions.ShoppingItemsSelfReadOnly, permission.Permission);
    }

    [Fact]
    public async Task GetRoles_WithAdminAuthorization_ReturnsSeededRoles()
    {
        await AuthorizeAsAdminAsync();

        var result = await _apiClient.GetAsync("/identity/roles");
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<IList<GetRolesQueryResponse>>();

        Assert.NotNull(response);
        Assert.Contains(response, role => string.Equals(role.Name, "Super Admin", StringComparison.Ordinal)
            && role.Permissions.Any(permission => permission.Permission == Permissions.All));
    }

    [Fact]
    public async Task GetUsers_WithAdminAuthorization_ReturnsSeededAdmin()
    {
        await AuthorizeAsAdminAsync();

        var result = await _apiClient.GetAsync("/identity/users?take=10&skip=0");
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<PaginatedResults<IList<GetUsersQueryResponse>>>();

        Assert.NotNull(response);
        Assert.True(response.TotalRecords >= 3);
        Assert.Contains(response.Data, user => string.Equals(user.Email, AdminEmail, StringComparison.Ordinal)
            && user.Roles.Any(role => string.Equals(role.Name, "Super Admin", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task UpdateRolePermissions_WithAdminAuthorization_ReplacesRolePermissions()
    {
        await AuthorizeAsAdminAsync();
        var role = await CreateRoleAsync(Permissions.ShoppingItemsSelfReadOnly);

        var result = await _apiClient.PostAsJsonAsync("/identity/update-role-permissions", new
        {
            role.RoleId,
            RoleName = role.Name,
            Permissions = new[] { new { Value = (long)Permissions.ShoppingItemsSelfReadWrite } },
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<UpdateRolePermissionsResponse>();

        Assert.NotNull(response);
        Assert.Equal(role.RoleId, response.RoleId);
        var permission = Assert.Single(response.Permissions);
        Assert.Equal(Permissions.ShoppingItemsSelfReadWrite, permission.Permission);
    }

    [Fact]
    public async Task UpdateUserRoles_WithAdminAuthorization_AssignsRoleToUser()
    {
        var signupResponse = await SignupAsync();
        await AuthorizeAsAdminAsync();

        var result = await _apiClient.PostAsJsonAsync("/identity/update-user-roles", new
        {
            signupResponse.UserId,
            RoleIds = new[] { 2L },
        });
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<UpdateUserRolesResponse>();

        Assert.NotNull(response);
        Assert.Equal(signupResponse.UserId, response.UserId);
        var role = Assert.Single(response.Roles);
        Assert.Equal(2L, role.RoleId);
        Assert.Equal("End User", role.Name);
    }

    private async Task AuthorizeAsAdminAsync()
    {
        var loginResponse = await LoginAsync(AdminEmail);
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
    }

    private async Task<LoginResponse> LoginAsync(string email)
    {
        var result = await _apiClient.PostAsJsonAsync("/identity/login", new SessionLoginCommand(email, Password));
        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("The login endpoint returned an empty response.");
    }

    private async Task<SignupResponse> SignupAsync()
    {
        var email = $"roles-{Guid.NewGuid():N}@example.gamesglobal";
        var result = await _apiClient.PostAsJsonAsync("/identity/signup", new
        {
            FirstName = "Role",
            LastName = "Tester",
            Email = email,
            Password,
            ConfirmPassword = Password,
        });
        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<SignupResponse>()
            ?? throw new InvalidOperationException("The signup endpoint returned an empty response.");
    }

    private async Task<AddRoleResponse> CreateRoleAsync(Permissions permission)
    {
        var roleName = $"Role {Guid.NewGuid():N}";
        var result = await _apiClient.PostAsJsonAsync("/identity/add-role", new
        {
            RoleName = roleName,
            Permissions = new[] { new { Value = (long)permission } },
        });
        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<AddRoleResponse>()
            ?? throw new InvalidOperationException("The add role endpoint returned an empty response.");
    }
}
