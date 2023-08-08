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
    /// <summary>
    /// Describes how many times the animation loop is repeated.
    /// </summary>
    [TypeConverter(typeof(RepeatBehaviorConverter))]
    public readonly struct RepeatBehavior : IEquatable<RepeatBehavior>
    {
        /// <summary>
        /// Repeats according to image data.
        /// </summary>
        public static RepeatBehavior Default { get; }
            = new RepeatBehavior(ulong.MinValue, TimeSpan.MinValue);

        /// <summary>
        /// Repeats forever.
        /// </summary>
        public static RepeatBehavior Forever { get; }
            = new RepeatBehavior(default, default);

        /// <summary>
        /// Gets the number of times the animation loop is repeated.
        /// </summary>
        public ulong Count { get; }

        /// <summary>
        /// Gets the minimum time length the animation loop is repeated.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets a value of whether this instance has a specified iteration count.
        /// </summary>
        public bool HasCount { get; }

        /// <summary>
        /// Gets a value of whether this instance has a specified duration.
        /// </summary>
        public bool HasDuration { get; }

        private RepeatBehavior(ulong c, TimeSpan duration)
        {
            HasCount = false;
            Count = c;

            HasDuration = false;
            Duration = duration;
        }

        /// <summary>
        /// Creates the instance with specified iteration count.
        /// </summary>
        /// <param name="count"><see cref="Count"/></param>
        public RepeatBehavior(ulong count)
        {
            HasCount = true;
            Count = count;

            Duration = default;
            HasDuration = false;
        }

        /// <summary>
        /// Creates the instance with specified duration.
        /// </summary>
        /// <param name="duration"><see cref="Duration"/></param>
        public RepeatBehavior(TimeSpan duration)
        {
            HasCount = false;
            Count = default;

            HasDuration = true;
            Duration = duration;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HasCount.GetHashCode()
                + Count.GetHashCode()
                + HasDuration.GetHashCode()
                + Duration.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is RepeatBehavior bef && Equals(bef);
        }

        /// <inheritdoc/>
        public bool Equals(RepeatBehavior bef)
        {
            return HasCount == bef.HasCount
                && Count == bef.Count
                && HasDuration == bef.HasDuration
                && Duration == bef.Duration;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this == Forever ? "Forever" :
                   this == Default ? "Default" :
                   HasCount ? Count + "x" :
                   Duration.ToString("hh':'mm':'ss");

        }

        /// Whether <code>l</code> is equals to <code>r</code>.
        public static bool operator ==(RepeatBehavior l, RepeatBehavior r) => l.Equals(r);

        /// Whether <code>l</code> is not equals to <code>r</code>.
        public static bool operator !=(RepeatBehavior l, RepeatBehavior r) => !l.Equals(r);
    }

    /// <summary>
    /// TypeConverter for <see cref="RepeatBehavior"/>
    /// </summary>
    public class RepeatBehaviorConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type type)
        {
            return type == typeof(string);
        }

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? type)
        {
            return type == typeof(string);
        }

        /// <inheritdoc/>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object? data)
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

        /// <inheritdoc/>
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? data, Type toType)
        {
            if (data is RepeatBehavior behavior)
            {
                return behavior.ToString();
            }
            throw new ArgumentException($"{nameof(data)} is not {typeof(RepeatBehavior).Name}");
        }
    }
}
