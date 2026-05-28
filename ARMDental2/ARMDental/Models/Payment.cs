using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARMDental.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        [Column(TypeName = "decimal(11,2)")]
        public decimal Amount { get; set; }
        [MaxLength(50)]
        public string? PayMethod { get; set; }
        public DateTime PayDate { get; set; } = DateTime.Now;
        [MaxLength(10)]
        public string Status { get; set; } = "Оплачено";

        //nav
        public Appointment? Appointment { get; set; }
    }
}