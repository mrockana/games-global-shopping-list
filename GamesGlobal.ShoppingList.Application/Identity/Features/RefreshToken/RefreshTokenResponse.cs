using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;

public sealed record RefreshTokenResponse(string Token, int ExpiresInMinutes, string RefreshToken, int RefreshTokenExpiresInMinutes, Permissions Permissions);
