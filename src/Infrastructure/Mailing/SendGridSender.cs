using System.Text.Json;
using Application.Interfaces.Mailing;
using Application.Services.Notifications.Models;
using Domain.Common;
using Microsoft.Extensions.Logging;
using SendGrid;

namespace Infrastructure.Mailing;

public class SendGridSender : IEmailSender
{
    private readonly ILogger<SendGridSender> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly ISendGridMessageFactory _sendGridMessageFactory;

    public SendGridSender(
        ILogger<SendGridSender> logger,
        ISendGridClient sendGridClient,
        ISendGridMessageFactory sendGridMessageFactory)
    {
        _logger = logger;
        _sendGridClient = sendGridClient;
        _sendGridMessageFactory = sendGridMessageFactory;
    }

    public async Task<SucceededOrNotResponse> SendAsync<TModel>(TModel model) where TModel : NotificationModel
    {
        var msg = _sendGridMessageFactory.CreateFromModel(model);
        var response = await _sendGridClient.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
            return new SucceededOrNotResponse(response.IsSuccessStatusCode);

        var errors = await GetErrorListFromResponse(response);
        _logger.LogError("Error occured while sending email. Errors : {errors}", JsonSerializer.Serialize(errors));

        return new SucceededOrNotResponse(response.IsSuccessStatusCode, errors);
    }

    private async Task<List<Error>> GetErrorListFromResponse(Response response)
    {
        var deserializedResponse = await response.DeserializeResponseBodyAsync();
        string stringErrorList = Convert.ToString(deserializedResponse.First().Value);
        var errors = JsonSerializer.Deserialize<List<SendGridError>>(stringErrorList);
        return errors == null ? [] : errors.Select(x => new Error("SendGridError", x.Message)).ToList();
    }
}