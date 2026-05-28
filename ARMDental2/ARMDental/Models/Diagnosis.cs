using System.ComponentModel.DataAnnotations;

namespace ARMDental.Models
{
    public class Diagnosis
    {
        public int Id { get; set; }
        
        [MaxLength(20)]
        [Required]
        public string Code { get; set; } = null!;  // код МКБ
        
        [MaxLength(200)]
        [Required]
        public string Name { get; set; } = null!;   // название диагноза
        
        public string? Description { get; set; }   // описание
        public ICollection<AppointmentDiagnosis> AppointmentDiagnoses { get; set; } = new List<AppointmentDiagnosis>();
    }
}
