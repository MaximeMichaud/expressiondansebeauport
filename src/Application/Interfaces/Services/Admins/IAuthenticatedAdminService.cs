using Domain.Entities;

namespace Application.Interfaces.Services.Admins;

public interface IAuthenticatedAdminService
{
    Administrator GetAuthenticatedAdmin();
}