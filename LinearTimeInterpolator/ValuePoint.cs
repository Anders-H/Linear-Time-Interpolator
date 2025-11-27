#nullable enable
using System;

namespace LinearTimeInterpolator;

public class ValuePoint
{
    public DateTime TimePoint { get; set; }
    public double Value { get; set; }

    public ValuePoint(DateTime timePoint, double value)
    {
        TimePoint = timePoint;
        Value = value;
    }

    public double DistanceInDays(ValuePoint v) =>
        DistanceInDays(v.TimePoint);
    
    public double DistanceInDays(DateTime t) =>
        Math.Abs(t.Subtract(TimePoint).TotalDays);
    
    internal string SaveString() => $"{TimePoint.Year:0000}{TimePoint.Month:00}{TimePoint.Day:00}{Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
}