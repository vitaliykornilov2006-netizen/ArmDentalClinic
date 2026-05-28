using ARMDental.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace ARMDental
{
    public class AppDbContext : DbContext
    {
        private readonly string connectionString = "Host=localhost;Port=5432;Database=ARM_Dental_new;Username=postgres;Password=zxc";
        
        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<AppointmentDiagnosis> AppointmentDiagnoses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .Property(p => p.BirthDate)
                .HasColumnType("date");

            modelBuilder.Entity<Patient>()
                .Property(p => p.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Doctor>()
                .Property(d => d.EmploymentDate)
                .HasColumnType("date");

            modelBuilder.Entity<Doctor>()
                .Property(d => d.CreatedAt)
                .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<Appointment>()
                .Property(a => a.AppointmentDate)
                .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<Payment>()
                .Property(p => p.PayDate)
                .HasColumnType("timestamp with time zone");

            base.OnModelCreating(modelBuilder);
        }
    }
}
