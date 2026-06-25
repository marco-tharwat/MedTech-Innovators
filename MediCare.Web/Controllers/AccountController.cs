using MediCare.Data.Models;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            if (request == null)
            {
                ModelState.AddModelError("", "The data is not found");
                return View("Register");
            }

            if (!ModelState.IsValid)
            {
                return View("Register", request);
            }

            ApplicationUser user = new ApplicationUser
            {
                FullName = request.Name,
                Gender = request.Gender,
                Email = request.Email,
                UserName = request.UserName
            };

            // 1️⃣ Create User
            var res = await _userManager.CreateAsync(user, request.Password);

            // 2️⃣ لو نجح بس
            if (res.Succeeded)
            {
                // 3️⃣ تأكد إن الرول موجود
                if (!string.IsNullOrEmpty(request.Role) &&
                    await _roleManager.RoleExistsAsync(request.Role))
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }

                return RedirectToAction("Index", "Home");
            }

            // 4️⃣ لو فشل اطبع الأخطاء
            foreach (var error in res.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

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
            if (!ModelState.IsValid)
                return View("Login");

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                ModelState.AddModelError("", "Username or password is incorrect");
                return View("Login");
            }

            var flag = await _userManager.CheckPasswordAsync(user, request.Password);

            if (flag)
            {
                await _signInManager.SignInAsync(user, request.Rememberme);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Username or password is incorrect");
            return View("Login");
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Seeder لل Roles
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