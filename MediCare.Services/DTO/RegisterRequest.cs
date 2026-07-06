using MediCare.Data.Models.Enum;

namespace MediCare.Services.DTO
{
    public class RegisterRequest
    {
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; }=null!;
        public Gender Gender { get; set; }
        public string Role {  get; set; }=null!;
        public string Password {  get; set; } = null!;
        public int? SpecializationId {  get; set; }
    }
}
