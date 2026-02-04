namespace Tests.Common.Extensions;

public static class ObjectExtensions
{
    public static string? GetStringValueOfProperty(this object value, string propertyName)
    {
        return value.GetType().GetProperties().First(x => x.Name == propertyName).GetValue(value)?.ToString();
    }
    
    public static List<T> GetValueOfPropertyAsListOf<T>(this object value, string propertyName)
    {
        return (List<T>)value.GetType().GetProperties().First(x => x.Name == propertyName).GetValue(value)!;
    }
}