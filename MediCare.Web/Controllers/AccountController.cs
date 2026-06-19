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

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            if (request == null) return BadRequest("ther is wrong with data!!");
            var test=await _userManager.FindByEmailAsync(request.Email);
            if(test is not null)
            {
                ModelState.AddModelError("Email", "this email is already exists");
                request.Email = "";
                return View("Register",request);
            }
            ApplicationUser user = new ApplicationUser { FullName=request.Name,Gender=request.Gender,Email=request.Email,UserName=request.Name+DateTime.Now.ToString("dMyyyy")+ request.Email};
            var res=await _userManager.CreateAsync(user,request.Password);
            await _userManager.AddToRoleAsync(user, request.Role);
            if (res.Succeeded)
            {
                return Ok("succeeded");
            }
            foreach (var error in res.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("Register");
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
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null)
                {
                    ModelState.AddModelError("", "the password or Email is incorrect!!!");
                    return View("Login");
                }
                var flag = await _userManager.CheckPasswordAsync(user,request.Password);
                if (flag) 
                { 
                    await _signInManager.SignInAsync(user,request.Rememberme);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "the password or Email is incorrect!!!");
            return View("Login");
        }

        [HttpPost]
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
