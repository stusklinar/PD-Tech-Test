using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
