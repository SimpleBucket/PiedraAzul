using Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiedraAzul.Application.Features.Patients.Commands.CreateGuestPatient
{
    public record CreateGuestPatientCommand(
    string Name,
    string Phone,
    string ExtraInfo
) : IRequest<string>;
}
