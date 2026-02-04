using Application.Exceptions.Users;
using Application.Interfaces.Services.Users;
using Application.Services.Users.Dtos;
using AutoMapper;
using Domain.Entities.Identity;
using Domain.Repositories;

namespace Application.Services.Users;

public class UserCreationService : IUserCreationService
{
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;

    public UserCreationService(IMapper mapper, IRoleRepository roleRepository, IUserRepository userRepository)
    {
        _mapper = mapper;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
    }

    public async Task<User> CreateUserOrUpdateDeletedUserWithSameEmail(UserCreationDto userCreationDto)
    {
        var user = _userRepository.FindByEmail(userCreationDto.Email, true);
        if (user != null && user.IsActive())
            throw new UserWithEmailAlreadyExistsException("A user with this email already exists.");

        var role = await _roleRepository.FindByName(userCreationDto.RoleName);
        if (user != null && !user.IsActive())
        {
            user.Restore();
            user = _mapper.Map(userCreationDto, user);
            user.AddRole(role);
            user = await _userRepository.UpdateUser(user);
        }
        else
        {
            user = _mapper.Map<User>(userCreationDto);
            user.AddRole(role);
            user = await _userRepository.CreateUser(user);
        }
        return user;
    }
}