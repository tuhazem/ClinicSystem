using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Patients.Commands.RegisterPatient
{
    public record RegisterPatientCommand
    (
        string FullName,
        string PhoneNumber,
        DateTime DateOfBirth,
        string? Email = null,
        string? Address = null
     ) : IRequest<Guid>;
}
