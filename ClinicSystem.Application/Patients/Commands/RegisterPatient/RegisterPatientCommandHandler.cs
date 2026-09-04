using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Patients;
using ClinicSystem.Domain.Patients.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Patients.Commands.RegisterPatient
{
    public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Guid>
    {
        private readonly IPatientRepository patientRepository;

        public RegisterPatientCommandHandler(IPatientRepository patientRepository)
        {
            this.patientRepository = patientRepository;
        }

        public async Task<Guid> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
        {
            var contactInfo = PatientContactInfo.Create(request.PhoneNumber, request.Email, request.Address);

            var mrn = await patientRepository.GenerateNextMedicalRecordNumberAsync(cancellationToken);

            var patient = Patient.Register(request.FullName, mrn , request.DateOfBirth , contactInfo);

            await patientRepository.AddAsync(patient, cancellationToken);

            return patient.Id;

        }
    }
}
