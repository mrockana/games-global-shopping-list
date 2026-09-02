using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.StringHelper;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

    public static bool IsValidEmail(this string email)
    {
        if (email.IsNullOrWhiteSpace())
        {
            return false;
        }

        try
        {
            // Normalize the domain
            email = Regex.Replace(email,
                                  @"(@)(.+)$",
                                  DomainMapper,
                                  RegexOptions.None,
                                  TimeSpan.FromMilliseconds(200));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                                @"^[^@\s]+@[^@\s]+\.?[^@\s]+$",
                                RegexOptions.IgnoreCase,
                                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static string DomainMapper(Match match)
    {
        // Use IdnMapping class to convert Unicode domain names
        var idn = new IdnMapping();

        // Pull out and process domain name (throws ArgumentException on invalid)
        string domainName = idn.GetAscii(match.Groups[2].Value);

        return match.Groups[1].Value + domainName;
    }
}
