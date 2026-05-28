using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ARMDental.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [MaxLength(20)]
        [Required]
        public string LastName { get; set; } = null!;
        [MaxLength(15)]
        [Required]
        public string FirstName { get; set; } = null!;
        [MaxLength(20)]
        public string? MiddleName { get; set; }
        [Required]
        [MaxLength(50)]
        public string Specialization { get; set; } = null!;
        [MaxLength(16)]
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EmploymentDate { get; set; }

        //nav
        public User? User { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}