namespace EShop.Shared.SystemClock.Abstractions;

public interface ISystemClock
{
    DateTimeOffset GetUtcNow();

    /// <summary>
    /// Setting Now to a custom date time will freeze flow of time and the clock with allways return specified value.
    /// If at the same time an Offset is set the returned time will be based on the specified value with the offset applied.
    /// To make sure the time still flows but control the value please use SetOffset instead.
    /// </summary>
    Task SetUtcNow(DateTimeOffset customDatetime);
}