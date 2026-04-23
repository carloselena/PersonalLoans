namespace Blocks.Domain.Time;

public static class DateProvider
{
    private static readonly TimeZoneInfo DrTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

    public static DateTimeOffset UtcNow()
        => DateTimeOffset.UtcNow;

    public static DateTimeOffset Now()
        => TimeZoneInfo.ConvertTime(UtcNow(), DrTimeZone);

    public static DateOnly Today()
        => DateOnly.FromDateTime(Now().DateTime);
    
    public static DateOnly ToLocalDate(DateTimeOffset dateTime)
        => DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(dateTime, DrTimeZone).DateTime);
}