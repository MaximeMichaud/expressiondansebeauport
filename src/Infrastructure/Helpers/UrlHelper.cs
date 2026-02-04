namespace Infrastructure.Helpers;

public static class UrlHelper
{
    public static string BuildUriWithQueryParameters(string baseUrl, List<KeyValuePair<string, string>> queryParameters)
    {
        if (!queryParameters.Any())
            return baseUrl;
        
        var baseQueryUrl = baseUrl + "?";
        foreach (var queryParameter in queryParameters)
        {
            var isLast = queryParameters.Last().Equals(queryParameter);
            baseQueryUrl += queryParameter.Key + "=" + queryParameter.Value + (isLast ? "" : "&");
        }
        return baseQueryUrl;
    }
}