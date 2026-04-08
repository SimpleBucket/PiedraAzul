using PiedraAzul.Contracts.Grpc;
using System.Reflection.Metadata;

namespace PiedraAzul.Client.Services.Utils
{
    public static class CreateContracts
    {
        public static GuestPatientInput CreateGuestPatientInput(string patientName, string patientPhone, string patientIdentification, string extraInfo)
        {
            return new GuestPatientInput
            {
                Name = patientName,
                Phone = patientPhone,
                Identification = patientIdentification,
                ExtraInfo = extraInfo
            };
        }
    }
}
