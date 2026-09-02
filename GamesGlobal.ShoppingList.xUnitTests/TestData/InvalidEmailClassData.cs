namespace GamesGlobal.ShoppingList.xUnitTests.TestData;

using System.Collections;
using System.Collections.Generic;
internal sealed class InvalidEmailClassData : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        // Empty, null, and various invalid email formats
        yield return new object?[] { null };
        yield return new object?[] { string.Empty };
        yield return new object?[] { "plainaddress" };
        yield return new object?[] { "#@%^%@#@#.com" };
        yield return new object?[] { "@example.com" };
        yield return new object?[] { "joe smith <email@example.com>" };
        yield return new object?[] { "user@.com" };
        yield return new object?[] { "user@domain..com" };
        yield return new object?[] { "email@example@example.com" };
        yield return new object?[] { "user@domain@domain.com" };
        yield return new object?[] { "user@-domain.com" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
