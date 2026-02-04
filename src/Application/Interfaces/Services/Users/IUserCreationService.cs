using Application.Services.Users.Dtos;
using Domain.Entities.Identity;

namespace Application.Interfaces.Services.Users;

public interface IUserCreationService
{
    Task<User> CreateUserOrUpdateDeletedUserWithSameEmail(UserCreationDto userCreationDto);
}