using PDR.PatientBooking.Service.Interfaces;
using System;

namespace PDR.PatientBooking.Service.Helpers
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
