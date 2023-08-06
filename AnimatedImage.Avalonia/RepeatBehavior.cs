using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimatedImage.Avalonia
{
    [TypeConverter(typeof(RepeatBehaviorConverter))]
    public struct RepeatBehavior
    {
        public static RepeatBehavior Default { get; }
            = new RepeatBehavior(ulong.MinValue, TimeSpan.MinValue);

        public static RepeatBehavior Forever { get; }
            = new RepeatBehavior(default, default);

        public ulong Count { get; }
        public TimeSpan Duration { get; }
        public bool HasCount { get; }
        public bool HasDuration { get; }

        private RepeatBehavior(ulong c, TimeSpan duration)
        {
            HasCount = false;
            Count = c;

            HasDuration = false;
            Duration = duration;
        }

        public RepeatBehavior(ulong count)
        {
            HasCount = true;
            Count = count;

            Duration = default;
            HasDuration = false;
        }

        public RepeatBehavior(TimeSpan duration)
        {
            HasCount = false;
            Count = default;

            HasDuration = true;
            Duration = duration;
        }

        public override int GetHashCode()
        {
            return HasCount.GetHashCode()
                + Count.GetHashCode()
                + HasDuration.GetHashCode()
                + Duration.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is RepeatBehavior bef && Equals(bef);
        }

        public bool Equals(RepeatBehavior bef)
        {
            return HasCount == bef.HasCount
                && Count == bef.Count
                && HasDuration == bef.HasDuration
                && Duration == bef.Duration;
        }

        public override string ToString()
        {
            return this == Forever ? "Forever" :
                   this == Default ? "Default" :
                   HasCount ? Count + "x" :
                   Duration.ToString("hh':'mm':'ss");

        }

        public static bool operator ==(RepeatBehavior l, RepeatBehavior r) => l.Equals(r);
        public static bool operator !=(RepeatBehavior l, RepeatBehavior r) => !l.Equals(r);
    }

    public class RepeatBehaviorConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type type)
        {
            return type == typeof(string);
        }
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type type)
        {
            return type == typeof(string);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo culture, Object? data)
        {
            if (data is string text)
            {
                if (Eq("default", text) || Eq("0x", text))
                {
                    return RepeatBehavior.Default;
                }
                if (Eq("infinite", text) || Eq("forever", text))
                {
                    return RepeatBehavior.Forever;
                }
                if (Regex.IsMatch(text, "^[0-9]+x$")
                    && ulong.TryParse(text.Substring(0, text.Length - 1), out ulong count))
                {
                    return new RepeatBehavior(count);
                }
                if (TimeSpan.TryParse(text, out var duration))
                {
                    return new RepeatBehavior(duration);
                }

                throw new FormatException($"Failure to parse '{text}' to {typeof(RepeatBehavior).Name}");

                static bool Eq(string t1, string t2)
                    => t1.Equals(t2, StringComparison.InvariantCultureIgnoreCase);
            }
            throw new ArgumentException($"{nameof(data)} cannot be casted to string");
        }


        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo culture, Object data, Type toType)
        {
            if (data is RepeatBehavior behavior)
            {
                return behavior.ToString();
            }
            throw new ArgumentException($"{nameof(data)} is not {typeof(RepeatBehavior).Name}");
        }
    }
}
