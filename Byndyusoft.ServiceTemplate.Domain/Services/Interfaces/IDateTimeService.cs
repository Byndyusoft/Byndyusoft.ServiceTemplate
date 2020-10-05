namespace Byndyusoft.ServiceTemplate.Domain.Services.Interfaces
{
    using System;

    public interface IDateTimeService
    {
        DateTime UtcNow();
        DateTimeOffset OffsetUtcNow();
    }
}