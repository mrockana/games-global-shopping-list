using System.Collections;
using System.Collections.Generic;

namespace GamesGlobal.ShoppingList.xUnitTests.TestData;

internal sealed class InvalidPasswordTestData : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        yield return new object?[] { null };
        yield return new object?[] { string.Empty };
        yield return new object?[] { "123" };      // Too short
        yield return new object?[] { "abcdefg" };  // 7 chars, still too short
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
