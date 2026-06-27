using MediCare.Data.Models;

namespace MediCare.Services.Factorys
{
    public class NotificationFactory
    {
        public static Notification Create(string id, string Message)
        {
            return new Notification
            {
                UserId = id,
                Message = Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            };
        }

        public static Notification DoctorApproved(string doctorUserId)
        => Create(doctorUserId, "Your doctor account has been approved by admin.");

        public static Notification AppointmentConfirmed(string patientUserId, string doctorName)
            => Create(patientUserId, $"Your appointment with Dr. {doctorName} has been confirmed.");

        public static Notification AppointmentCancelled(string patientUserId)
            => Create(patientUserId, "Your appointment has been cancelled.");
    }
}
