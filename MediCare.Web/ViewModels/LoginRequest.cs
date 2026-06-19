namespace MediCare.Web.ViewModels
{
    public class LoginRequest
    {
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool Rememberme {  get; set; }
    }
}
