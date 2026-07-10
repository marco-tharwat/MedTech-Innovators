using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MediCare.Data.Repositories.Implementations;

public class AccountRepositories(UserManager<ApplicationUser> _userManager,IUnitOfWork _unitOfWork) : IAccountRepositories
{

    public async Task<IEnumerable<string>> SetNewAccount(ApplicationUser user, string Role, int? SpecializationId, string password, DateTime? BirthDate)
    {
        List<string> response = new();

        var flag = await _userManager.CreateAsync(user, password);
        if (flag.Succeeded)
        {
            var res = await _userManager.AddToRoleAsync(user, Role);

            if (res.Succeeded)
            {
                if (Role == "Doctor")
                {
                    Doctor doctor = new();
                    doctor.User = user;
                    doctor.UserId = user.Id;
                    doctor.IsApproved = false;
                    doctor.SpecializationId = SpecializationId ?? 0;
                    await _unitOfWork.Doctors.AddAsync(doctor);
                }
                else
                {
                    Patient patient = new Patient();
                    patient.User = user;
                    patient.UserId = user.Id;
                    patient.BirthDate = BirthDate ?? default;
                    await _unitOfWork.Patients.AddAsync(patient);
                }
                await _unitOfWork.SaveChangesAsync();
                return response;
            }
            foreach (var error in res.Errors)
            {
                response.Add(error.Description);
            }
        }
        foreach (var error in flag.Errors)
        {
            response.Add(error.Description);
        }
        return response;
    }
}
