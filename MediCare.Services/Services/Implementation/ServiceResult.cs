namespace MediCare.Web.ViewModels
{
    public class ServiceResult
    {
        public bool Succeeded { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ServiceResult Success() => new ServiceResult { Succeeded = true };
        public static ServiceResult Failure(string error) => new ServiceResult { Succeeded = false, ErrorMessage = error };
    }
}
