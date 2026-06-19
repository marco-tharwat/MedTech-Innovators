using MediCare.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IUnitOfWork _unitOfWork;

        public AdminController(UserManager<ApplicationUser> userManager/*, IUnitOfWork unitOfWork*/)
        {
            _userManager = userManager;
            //_unitOfWork = unitOfWork;
        }

        public IActionResult Dashboard() => View();
        public IActionResult ManageDoctors() => View();
        public IActionResult ManagePatients() => View();
        public IActionResult AllAppointments() => View();
    }
}
