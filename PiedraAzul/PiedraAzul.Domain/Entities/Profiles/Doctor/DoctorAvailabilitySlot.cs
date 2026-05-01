using PiedraAzul.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiedraAzul.Domain.Entities.Profiles.Doctor
{
    public class DoctorAvailabilitySlot
    {
        public Guid Id { get; private set; }

        public string DoctorId { get; private set; }

        public DayOfWeek DayOfWeek { get; private set; }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        private DoctorAvailabilitySlot() { }

        public DoctorAvailabilitySlot(
            string doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan start,
            TimeSpan end)
        {
            if (start >= end)
                throw new DomainException("Invalid range");

            Id = Guid.NewGuid();
            DoctorId = doctorId;
            DayOfWeek = dayOfWeek;
            StartTime = start;
            EndTime = end;
        }

        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
        }

        public bool Matches(DateOnly date)
        {
            return date.DayOfWeek == DayOfWeek;
        }
    }
}
