using System.Globalization;

namespace Jenkade.Presentation;

/// <summary>Formats dates, times and durations in the current <see cref="UiLanguage"/>.</summary>
public static class DisplayFormat
{
    /// <summary>Short date and time, e.g. <c>22.10.2026 14:05</c> in Polish.</summary>
    public static string Timestamp(DateTimeOffset timestamp) => timestamp.ToString("g", CultureInfo.CurrentCulture);

    /// <summary>The two largest whole units, e.g. <c>3 min 7 s</c> or <c>2 h 15 min</c>.</summary>
    public static string Duration(TimeSpan duration)
    {
        var hours = (int)duration.TotalHours;
        if (hours > 0)
            return Format(Strings.DurationHoursMinutes, hours, duration.Minutes);
        if (duration.Minutes > 0)
            return Format(Strings.DurationMinutesSeconds, duration.Minutes, duration.Seconds);
        return Format(Strings.DurationSeconds, duration.Seconds);
    }

    private static string Format(string format, params object[] values) =>
        string.Format(CultureInfo.CurrentCulture, format, values);
}
