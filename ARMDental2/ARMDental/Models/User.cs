using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ARMDental.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //nav
        public Doctor? Doctor { get; set; }
    }
}