using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Patients.Queries.GetPatientById
{
    public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDto?>
    {
        private readonly IPatientRepository _patientRepository;

        public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<PatientDto?> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken);
            if (patient == null)
            {
                return null;
            }

            return new PatientDto(
                patient.Id,
                patient.FullName,
                patient.MedicalRecordNumber,
                patient.DateOfBirth,
                patient.ContactInfo.PhoneNumber,
                patient.ContactInfo.Email,
                patient.ContactInfo.Address,
                patient.MedicalHistory,
                patient.CreatedAtUtc,
                patient.LastModifiedAtUtc
            );
        }
    }
}
