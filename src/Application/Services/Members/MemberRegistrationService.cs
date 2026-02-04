using Application.Interfaces.Services.Members;
using Application.Interfaces.Services.Users;
using Application.Services.Members.Dtos;
using Application.Services.Users.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services.Members;

public class MemberRegistrationService : IMemberRegistrationService
{
    private readonly IMapper _mapper;
    private readonly IMemberRepository _memberRepository;
    private readonly IUserCreationService _userCreationService;

    public MemberRegistrationService(
        IMapper mapper,
        IMemberRepository memberRepository,
        IUserCreationService userCreationService)
    {
        _mapper = mapper;
        _memberRepository = memberRepository;
        _userCreationService = userCreationService;
    }

    public async Task<Member> RegisterMember(MemberRegistrationDto memberRegistrationDto)
    {
        var userCreationDto = _mapper.Map<UserCreationDto>(memberRegistrationDto);
        var user = await _userCreationService.CreateUserOrUpdateDeletedUserWithSameEmail(userCreationDto);

        var member = _mapper.Map<Member>(memberRegistrationDto);
        member.SetUser(user);

        await _memberRepository.Create(member);

        return member;
    }
}