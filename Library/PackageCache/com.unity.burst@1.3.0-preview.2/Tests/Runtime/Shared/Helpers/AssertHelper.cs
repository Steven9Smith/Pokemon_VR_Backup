using System;
using System.Numerics;
using NUnit.Framework;

#if BURST_INTERNAL
using System.Text;
using Unity.Burst.Intrinsics;
#endif

namespace Burst.Compiler.IL.Tests.Helpers
{
    internal static class AssertHelper
    {
#if BURST_INTERNAL
        // Workaround for Mono broken Equals() on v64/v128/v256
        private static bool AreVectorsEqual(v64 a, v64 b)
        {
            return a.SLong0 == b.SLong0;
        }

        private static bool AreVectorsEqual(v128 a, v128 b)
        {
            return a.SLong0 == b.SLong0 && a.SLong1 == b.SLong1;
        }

        private static bool AreVectorsEqual(v256 a, v256 b)
        {
            return AreVectorsEqual(a.Lo128, b.Lo128) && AreVectorsEqual(a.Hi128, b.Hi128);
        }
#endif

        /// <summary>
        /// AreEqual handling specially precision for float and intrinsic vector types
        /// </summary>
        /// <param name="expected">The expected result</param>
        /// <param name="result">the actual result</param>
        public static void AreEqual(object expected, object result, int maxUlp)
        {
            if (expected is float && result is float)
            {
                var expectedF = (float)expected;
                var resultF = (float)result;
                int ulp;
                Assert.True(NearEqualFloat(expectedF, resultF, maxUlp, out ulp), $"Expected: {expectedF} != Result: {resultF}, ULPs: {ulp}");
                return;
            }

            if (expected is double && result is double)
            {
                var expectedF = (double)expected;
                var resultF = (double)result;
                long ulp;
                Assert.True(NearEqualDouble(expectedF, resultF, maxUlp, out ulp), $"Expected: {expectedF} != Result: {resultF}, ULPs: {ulp}");
                return;
            }

#if BURST_INTERNAL
            if (expected is v64 && result is v64)
            {
                if (!AreVectorsEqual((v64)expected, (v64)result))
                {
                    Assert.Fail(FormatVectorFailure64((v64)expected, (v64)result));
                }
                return;
            }

            if (expected is v128 && result is v128)
            {
                if (!AreVectorsEqual((v128)expected, (v128)result))
                {
                    Assert.Fail(FormatVectorFailure128((v128)expected, (v128)result));
                }
                return;
            }

            if (expected is v256 && result is v256)
            {
                if (!AreVectorsEqual((v256)expected, (v256)result))
                {
                    Assert.Fail(FormatVectorFailure256((v256)expected, (v256)result));
                }
                return;
            }
#endif

            Assert.AreEqual(expected, result);
        }

#if BURST_INTERNAL
        private unsafe static string FormatVectorFailure64(v64 expected, v64 result)
        {
            var b = new StringBuilder();
            b.AppendLine("64-bit vectors differ!");
            b.AppendLine("Expected:");
            FormatVector(b, (void*)&expected, 8);
            b.AppendLine();
            b.AppendLine("But was :");
            FormatVector(b, (void*)&result, 8);
            b.AppendLine();
            return b.ToString();
        }

        private unsafe static string FormatVectorFailure128(v128 expected, v128 result)
        {
            var b = new StringBuilder();
            b.AppendLine("128-bit vectors differ!");
            b.AppendLine("Expected:");
            FormatVector(b, (void*) &expected, 16);
            b.AppendLine();
            b.AppendLine("But was :");
            FormatVector(b, (void*) &result, 16);
            b.AppendLine();
            return b.ToString();
        }

        private unsafe static string FormatVectorFailure256(v256 expected, v256 result)
        {
            var b = new StringBuilder();
            b.AppendLine("256-bit vectors differ!");
            b.AppendLine("Expected:");
            FormatVector(b, (void*) &expected, 32);
            b.AppendLine();
            b.AppendLine("But was :");
            FormatVector(b, (void*) &result, 32);
            b.AppendLine();
            return b.ToString();
        }

        private unsafe static void FormatVector(StringBuilder b, void* v, int bytes)
        {
            b.Append("Double: ");
            for (int i = 0; i < bytes / 8; ++i)
            {
                if (i > 0)
                    b.AppendFormat(" | ");
                b.AppendFormat("{0:G17}", ((double*)v)[i]);
            }
            b.AppendLine();
            b.Append("Float : ");
            for (int i = 0; i < bytes / 4; ++i)
            {
                if (i > 0)
                    b.AppendFormat(" | ");
                b.AppendFormat("{0:G15}", ((float*)v)[i]);
            }

            b.AppendLine();
            b.Append("UInt32: ");
            for (int i = 0; i < bytes / 4; ++i)
            {
                if (i > 0)
                    b.AppendFormat(" | ");
                b.AppendFormat("{0:X8}", ((uint*)v)[i]);
            }
            b.AppendLine();
        }
#endif

        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const float ZeroTolerance = 4 * float.Epsilon;

        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const double ZeroToleranceDouble = 4 * double.Epsilon;

        public static bool NearEqualFloat(float a, float b, int maxUlp, out int ulp)
        {
            ulp = 0;
            if (Math.Abs(a - b) < ZeroTolerance) return true;

            ulp = GetUlpFloatDistance(a, b);
            return ulp <= maxUlp;
        }

        public static unsafe int GetUlpFloatDistance(float a, float b)
        {
            // Save work if the floats are equal.
            // Also handles +0 == -0
            if (a == b)
            {
                return 0;
            }

            if (float.IsNaN(a) && float.IsNaN(b))
            {
                return 0;
            }

            if (float.IsInfinity(a) && float.IsInfinity(b))
            {
                return 0;
            }

            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            if ((aInt < 0) != (bInt < 0)) return int.MaxValue;

            // Because we would have an overflow below while trying to do -(int.MinValue)
            // We modify it here so that we don't overflow
            var ulp = (long)aInt - bInt;

            if (ulp <= int.MinValue) return int.MaxValue;
            if (ulp > int.MaxValue) return int.MaxValue;

            // We know for sure that numbers are in the range ]int.MinValue, int.MaxValue]
            return (int)Math.Abs(ulp);
        }

        public static bool NearEqualDouble(double a, double b, int maxUlp, out long ulp)
        {
            ulp = 0;
            if (Math.Abs(a - b) < ZeroTolerance) return true;

            ulp = GetUlpDoubleDistance(a, b);
            return ulp <= maxUlp;
        }

        private static readonly long LongMinValue = long.MinValue;
        private static readonly long LongMaxValue = long.MaxValue;

        public static unsafe long GetUlpDoubleDistance(double a, double b)
        {
            // Save work if the floats are equal.
            // Also handles +0 == -0
            if (a == b)
            {
                return 0;
            }

            if (double.IsNaN(a) && double.IsNaN(b))
            {
                return 0;
            }

            if (double.IsInfinity(a) && double.IsInfinity(b))
            {
                return 0;
            }

            long aInt = *(long*)&a;
            long bInt = *(long*)&b;

            if ((aInt < 0) != (bInt < 0)) return long.MaxValue;

            var ulp = aInt - bInt;

            if (ulp <= LongMinValue) return long.MaxValue;
            if (ulp > LongMaxValue) return long.MaxValue;

            return Math.Abs((long) ulp);
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(double a)
        {
            return Math.Abs(a) < ZeroToleranceDouble;
        }
    }
}