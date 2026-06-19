using MediCare.Data.Models;

namespace MediCare.Web.ViewModels
{
    public class RegisterRequest
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; }=null!;
        public Gender Gender { get; set; }
        public string Role {  get; set; }=null!;
        public string Password {  get; set; } = null!;
    }
}
