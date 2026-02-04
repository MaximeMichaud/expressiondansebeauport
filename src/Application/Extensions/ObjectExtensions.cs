namespace Application.Extensions;

public static class ObjectExtensions
{
    public static List<T> IntoList<T>(this T anyObject)
    {
        return new List<T> { anyObject };
    }
}