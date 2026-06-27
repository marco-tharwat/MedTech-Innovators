using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCare.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var all = await _unitOfWork.Repository<Notification>().FindAsync(x=>x.UserId==userId);
            var notifications = all.OrderByDescending(n => n.CreatedAt).ToList();

            return View(notifications);
        }
    }
}