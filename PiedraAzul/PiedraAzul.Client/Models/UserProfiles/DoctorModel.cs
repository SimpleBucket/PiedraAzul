using System;
using System.Collections.Generic;
using System.Text;

namespace PiedraAzul.Client.Models.UserProfiles
{
    public class DoctorModel
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string AvatarUrl { get; init; }
        public string Specialty { get; init; }
        public string Clinic { get; init; }
    }
}
