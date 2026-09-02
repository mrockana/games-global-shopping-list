using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;

public interface IUserHashGenerator
{
    string GenerateHashedToken(User user);
}
