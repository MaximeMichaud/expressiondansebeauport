namespace Application.Services.Users.Exceptions;

public class TwoFactorAuthenticationException(string message): Exception(message);