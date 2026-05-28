using System;
using System.Collections.Generic;
using System.Text;

namespace ARMDental.Models
{
    public class AppointmentDiagnosis
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public int DiagnosisId { get; set; }

        // nav
        public Appointment Appointment { get; set; } = null!;
        public Diagnosis Diagnosis { get; set; } = null!;
    }
}