using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TimeZoneConverter;

namespace DMXCore.DMXCore100.Common;

public class TimeZoneOption
{
    public string Id { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public TimeZoneOption(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public string GetCurrentTime(DateTime utcNow)
    {
        var cultureInfo = CultureInfo.DefaultThreadCurrentUICulture;

        TZConvert.TryGetTimeZoneInfo(Id, out var timeZoneInfo);

        if (timeZoneInfo == null)
            return utcNow.ToString("t", cultureInfo);

        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

        return localNow.ToString("t", cultureInfo);
    }
}
