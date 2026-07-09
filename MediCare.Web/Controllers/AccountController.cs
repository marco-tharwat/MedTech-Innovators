using MediCare.Data.Models;
using MediCare.Data.Repositories.Implementations;
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
        readonly IAccountRepositories _accountRepositories;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IAccountRepositories accountRepositories)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _accountRepositories = accountRepositories;
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
        { FullName = request.Name, Gender = request.Gender, Email = request.Email, UserName = request.UserName, Created = DateTime.Today };

            var response = await _accountRepositories
                .SetNewAccount(user, request.Role, request.SpecializationId, request.Password);

            if (response is null || response.Count() == 0)
                return RedirectToAction("Login");

            foreach (var err in response)
            {
                ModelState.AddModelError("", err);
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
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (roles.Contains("Doctor"))
                    {
                        return RedirectToAction("Index", "Doctor");
                    }
                    else if (roles.Contains("Patient"))
                    {
                        return RedirectToAction("Index", "Patient");
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "the password!");
            return View("Login");
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

    public static async Task SeedRolesAndAdminAccount(UserManager<ApplicationUser> userManager,
                                       RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Doctor"))
            await roleManager.CreateAsync(new IdentityRole("Doctor"));

        if (!await roleManager.RoleExistsAsync("Patient"))
            await roleManager.CreateAsync(new IdentityRole("Patient"));

        //------------- add an admin account for testing ------------

        var adminUsername = "admin";
        var adminPassword = "Admin@123";
        if (await userManager.FindByNameAsync(adminUsername) is null)
        {
            var admin = new ApplicationUser 
            { UserName = adminUsername, EmailConfirmed=true, FullName="Administrator" };
            var result = await userManager.CreateAsync(admin,adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
