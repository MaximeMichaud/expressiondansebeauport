using System.Text.Json;
using System.Text.RegularExpressions;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;

namespace Web.Features.Admins.ErrorLogs;

public class GetErrorLogsEndpoint : EndpointWithoutRequest<List<ErrorLogDto>>
{
    private readonly IWebHostEnvironment _env;

    public GetErrorLogsEndpoint(IWebHostEnvironment env)
    {
        _env = env;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/error-logs");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var logsDir = Path.Combine(_env.ContentRootPath, "logs");
        var errorLogs = new List<ErrorLogDto>();

        if (!Directory.Exists(logsDir))
        {
            await Send.OkAsync(errorLogs, cancellation: ct);
            return;
        }

        var logFiles = Directory.GetFiles(logsDir, "logs-*.log")
            .OrderByDescending(f => f)
            .Take(7);

        foreach (var file in logFiles)
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var entry = JsonSerializer.Deserialize<JsonElement>(line);
                    var level = entry.TryGetProperty("@l", out var lvl) ? lvl.GetString() : null;

                    if (level is not ("Warning" or "Error" or "Fatal"))
                        continue;

                    errorLogs.Add(new ErrorLogDto
                    {
                        Timestamp = entry.TryGetProperty("@t", out var ts) ? ts.GetString()! : "",
                        Level = level,
                        Message = RenderMessage(entry),
                        Exception = entry.TryGetProperty("@x", out var ex) ? ex.GetString() : null,
                        RequestId = entry.TryGetProperty("RequestId", out var rid) ? rid.GetString() : null,
                        SourceContext = entry.TryGetProperty("SourceContext", out var src) ? src.GetString() : null
                    });
                }
                catch (JsonException)
                {
                    // Ligne non-JSON (ancien format), on skip
                }
            }
        }

        var result = errorLogs
            .OrderByDescending(e => e.Timestamp)
            .Take(200)
            .ToList();

        await Send.OkAsync(result, cancellation: ct);
    }

    private static string RenderMessage(JsonElement entry)
    {
        if (entry.TryGetProperty("@m", out var msg) && msg.ValueKind == JsonValueKind.String)
            return msg.GetString()!;

        if (!entry.TryGetProperty("@mt", out var mt) || mt.ValueKind != JsonValueKind.String)
            return "";

        var template = mt.GetString()!;

        // Substitue {propertyName} et {propertyName:format} avec les valeurs du JSON
        return Regex.Replace(template, @"\{(\w+)(?::[^}]*)?\}", match =>
        {
            var name = match.Groups[1].Value;
            if (entry.TryGetProperty(name, out var prop))
            {
                return prop.ValueKind == JsonValueKind.String
                    ? prop.GetString() ?? match.Value
                    : prop.ToString();
            }
            return match.Value;
        });
    }
}
