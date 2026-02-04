namespace Application.Services.Users.Exceptions;

public class InvalidJwtSettingsException(string message) : Exception(message);