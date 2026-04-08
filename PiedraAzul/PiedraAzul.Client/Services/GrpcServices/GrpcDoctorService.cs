using PiedraAzul.Contracts.Grpc;

namespace PiedraAzul.Client.Services.GrpcServices
{
    public class GrpcDoctorService(DoctorService.DoctorServiceClient doctorClient)
    {
        public async Task<List<DoctorResponse>> GetDoctorsByTypeAsync(Contracts.Enums.DoctorType doctorType)
        {
            var request = new DoctorTypeRequest { DoctorType = (DoctorType)doctorType };
            var response = await doctorClient.GetDoctorsByTypeAsync(request);
            return response.Doctors.ToList();
        }
    }
}
