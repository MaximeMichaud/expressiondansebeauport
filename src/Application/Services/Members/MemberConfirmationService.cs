using Application.Exceptions.Members;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Application.Services.Members;

public class MemberConfirmationService : IEmailConfirmationService
{
    private readonly IEmailConfirmationCodeRepository _codeRepository;
    private readonly UserManager<User> _userManager;

    public MemberConfirmationService(
        IEmailConfirmationCodeRepository codeRepository,
        UserManager<User> userManager)
    {
        _codeRepository = codeRepository;
        _userManager = userManager;
    }

    public async Task<bool> ConfirmEmail(Guid userId, string code)
    {
        var confirmationCode = await _codeRepository.GetLatestForUser(userId);
        if (confirmationCode == null)
            return false;

        if (confirmationCode.IsLockedOut())
            throw new AccountLockedException("Too many attempts. Please wait 30 minutes.");

        confirmationCode.IncrementAttemptCount();

        var now = InstantHelper.GetLocalNow();
        if (confirmationCode.IsExpired(now) || confirmationCode.IsUsed)
        {
            await _codeRepository.Update(confirmationCode);
            return false;
        }

        if (confirmationCode.Code != code)
        {
            await _codeRepository.Update(confirmationCode);
            return false;
        }

        confirmationCode.MarkAsUsed();
        await _codeRepository.Update(confirmationCode);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        return true;
    }

    public async Task<string> ResendCode(Guid userId)
    {
        var existing = await _codeRepository.GetLatestForUser(userId);

        if (existing != null && !existing.CanResend())
            throw new RateLimitExceededException("Maximum 3 resends per hour.");

        if (existing != null)
        {
            var now = InstantHelper.GetLocalNow();
            existing.IncrementResendCount(now);
            await _codeRepository.Update(existing);
        }

        var code = Random.Shared.Next(100000, 999999).ToString();
        var newCode = new EmailConfirmationCode();
        newCode.SetUser((await _userManager.FindByIdAsync(userId.ToString()))!);
        newCode.SetCode(code);
        newCode.SetExpiresAt(InstantHelper.GetLocalNow().Plus(Duration.FromMinutes(15)));
        await _codeRepository.Add(newCode);

        return code;
    }
}
