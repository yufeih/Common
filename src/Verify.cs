namespace System
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Contains commonly used verifications.
    /// </summary>
    [DebuggerStepThrough]
    static class Verify
    {
        public static void NotNull(object target, [CallerMemberName]string member = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(member);
            }
        }

        public static void NotEmpty(string target, [CallerMemberName]string member = null)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentException("string cannot be empty", member);
            }
        }

        public static void NotEmpty<T>(IEnumerable<T> target, [CallerMemberName]string member = null)
        {
            if (target == null || !target.Any())
            {
                throw new ArgumentOutOfRangeException("array cannot be empty", member);
            }
        }

        public static void Between(int value, int min, int max, [CallerMemberName]string member = null)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(string.Concat(member, ", ", value.ToString()));
            }
        }

        public static void NotContains<T>(T value, IEnumerable<T> invalidValues, [CallerMemberName]string member = null)
        {
            if (invalidValues.Contains(value))
            {
                if (value == null) throw new ArgumentNullException(member);
                throw new ArgumentOutOfRangeException(string.Concat(member, ", ", value.ToString()));
            }
        }

        public static void Contains<T>(T value, IEnumerable<T> validValues, [CallerMemberName]string member = null)
        {
            if (!validValues.Contains(value))
            {
                if (value == null) throw new ArgumentNullException(member);
                throw new ArgumentOutOfRangeException(string.Concat(member, ", ", value.ToString()));
            }
        }

        public static void NullOrContains<T>(T value, IEnumerable<T> validValues, [CallerMemberName]string member = null)
        {
            if (value != null && !validValues.Contains(value))
            {
                throw new ArgumentOutOfRangeException(string.Concat(member, ", ", value.ToString()));
            }
        }

        public static void ShorterThan(string text, int maxLength, [CallerMemberName]string member = null)
        {
            if (text != null && text.Length > maxLength)
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} cannot be longer than {1}, got {2}", member, maxLength, text.Length));
            }
        }

        public static void IsTrue(bool value, [CallerMemberName]string member = null)
        {
            if (!value)
            {
                throw new InvalidOperationException("Expression is not true. " + member);
            }
        }

        public static void IsFalse(bool value, [CallerMemberName]string member = null)
        {
            if (value)
            {
                throw new InvalidOperationException("Expression is not false. " + member);
            }
        }

        public static void AreEqual<T>(T x, T y)
        {
            if (!object.Equals(x, y))
            {
                throw new InvalidOperationException("Value has to be equal");
            }
        }

        public static void AreNotEqual<T>(T x, T y)
        {
            if (object.Equals(x, y))
            {
                throw new InvalidOperationException("Value cannot be equal");
            }
        }
    }
}

