using Shared.Grpc;

namespace PiedraAzul.Client.Services.GrpcServices
{
    public class GrpcPatientService(PatientService.PatientServiceClient patientServiceClient)
    {
        public async Task<List<PatientSearchResult>> SearchAutoCompletePatientsAsync(string query, int limit)
        {
            var request = new SearchPatientsRequest
            {
                Query = query,
                Limit = limit
            };
            var response = await patientServiceClient.SearchAutoCompletePatientsAsync(request);
            return response.Patients.ToList();
        }
        public async Task<List<PatientSearchResult>> SearchPatientsAsync(string query, int limit)
        {
            var request = new SearchPatientsRequest
            {
                Query = query,
                Limit = limit
            };
            var response = await patientServiceClient.SearchPatientsAsync(request);
            return response.Patients.ToList();
        }
    }
}
