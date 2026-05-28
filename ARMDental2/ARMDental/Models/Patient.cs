using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARMDental.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Required]
        public string LastName { get; set; } = null!;
        [MaxLength(15)]
        [Required]
        public string FirstName { get; set; } = null!;
        [MaxLength(20)]
        public string? MiddleName { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [MaxLength(18)]
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //nav
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}