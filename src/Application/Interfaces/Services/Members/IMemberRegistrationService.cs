using Application.Services.Members.Dtos;
using Domain.Entities;

namespace Application.Interfaces.Services.Members;

public interface IMemberRegistrationService
{
    Task<Member> RegisterMember(MemberRegistrationDto memberRegistrationDto);
}