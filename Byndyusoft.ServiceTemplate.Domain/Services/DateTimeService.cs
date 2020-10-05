namespace Byndyusoft.ServiceTemplate.Domain.Services
{
    using System;
    using Interfaces;

    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        public DateTimeOffset OffsetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}