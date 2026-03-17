using Application.Exceptions.Members;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Application.Services.Members;

public class MemberRegistrationService : IMemberRegistrationService
{
    private readonly UserManager<User> _userManager;
    private readonly IMemberRepository _memberRepository;
    private readonly IEmailConfirmationCodeRepository _confirmationCodeRepository;

    public MemberRegistrationService(
        UserManager<User> userManager,
        IMemberRepository memberRepository,
        IEmailConfirmationCodeRepository confirmationCodeRepository)
    {
        _userManager = userManager;
        _memberRepository = memberRepository;
        _confirmationCodeRepository = confirmationCodeRepository;
    }

    public async Task<(Member member, string confirmationCode)> RegisterMember(
        string firstName, string lastName, string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
            throw new Application.Exceptions.Users.UserWithEmailAlreadyExistsException("A user with this email already exists.");

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new UserCreationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, Domain.Constants.User.Roles.MEMBER);

        var member = new Member(firstName, lastName);
        member.SetUser(user);
        member.SanitizeForSaving();
        await _memberRepository.Add(member);

        var code = GenerateCode();
        var confirmationCode = new EmailConfirmationCode();
        confirmationCode.SetUser(user);
        confirmationCode.SetCode(code);
        confirmationCode.SetExpiresAt(InstantHelper.GetLocalNow().Plus(Duration.FromMinutes(15)));
        await _confirmationCodeRepository.Add(confirmationCode);

        return (member, code);
    }

    private static string GenerateCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
