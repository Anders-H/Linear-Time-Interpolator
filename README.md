# Linear Time Interpolator

A class that lets you associate dates with numeric values and interpolate values between dates.

Example usage for retreiving interpolated values between dates:


```
var s = new Series();

// 2000-10-01 has value 5.
s.AddSource(2000, 10, 1, 5.0);

// 2020-10-01 has value 20.
s.AddSource(2020, 10, 1, 20.0);

// Therefore, halfway between these two dates, i.e., 2010-10-01, should have value 12.5.
Console.WriteLine(Math.Round(s.GetValue(2010, 10, 1), 1));

// A value close to the start date, e.g., 2001-10-01, should be close to 5.0 (actually 5.7).
Console.WriteLine(Math.Round(s.GetValue(2001, 10, 1), 1));

// And a value close to the end date, e.g., 2019-10-01, should be close to 20.0 (actually 19.2).
Console.WriteLine(Math.Round(s.GetValue(2019, 10, 1), 1));
```
