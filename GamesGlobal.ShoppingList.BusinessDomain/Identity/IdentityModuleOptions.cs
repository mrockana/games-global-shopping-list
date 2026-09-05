namespace GamesGlobal.ShoppingList.BusinessDomain.Identity;

public sealed class IdentityModuleOptions
{
    public required string ApplicationUrl { get; set; }

    public required string JwtSigningKey { get; set; }

    public required int JwtExpiresInMinutes { get; set; }

    public required string HashedTokenSigningKey { get; set; }

    public required int RefreshTokenExpiresInMinutes { get; set; }
}
