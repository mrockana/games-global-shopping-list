using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.RefreshToken;

public interface IRefreshTokenService
{
    Task<(Entities.RefreshToken? newSession, bool isSuccess)> CreateRefreshToken(IIdentityRepository identityRepository, User user);
}
