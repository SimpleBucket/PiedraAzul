namespace PiedraAzul.ApplicationServices.AutoCompleteServices
{
    public interface IDoctorAutocompleteService
    {
        void IndexDoctor(object patient);
        List<PatientAutocompleteResult> Search(string text, int max = 10);
    }
    public class DoctorAutocompleteService
    {
    }
}
