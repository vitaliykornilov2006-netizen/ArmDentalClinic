using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARMDental.Models
{
    public class AppointmentService
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int ServiceId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal Price { get; set; }

        //nav
        public Appointment Appointment { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}