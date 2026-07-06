using MediCare.Data.Models;

namespace MediCare.Data.Repositories.Interfaces;

public interface IAccountRepositories
{
    public Task<IEnumerable<string>> SetNewAccount
        (ApplicationUser user, string Role, int? SpecializationId, string password);
}
