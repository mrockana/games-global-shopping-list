using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.Login;

public sealed record LoginResponse(string Token, int ExpiresInMinutes, string RefreshToken, int RefreshTokenExpiresInMinutes, Permissions Permissions);
