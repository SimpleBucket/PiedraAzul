using Grpc.Core;
using Shared.Grpc;

namespace PiedraAzul.GrpcServices
{
    public class GrpcAppointment(PiedraAzul.ApplicationServices.Services.IAppointmentService appointmentService) 
        : AppointmentService.AppointmentServiceBase
    {
        public override Task<AppointmentResponse> CreateAppointment(CreateAppointmentRequest request, ServerCallContext context)
        {
            CreateAppointmentRequest appointmentRequest = new CreateAppointmentRequest
            {
                PatientId = request.PatientId,
                AppointmentSlotId = request.AppointmentSlotId
            };
            return base.CreateAppointment(request, context);
        }
    }
}
