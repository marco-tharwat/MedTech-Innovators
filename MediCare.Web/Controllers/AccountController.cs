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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        // ================= REGISTER =================
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specs, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var user = new ApplicationUser
            {
                FullName = request.Name,
                Gender = request.Gender,
                Email = request.Email,
                UserName = request.UserName,
                Created = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // ✅ إضافة الـ Role
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

                if (!roleResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to assign role");
                    return View(request);
                }

                // ✅ إنشاء Doctor أو Patient
                if (request.Role == "Doctor")
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        IsApproved = false,
                        SpecializationId = request.SpecializationId ?? 0
                    };
                    await _unitOfWork.Doctors.AddAsync(doctor);
                }
                else
                {
                    var patient = new Patient
                    {
                        UserId = user.Id
                    };
                    await _unitOfWork.Patients.AddAsync(patient);
                }

                await _unitOfWork.SaveChangesAsync();

                // 🔥 مهم جدًا: اعمل Logout عشان الـ Claims تتعمل صح
                await _signInManager.SignOutAsync();

                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var specs = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            ViewBag.Specializations = new SelectList(specs, "Id", "Name");

            return View(request);
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await _signInManager.PasswordSignInAsync(
                request.UserName,
                request.Password,
                request.Rememberme,
                false
            );

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid Username or Password");
            return View(request);
        }

        // ================= LOGOUT =================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ================= ACCESS DENIED =================
        public IActionResult AccessDenied()
        {
            return Content("Access Denied 🚫");
        }

        // ================= SEED ROLES =================
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