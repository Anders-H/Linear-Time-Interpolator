#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LinearTimeInterpolator;

public class Series
{
    public string SeriesName { get; set; }
    public SortedList<DateTime, ValuePoint> SourceValuePoints { get; } = new();

    public Series() : this("")
    {
    }

    public Series(string? seriesName)
    {
        SeriesName = (seriesName ?? "").Trim();
    }


    public void AddSource(int year, int month, int day, double value) =>
        AddSource(new ValuePoint(new DateTime(year, month, day), value));

    public void AddSource(DateTime timePoint, double value) =>
        AddSource(new ValuePoint(timePoint, value));

    public void AddSource(ValuePoint valuePoint)
    {
        var key = new DateTime(valuePoint.TimePoint.Year, valuePoint.TimePoint.Month, valuePoint.TimePoint.Day);
        SourceValuePoints.Remove(key);
        valuePoint.TimePoint = key;
        SourceValuePoints.Add(key, valuePoint);
    }

    public ValuePoint? GetValuePoint(DateTime timePoint)
    {
        var key = new DateTime(timePoint.Year, timePoint.Month, timePoint.Day);
        var ok = SourceValuePoints.TryGetValue(key, out var v);
        return ok ? v : null;
    }

    public double GetValue(int year, int month, int day) =>
        GetValueFromKey(new DateTime(year, month, day));

    public double GetValue(DateTime timePoint) =>
        GetValueFromKey(new DateTime(timePoint.Year, timePoint.Month, timePoint.Day));

    private double GetValueFromKey(DateTime key)
    {
        switch (SourceValuePoints.Count)
        {
            case 0:
                return 0.0;
            case 1:
                return SourceValuePoints.First().Value.Value;
            default:
                var isHit = SourceValuePoints.TryGetValue(key, out var hit);

                if (isHit && hit != null)
                    return hit.Value;

                if (key <= SourceValuePoints.First().Key)
                    return SourceValuePoints.First().Value.Value;

                if (key >= SourceValuePoints.Last().Key)
                    return SourceValuePoints.Last().Value.Value;

                GetValuePointsAroundKey(key, out var before, out var after);
                before ??= SourceValuePoints.First().Value;
                after ??= SourceValuePoints.Last().Value;
                var fullDistance = before.DistanceInDays(after);
                var requestedDistance = before.DistanceInDays(key);
                var percent = requestedDistance / fullDistance;
                return before.Value + percent * (after.Value - before.Value);
        }
    }

    private static string CleanString(string? s) =>
        (s ?? "").Replace("|", "").Replace("~", "").Replace("¤", "").Trim();

    internal string SaveString()
    {
        var s = new StringBuilder();
        s.Append(CleanString(SeriesName));
        s.Append("|");

        switch (SourceValuePoints.Count)
        {
            case 0:
                break;
            case 1:
                s.Append(SourceValuePoints.First().Value.SaveString());
                break;
            default:
                foreach (var x in SourceValuePoints)
                {
                    s.Append(x.Value.SaveString());
                    if (x.Value != SourceValuePoints.Last().Value)
                        s.Append("¤");
                }
                break;
        }

        return s.ToString();
    }

    public static Series? Parse(string data)
    {
        var parts = data.Split('|');

        if (parts.Length < 1)
            return null;

        if (parts.Length == 1)
            return new Series(parts[0]);

        var ret = new Series(parts[0]);
        var values = parts[1].Split('¤');

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (value.Length < 8)
                continue;

            int.TryParse(value.Substring(0, 4), out var year);
            int.TryParse(value.Substring(4, 2), out var month);
            int.TryParse(value.Substring(6, 2), out var day);
            var v = 0.0;

            if (value.Length > 8)
                double.TryParse(value.Substring(8), NumberStyles.Any, CultureInfo.InvariantCulture, out v);

            ret.AddSource(year, month, day, v);
        }

        return ret;
    }

    private void GetValuePointsAroundKey(DateTime key, out ValuePoint? before, out ValuePoint? after)
    {
        before = null;
        after = null;
        DateTime? prevKey = null;

        foreach (var x in SourceValuePoints.Values)
        {
            if (prevKey != null && key >= prevKey.Value && key <= x.TimePoint)
            {
                before = SourceValuePoints[prevKey.Value];
                after = x;
                return;
            }

            prevKey = x.TimePoint;
        }
    }

    public string SeriesDateSpanDescription =>
            SourceValuePoints.Count switch
            {
                0 => "",
                1 => SourceValuePoints.First().Value.TimePoint.ToShortDateString(),
                _ => $"{SourceValuePoints.First().Value.TimePoint.ToShortDateString()} - {SourceValuePoints.Last().Value.TimePoint.ToShortDateString()}"
            };

    public override string ToString()
    {
        var name = string.IsNullOrWhiteSpace(SeriesName) ? "[Nameless series]" : SeriesName.Trim();

        return SourceValuePoints.Count switch
        {
            0 => name,
            1 => $"{name} ({SeriesDateSpanDescription})",
            _ => $"{name} ({SourceValuePoints.Count} values, {SeriesDateSpanDescription})"
        };
    }
}