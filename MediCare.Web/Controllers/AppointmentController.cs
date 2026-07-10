using MediCare.Data.Models;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Services.Interfaces;
using MediCare.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IWorkingHoursService _workingHoursService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public AppointmentController(
            IAppointmentService appointmentService,
            IWorkingHoursService workingHoursService,
            UserManager<ApplicationUser> userManager,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository)
        {
            _appointmentService = appointmentService;
            _workingHoursService = workingHoursService;
            _userManager = userManager;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        // ── Patient: Book ─────────────────────────────────────────────────────

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Book(int doctorId, DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            // FIX: use the repository method that Includes Specialization.
            // Note: this method does NOT include the related User, so we fetch
            // it separately via UserManager using doctor.UserId.
            var doctor = await _doctorRepository.GetDoctorWithSpecializationAsync(doctorId);

            ApplicationUser? doctorUser = null;
            if (doctor is not null)
            {
                doctorUser = await _userManager.FindByIdAsync(doctor.UserId);
            }

            var slots = await _workingHoursService.GetAvailableSlotsAsync(doctorId, selectedDate);

            var vm = new BookAppointmentViewModel
            {
                DoctorId = doctorId,
                // FIX: added extra null-conditional (?.) before FullName / Name
                // so that a missing User or Specialization doesn't throw a
                // NullReferenceException.
                DoctorName = doctorUser?.FullName ?? "",
                SpecializationName = doctor?.Specialization?.Name ?? "",
                SelectedDate = selectedDate,
                AvailableSlots = slots.ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> Book(BookAppointmentViewModel vm)
        {
            if (vm.SelectedSlot is null)
            {
                ModelState.AddModelError("", "Please select a time slot.");
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            var patient = user?.PatientProfile;
            if (patient is null) return Forbid();

            var result = await _appointmentService
                .BookAsync(patient.Id, vm.DoctorId, vm.SelectedSlot.Value, vm.Notes);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage!);
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View(vm);
            }

            TempData["Success"] = "Appointment booked successfully.";
            return RedirectToAction("PatientAppointments");
        }

        // ── Patient: Cancel ───────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _appointmentService.CancelAsync(id, userId!);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Appointment cancelled." : result.ErrorMessage;

            return RedirectToAction("PatientAppointments");
        }

        // ── Patient: Reschedule ───────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Reschedule(int id, DateTime? date)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment is null) return NotFound();

            var selectedDate = date ?? DateTime.Today;
            var slots = await _workingHoursService
                .GetAvailableSlotsAsync(appointment.DoctorId, selectedDate);

            var vm = new BookAppointmentViewModel
            {
                AppointmentId = id,
                DoctorId = appointment.DoctorId,
                // FIX: guard against missing Doctor/User/Specialization
                DoctorName = appointment.Doctor?.User?.FullName ?? "",
                SpecializationName = appointment.Doctor?.Specialization?.Name ?? "",
                SelectedDate = selectedDate,
                AvailableSlots = slots.ToList()
            };

            return View("Book", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Reschedule(int id, BookAppointmentViewModel vm)
        {
            if (vm.SelectedSlot is null)
            {
                ModelState.AddModelError("", "Please select a time slot.");
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View("Book", vm);
            }

            var userId = _userManager.GetUserId(User);
            var result = await _appointmentService
                .RescheduleAsync(id, vm.SelectedSlot.Value, userId!);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage!);
                vm.AvailableSlots = (await _workingHoursService
                    .GetAvailableSlotsAsync(vm.DoctorId, vm.SelectedDate)).ToList();
                return View("Book", vm);
            }

            TempData["Success"] = "Appointment rescheduled successfully.";
            return RedirectToAction("PatientAppointments");
        }

        // ── Patient: List ─────────────────────────────────────────────────────

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PatientAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Forbid();

            // FIX: removed unused / risky variables (rol, id, userid) that added
            // no value and could throw if patient was null before the check.
            var patient = await _patientRepository.GetPatientByUserIdAsync(user.Id);
            if (patient is null) return Forbid();

            var appointments = await _appointmentService.GetByPatientIdAsync(patient.Id);
            return View(appointments.ToList());
        }

        // ── Doctor: Daily List ────────────────────────────────────────────────

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorDailyList(DateTime? date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Forbid();

            var doctor = await _doctorRepository.GetDoctorByUserIdAsync(user.Id);
            if (doctor is null) return Forbid();

            var selectedDate = date ?? DateTime.Today;
            var appointments = await _appointmentService.GetByDoctorIdAsync(doctor.Id);

            ViewBag.SelectedDate = selectedDate;
            return View(appointments
                .Where(a => a.AppointmentDate.Date == selectedDate.Date)
                .ToList());
        }

        // ── Doctor: Approve ───────────────────────────────────────────────────

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _appointmentService.ApproveAsync(id, userId!);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Appointment confirmed." : result.ErrorMessage;

            return RedirectToAction("DoctorDailyList");
        }

        // ── Doctor: Reject ────────────────────────────────────────────────────

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _appointmentService.RejectAsync(id, userId!);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Appointment rejected." : result.ErrorMessage;

            return RedirectToAction("DoctorDailyList");
        }

        // ── Doctor: Working Hours ─────────────────────────────────────────────

        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public async Task<IActionResult> ManageWorkingHours()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Forbid();

            var doctor = await _doctorRepository.GetDoctorByUserIdAsync(user.Id);
            if (doctor is null) return Forbid();

            var workingHours = await _workingHoursService.GetByDoctorIdAsync(doctor.Id);
            ViewBag.DoctorId = doctor.Id;
            return View(workingHours.ToList());
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> AddWorkingHours(int doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var result = await _workingHoursService.AddAsync(new WorkingHours
            {
                DoctorId = doctorId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            });

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Working hours added." : result.ErrorMessage;

            return RedirectToAction("ManageWorkingHours");
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> UpdateWorkingHours(int id, int doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var result = await _workingHoursService.UpdateAsync(new WorkingHours
            {
                Id = id,
                DoctorId = doctorId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            });

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Working hours updated." : result.ErrorMessage;

            return RedirectToAction("ManageWorkingHours");
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> DeleteWorkingHours(int id)
        {
            var result = await _workingHoursService.DeleteAsync(id);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Working hours removed." : result.ErrorMessage;

            return RedirectToAction("ManageWorkingHours");
        }
    }
}
