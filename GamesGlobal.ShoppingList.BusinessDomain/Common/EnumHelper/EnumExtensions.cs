using System;
using System.ComponentModel;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attr = field!.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attr.Length == 0 ? value.ToString() : ((DescriptionAttribute)attr[0]).Description;
    }
}
