using Grpc.Core;
using PiedraAzul.Shared.Grpc;
using Shared.Grpc;

namespace PiedraAzul.GrpcServices
{
    public class GrpcDoctor(PiedraAzul.ApplicationServices.Services.IDoctorService doctorService) : DoctorService.DoctorServiceBase
    {
        public override Task<DoctorResponse> GetDoctor(DoctorRequest request, ServerCallContext context)
        {
            return base.GetDoctor(request, context);
        }
        public override Task<DoctorListResponse> GetDoctors(Empty request, ServerCallContext context)
        {
            return base.GetDoctors(request, context);
        }
        public override async Task<DoctorListResponse> GetDoctorsByType(DoctorTypeRequest request, ServerCallContext context)
        {
            var response = await doctorService.GetDoctorByTypeAsync((PiedraAzul.Shared.Enums.DoctorType)request.DoctorType);

            DoctorListResponse doctorListResponse= new DoctorListResponse();
            doctorListResponse.Doctors.AddRange(response.Select(d => new DoctorResponse
            {
                UserId = d.UserId,
                DoctorId = d.DoctorId.ToString(),
                LicenseNumber = d.LicenseNumber,
                Specialty = (DoctorType)d.Specialty,
                Name = d.User.Name,
                AvatarUrl = d.User.AvatarUrl,
                Notes = d.Notes,
            }));

            return doctorListResponse;
        }
    }
}
