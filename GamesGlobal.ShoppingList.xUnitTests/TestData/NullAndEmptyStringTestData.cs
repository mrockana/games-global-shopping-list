using System.Collections;

namespace GamesGlobal.ShoppingList.xUnitTests.TestData;

internal sealed class NullAndEmptyStringTestData : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        yield return new object?[] { null };
        yield return new object?[] { string.Empty };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
