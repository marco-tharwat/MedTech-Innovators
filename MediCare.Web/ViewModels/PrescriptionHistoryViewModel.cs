namespace MediCare.Web.ViewModels
{

    public record PrescriptionHistoryViewModel(
        List<PrescriptionHistoryItemViewModel> Prescriptions
    );
}
