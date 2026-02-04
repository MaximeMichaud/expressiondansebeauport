using Application.Exceptions.Members;
using Application.Exceptions.Users;
using Application.Interfaces.Services.Members;
using Application.Interfaces.Services.Users;
using Application.Services.Members.Exceptions;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services.Members;

public class AuthenticatedMemberService : IAuthenticatedMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public AuthenticatedMemberService(
        IMemberRepository memberRepository,
        IAuthenticatedUserService authenticatedUserService)
    {
        _memberRepository = memberRepository;
        _authenticatedUserService = authenticatedUserService;
    }

    public Member GetAuthenticatedMember()
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null)
            throw new UserNotFoundException("Could not find user associated with authenticated member.");

        if (!user.UserRoles.Any())
            throw new GetAuthenticatedMemberException($"Authenticated user {user.Id} has no role.");

        var member = _memberRepository.FindByUserId(user.Id);
        if (member == null)
            throw new MemberNotFoundException($"Could not find member associated with user with id {user.Id}");

        return member;
    }
}