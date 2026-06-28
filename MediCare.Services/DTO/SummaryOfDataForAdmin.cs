namespace MediCare.Services.DTO;
    public class SummaryOfDataForAdmin
    {
        public int TotalNumberOfDoctors { get; set; }
        public int TotalNumberOfPatients { get; set; }
        public int DoctorsPending {  get; set; }
        public int NumOfAppointments {  get; set; }
        public int NumOfAppointmentsScheduledForToday { get; set; }

    }