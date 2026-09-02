using GamesGlobal.ShoppingList.BusinessDomain.Common.StringHelper;
using GamesGlobal.ShoppingList.xUnitTests.TestData;

namespace GamesGlobal.ShoppingList.xUnitTests.BusinessDomain.Common;

public sealed class StringExtensionsTest
{
    [Theory]
    [InlineData("email@example.com")]
    [InlineData("firstname.lastname@example.com")]
    [InlineData("email@subdomain.example.com")]
    [InlineData("firstname+lastname@example.com")]
    [InlineData("email@123.123.123.123.123")]
    [InlineData("email@[123.123.123.123.123]")]
    [InlineData("1234567890@example.com")]
    [InlineData("email@example-one.com")]
    [InlineData("email@example.name")]
    [InlineData("email@example.museum")]
    [InlineData("email@example.co.jp")]
    [InlineData("firstname-lastname@example.com")]
    [InlineData("email@example")]
    [InlineData("“email“@example.com")]
    [InlineData("much.“more\\unusual“@example.com")]
    [InlineData("very.“(),:;<>[]“.VERY.“very\\\\\"very“@strange.example.com")]
    public void IsEmailValid_WhenEmailsAreValid_ShouldReturnTrue(string email)
    {
        // act
        var result = StringExtensions.IsValidEmail(email);

        // assert
        Assert.True(result);
    }

    [Theory]
    [ClassData(typeof(InvalidEmailClassData))]
    public void IsEmailValid_WhenEmailsAreInvalid_ShouldReturnFalse(string email)
    {
        // act
        var result = StringExtensions.IsValidEmail(email);

        // assert
        Assert.False(result);
    }
}
