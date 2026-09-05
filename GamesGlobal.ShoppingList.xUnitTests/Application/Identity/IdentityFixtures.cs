using AutoFixture;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.RefreshToken;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Identity;

internal static class IdentityFixtures
{
    public const string Username = "username@example.com";
    public const string Email = "username@example.com";
    public const string InvalidEmail = "plainaddress";
    public const string Password = "Password123";
    public const string OTP = "654321";
    public const string AlternativePassword = "DifferentPassword";
    public const string JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJQZXJtaXNzaW9ucyI6Ii0xIiwic2lkIjoiMDc0M2VhZGItMGExYy00ZjYyLWE2YjYtZjdhYTAyNjA3ZjJjIiwic3ViIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJlbWFpbCI6ImFkbWluQGV4YW1wbGUuY29tIiwianRpIjoiM2U5ZjUwYjctM2NiZi00YWRhLWE0NDAtOGY0NDhkZjk5NTU1IiwiZXhwIjoxNzUzNjI2MDQzLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjIzIiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzIyMyJ9.jMSTjNHukj9co_iW-kLOTvspZadJQq1dOxoEu9-f0GI";
    public const string HashedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJQZXJtaXNzaW9ucyI6Ii0xIiwic2lkIjo";

    public static IOptions<IdentityModuleOptions> IdentittyModuleOptions => Options.Create(new IdentityModuleOptions
    {
        JwtExpiresInMinutes = 60,
        RefreshTokenExpiresInMinutes = 120,
        ApplicationUrl = "http://localhost",
        HashedTokenSigningKey = Guid.NewGuid().ToString(),
        JwtSigningKey = Guid.NewGuid().ToString(),
    });

    public static Fixture Fixture => new Fixture();

    public static IIdentityRepository Repository => Substitute.For<IIdentityRepository>();
    public static IJwtTokenGenerator JwtTokenGenerator => Substitute.For<IJwtTokenGenerator>();
    public static IUserHashGenerator HashedTokenGenerator => Substitute.For<IUserHashGenerator>();

    public static IRefreshTokenService LoginSessionCreator => Substitute.For<IRefreshTokenService>();

    public static User BasicUser => new User
    {
        UserId = Fixture.Create<long>(),
        Email = Email,
        FirstName = Fixture.Create<string>(),
        LastName = Fixture.Create<string>(),
    };
}
