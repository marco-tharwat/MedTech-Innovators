using MediCare.Data.Models;
using MediCare.Data.Models.Enum;
using MediCare.Data.Repositories.Implementations;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTO;
using MediCare.Services.Factorys;
namespace MediCare.Services.Services.Interfaces
{
    public interface IAdminServices
    {
        Task<ReportsResponse> GetReportsAsync();

        Task<bool> UpdateValuesOfDoctor(UpdateDoctorRequest doctorRequest, int id);

        Task<UpdateDoctorRequest?> mapFromDoctorToUpdateDoctorRequest(int id);

        Task<bool> DeleteDoctor(int id);

        Task<bool> DeletePatient(int id);

        Task<Specialization?> GetSpecialization(int id);

        Task<IEnumerable<SpecializationSetup>?> GetSpecializationSetupAsync();

        Task<bool> UpdateAppointmentStatus(string status, int id);

        Task<SummaryOfDataForAdmin> SetSummaryOfDataForAdmin();

        Task<Doctor?> FetchDoctorData(int id);

        Task<IEnumerable<Doctor>> FetchDoctorsData();

        Task<IEnumerable<Patient>> FetchPatientsData();
        Task<Patient> FetchPatientData(int id);

        Task<IEnumerable<Appointment>> FetchAppointmentsData();

        Task<IEnumerable<RegisteredAccountsData>> SetRegisteredAccountsData();

        Task<AppointmentsDTO> SetTheAppointments(string status, string order, string date, int PageNum);
        Task<bool> UpdatePatients(UpdatePatientsRequest request);

    }
}
