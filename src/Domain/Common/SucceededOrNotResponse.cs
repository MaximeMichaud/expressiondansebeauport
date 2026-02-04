namespace Domain.Common;

public class SucceededOrNotResponse
{
    public bool Succeeded { get; set; }
    public List<Error> Errors { get; set; } = new();

    public SucceededOrNotResponse() {}

    public SucceededOrNotResponse(bool succeeded, Error? error = null)
    {
        Succeeded = succeeded;
        if (error != null)
            Errors.Add(error);
    }

    public SucceededOrNotResponse(bool succeeded, IEnumerable<Error> errors)
    {
        Succeeded = succeeded;
        Errors.AddRange(errors);
    }
}