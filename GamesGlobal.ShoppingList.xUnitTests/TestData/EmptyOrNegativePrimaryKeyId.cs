using System.Collections;

namespace GamesGlobal.ShoppingList.xUnitTests.TestData;

internal sealed class EmptyOrNegativePrimaryKeyId : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 0L };
        yield return new object[] { -1L };
        yield return new object[] { -100L };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}