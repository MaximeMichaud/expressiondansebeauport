using System.ComponentModel;

namespace Application.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo == null)
            return string.Empty;

        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
        return attribute != null ? attribute.Description : value.ToString();
    }
}