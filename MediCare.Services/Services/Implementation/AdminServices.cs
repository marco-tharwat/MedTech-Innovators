using MediCare.Data.Models;
using MediCare.Data.Models.Enum;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Factorys;
using MediCare.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Services
{
    public class AdminServices(IUnitOfWork _unitOfWork,UserManager<ApplicationUser> _userManager):IAdminServices
    {
        public async Task<ReportsResponse> GetReportsAsync()
        {
            var doctors = await _unitOfWork.Repository<Doctor>()
                .Query()
                .Include(d => d.Appointments)
                .Include(d => d.Specialization)
                .Include(d => d.User)
                .ToListAsync();
            var appointmentsPerDoctor = new Dictionary<string, List<Appointment>>();
            foreach (var doctor in doctors)
            {
                var Name = doctor.User?.UserName ?? "Unknown";
                if (!appointmentsPerDoctor.ContainsKey(Name))
                {
                    appointmentsPerDoctor[Name] = new List<Appointment>();
                }
                if (doctor.Appointments != null)
                {
                    appointmentsPerDoctor[Name].AddRange(doctor.Appointments);
                }
            }
            var allAppointments = doctors
                .SelectMany(d => d.Appointments ?? new List<Appointment>(), (doctor, appt) => new
                {
                    SpecializationName = doctor.Specialization?.Name ?? "unknown",
                    Appointment = appt
                }).ToList();

            var specializationsCount = allAppointments
                .GroupBy(x => x.SpecializationName)
                .ToDictionary(x => x.Key, x => x.Count());

            return new ReportsResponse
            {
                appointmentsPerDoctor = appointmentsPerDoctor,
                specializationsCount = specializationsCount
            };
        }

        public async Task<bool> UpdateValuesOfDoctor(UpdateDoctorRequest doctorRequest, int id)
        {
            if (doctorRequest is null) return false;
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor is null) return false;
            if (doctorRequest.IsApproved && !doctor.IsApproved)
            {
                var notification = NotificationFactory.DoctorApproved(doctor.UserId);
                await _unitOfWork.Repository<Notification>().AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
            }
            doctor.ConsultationFee = doctorRequest.ConsultationFee;
            doctor.Bio = doctorRequest.Bio;
            doctor.ImageURL = doctorRequest.ImageURL;
            doctor.Location = doctorRequest.Location;
            doctor.IsApproved = doctorRequest.IsApproved;
            doctor.SpecializationId = doctorRequest.SpecializationId;
            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<UpdateDoctorRequest?> mapFromDoctorToUpdateDoctorRequest(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor is null) return null;

            var model = new UpdateDoctorRequest
            {
                id = doctor.UserId,
                ConsultationFee = doctor.ConsultationFee,
                Bio = doctor.Bio ?? string.Empty,
                ImageURL = doctor.ImageURL ?? string.Empty,
                Location = doctor.Location ?? string.Empty,
                IsApproved = doctor.IsApproved,
                SpecializationId = doctor.SpecializationId
            };
            return model;
        }

        public async Task<bool> DeleteDoctor(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor is null) return false;
            _unitOfWork.Doctors.Remove(doctor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<Specialization?> GetSpecialization(int id)
        {
            var res = await _unitOfWork.Repository<Specialization>()
                .Query().Include(x => x.Doctors).ThenInclude(x=>x.User).FirstAsync(x => x.Id == id);
            return res;
        }

        public async Task<IEnumerable<SpecializationSetup>?> GetSpecializationSetupAsync()
        {
            IEnumerable<Specialization> raw = await _unitOfWork.Repository<Specialization>().GetAllAsync();
            if (raw is null) return null;
            IEnumerable<SpecializationSetup> data = raw.Select(x => new SpecializationSetup { Name = x.Name, id = x.Id });
            if (data is null) return null;
            return data;
        }

        public async Task<bool> UpdateAppointmentStatus(string status, int id)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment is null) return false;
            status = status.ToLower();
            switch (status)
            {
                case "pending":
                    appointment.Status = Status.Pending;
                    break;
                case "confirmed":
                    appointment.Status = Status.Confirmed;
                    break;
                case "completed":
                    appointment.Status = Status.Completed;
                    break;
                case "cancelled":
                    appointment.Status = Status.Cancelled;
                    break;
                default:
                    return false;
            }
            if (status == "cancelled")
            {
                _unitOfWork.Appointments.Remove(appointment);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<SummaryOfDataForAdmin> SetSummaryOfDataForAdmin()
        {
            var ListDoctors = await _unitOfWork.Doctors.GetAllAsync();
            int TotalNumberOfDoctors= ListDoctors.Count();
            int DoctorsPending=ListDoctors.Where(x=>x.IsApproved==false).Count();

            var ListOfPatients=await _unitOfWork.Patients.GetAllAsync();
            int TotalNumberOfPatients = ListOfPatients.Count();

            var ListOfAppointments=await _unitOfWork.Appointments.GetAllAsync();
            int NumOfAppointments = ListOfAppointments.Count();
            int NumOfAppointmentsScheduledForToday = ListOfAppointments.Where(x => x.AppointmentDate.Date == DateTime.Today).Count();

            return new SummaryOfDataForAdmin
            {
                TotalNumberOfDoctors= TotalNumberOfDoctors,
                TotalNumberOfPatients= TotalNumberOfPatients,
                DoctorsPending= DoctorsPending,
                NumOfAppointments= NumOfAppointments,
                NumOfAppointmentsScheduledForToday = NumOfAppointmentsScheduledForToday
            };
        }

        public async Task<Doctor?> FetchDoctorData(int id)
        {
            var data = await _unitOfWork.Doctors.Query().Include(x => x.User).FirstAsync(x=>x.Id==id);
            return data;
        }

        public async Task<IEnumerable<Doctor>> FetchDoctorsData()
        {
            var data =_unitOfWork.Doctors.Query().Include(x => x.User).Include(_=>_.Specialization).AsEnumerable();
            return data;
        }

        public async Task<IEnumerable<Patient>> FetchPatientsData()
        {
            var data = _unitOfWork.Patients.Query().Include(_ => _.User).AsEnumerable();
            return data;
        }

        public async Task<IEnumerable<Appointment>> FetchAppointmentsData()
        {
            return _unitOfWork.Appointments
                .Query()
                .Include(_ => _.Doctor).ThenInclude(_ => _.User)
                .Include(_ => _.Patient).ThenInclude(_ => _.User).AsEnumerable();
        }

        public async Task<IEnumerable<RegisteredAccountsData>> SetRegisteredAccountsData()
        {
            var data = await _unitOfWork.Repository<ApplicationUser>().GetAllAsync();
            List<RegisteredAccountsData> registeredAccountsDatas = new();
            foreach(var item in data)
            {
                var roles = await _userManager.GetRolesAsync(item);
                var role=roles[0];
                registeredAccountsDatas
                    .Add(new RegisteredAccountsData 
                    { Name = item.UserName ?? "", Email = item.Email, Role = role, Created = item.Created });
            }
            return registeredAccountsDatas;
        }
    }
}
