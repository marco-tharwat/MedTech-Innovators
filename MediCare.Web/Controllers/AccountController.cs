using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCare.Web.Controllers
{
    public class AccountController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IUnitOfWork _unitOfWork;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            , IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult> Register()
        {
            var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specs, "Id", "Name");
            return View("Register");
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            if (request == null)
            {
                ModelState.AddModelError("", "the data is not found");
                return RedirectToAction("Register");
            }

            ApplicationUser user = new ApplicationUser
            { FullName = request.Name, Gender = request.Gender, Email = request.Email, UserName = request.UserName };

            var res = await _userManager.CreateAsync(user, request.Password);

            await _userManager.AddToRoleAsync(user, request.Role);

            if (res.Succeeded)
            {
                if (request.Role == "Doctor")
                {
                    Doctor doctor = new();
                    doctor.User = user;
                    doctor.UserId = user.Id;
                    doctor.IsApproved = false;
                    doctor.SpecializationId=request.SpecializationId ?? 0;
                    await _unitOfWork.Doctors.AddAsync(doctor);
                }
                else
                {
                    Patient patient = new Patient();
                    patient.User = user;
                    patient.UserId = user.Id;
                    await _unitOfWork.Patients.AddAsync(patient);
                }
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction("Login");
            }

            foreach (var error in res.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specs, "Id", "Name");
            return View("Register", request);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginRequest request)
        {
            if (ModelState.IsValid)
            {
                if (request is null) return View("Login");
                var user = await _userManager.FindByNameAsync(request.UserName);
                if (user is null)
                {
                    ModelState.AddModelError("", "UserName is incorrect!");
                    return View("Login");
                }
                var flag = await _userManager.CheckPasswordAsync(user, request.Password);
                if (flag)
                {
                    await _signInManager.SignInAsync(user, request.Rememberme);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "the password or UserName is incorrect!!!");
            return View("Login");
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Doctor"))
                await roleManager.CreateAsync(new IdentityRole("Doctor"));

            if (!await roleManager.RoleExistsAsync("Patient"))
                await roleManager.CreateAsync(new IdentityRole("Patient"));
        }
    }
}
