﻿using System;
using System.Collections.Generic;
using TimeZoneConverter;

namespace Mirror.Calendar
{
    public class TimeZoneParser
    {
        const string TimeZoneId = "TZID";

        public static TimeZoneInfo Parse(Dictionary<string, List<string>> parameters)
        {
            if (parameters == null)
            {
                return null;
            }

            if (parameters.ContainsKey(TimeZoneId) && parameters[TimeZoneId].Count == 1)
            {
                var scrubbedValue = ScrubTimeZone(parameters[TimeZoneId][0]);
                return TryParse(scrubbedValue) ?? TryParse(scrubbedValue, true);
            }

            return null;
        }

        static TimeZoneInfo TryParse(string timezoneId, bool convertFromIana = false)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    convertFromIana
                        ? TZConvert.IanaToWindows(timezoneId)
                        : timezoneId);
            }
            catch
            {
                return null;
            }
        }

        static string ScrubTimeZone(string timezone)
        {
            var value =
                timezone.Replace("\"", string.Empty)
                        .Replace("(", string.Empty)
                        .Replace(")", string.Empty);

            return value.Contains("-")
                ? value.Substring(0, 3)
                : value;
        }                       
    }
}