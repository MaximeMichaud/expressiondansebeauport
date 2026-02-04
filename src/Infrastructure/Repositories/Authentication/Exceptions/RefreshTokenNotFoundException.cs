namespace Infrastructure.Repositories.Authentication.Exceptions;

public class RefreshTokenNotFoundException(string message) : Exception(message);