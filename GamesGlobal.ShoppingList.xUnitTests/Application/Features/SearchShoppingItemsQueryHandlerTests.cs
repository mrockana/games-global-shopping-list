using GamesGlobal.ShoppingList.Application.Features.SearchShoppingItems;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features.SearchShoppingItems;

public sealed class SearchShoppingItemsQueryHandlerTests
{
    [Fact]
    public void Validation_SearchIsEmpty_ReturnsInvalid()
    {
        var result = new SearchShoppingItemsValidation().Validate(new SearchShoppingItemsQuery(Guid.NewGuid(), string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.Equals(nameof(SearchShoppingItemsQuery.Search), StringComparison.Ordinal));
    }
}