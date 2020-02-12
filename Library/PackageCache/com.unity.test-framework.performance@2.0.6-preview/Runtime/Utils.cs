using System;
using System.Collections.Generic;
using System.IO;
using Unity.PerformanceTesting.Exceptions;
using UnityEngine;

namespace Unity.PerformanceTesting.Runtime
{
    public static class Utils
    {
        public static string ResourcesPath => Path.Combine(Application.dataPath, "Resources");
        public const string TestRunPath = "Assets/Resources/" + TestRunInfo;
        public const string TestRunInfo = "PerformanceTestRunInfo.json";
        public const string PlayerPrefKeyRunJSON = "PT_Run";

        public static DateTime ConvertFromUnixTimestamp(int timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        internal static void ShiftUnit(this Data.SampleGroup sg)
        {
            if (sg.Unit == SampleUnit.Undefined) return;

            while (true)
            {
                if (sg.Median > 10000)
                {
                    if (sg.Unit == SampleUnit.Second || sg.Unit == SampleUnit.Gigabyte)
                        break;

                    Shrink(sg);
                    continue;
                }

                if (sg.Median < 10)
                {
                    if (sg.Unit == SampleUnit.Nanosecond || sg.Unit == SampleUnit.Byte)
                        break;

                    Expand(sg);
                    continue;
                }

                break;
            }
        }

        private static void Shrink(this Data.SampleGroup sg)
        {
            for (var i = 0; i < sg.Samples.Count; i++)
            {
                sg.Samples[i] = ConvertSample(sg.Unit, sg.Unit + 1, sg.Samples[i]);
            }

            sg.Unit += 1;
            sg.UpdateStatistics();
        }

        private static void Expand(this Data.SampleGroup sg)
        {
            for (var i = 0; i < sg.Samples.Count; i++)
            {
                sg.Samples[i] = ConvertSample(sg.Unit, sg.Unit - 1, sg.Samples[i]);
            }

            sg.Unit -= 1;
            sg.UpdateStatistics();
        }

        public static double ConvertSample(SampleUnit from, SampleUnit to, double value)
        {
            double f = RelativeSampleUnit(from);
            double t = RelativeSampleUnit(to);

            return value * (f / t);
        }

        public static double RelativeSampleUnit(SampleUnit unit)
        {
            switch (unit)
            {
                case SampleUnit.Nanosecond:
                    return 1;
                case SampleUnit.Microsecond:
                    return 1000;
                case SampleUnit.Millisecond:
                    return 1000000;
                case SampleUnit.Second:
                    return 1000000000;
                case SampleUnit.Byte:
                    return 1;
                case SampleUnit.Kilobyte:
                    return 1000;
                case SampleUnit.Megabyte:
                    return 1000000;
                case SampleUnit.Gigabyte:
                    return 1000000000;
                default:
                    throw new PerformanceTestException(
                        "Wrong SampleUnit type used.");
            }
        }

        public static void UpdateStatistics(this Data.SampleGroup sampleGroup)
        {
            if (sampleGroup.Samples == null) return;
            var samples = sampleGroup.Samples;
            if (samples.Count < 2)
            {
                sampleGroup.Min = samples[0];
                sampleGroup.Max = samples[0];
                sampleGroup.Median = samples[0];
                sampleGroup.Average = samples[0];
                sampleGroup.Sum = samples[0];
                sampleGroup.StandardDeviation = 0;
            }
            else
            {
                sampleGroup.Min = Utils.Min(samples);
                sampleGroup.Max = Utils.Max(samples);
                sampleGroup.Median = Utils.GetMedianValue(samples);
                sampleGroup.Average = Utils.Average(samples);
                sampleGroup.Sum = Utils.Sum(samples);
                sampleGroup.StandardDeviation = Utils.GetStandardDeviation(samples, sampleGroup.Average);
            }
        }

        public static int GetZeroValueCount(List<double> samples)
        {
            var zeroValues = 0;
            foreach (var sample in samples)
            {
                if (Math.Abs(sample) < .0001f)
                {
                    zeroValues++;
                }
            }

            return zeroValues;
        }

        public static double GetMedianValue(List<double> samples)
        {
            var samplesClone = new List<double>(samples);
            samplesClone.Sort();

            var middleIdx = samplesClone.Count / 2;
            return samplesClone[middleIdx];
        }

        public static double GetPercentile(List<double> samples, double percentile)
        {
            if (percentile < 0.00001D)
                return percentile;

            var samplesClone = new List<double>(samples);
            samplesClone.Sort();

            if (samplesClone.Count == 1)
            {
                return samplesClone[0];
            }

            var rank = percentile * (samplesClone.Count + 1);
            var integral = (int)rank;
            var fractional = rank % 1;
            return samplesClone[integral - 1] + fractional * (samplesClone[integral] - samplesClone[integral - 1]);
        }

        public static double GetStandardDeviation(List<double> samples, double average)
        {
            double sumOfSquaresOfDifferences = 0.0D;
            foreach (var sample in samples)
            {
                sumOfSquaresOfDifferences += (sample - average) * (sample - average);
            }

            return Math.Sqrt(sumOfSquaresOfDifferences / samples.Count);
        }

        public static double Min(List<double> samples)
        {
            double min = Mathf.Infinity;
            foreach (var sample in samples)
            {
                if (sample < min) min = sample;
            }

            return min;
        }

        public static double Max(List<double> samples)
        {
            double max = Mathf.NegativeInfinity;
            foreach (var sample in samples)
            {
                if (sample > max) max = sample;
            }

            return max;
        }

        public static double Average(List<double> samples)
        {
            return Sum(samples) / samples.Count;
        }

        public static double Sum(List<double> samples)
        {
            double sum = 0.0D;
            foreach (var sample in samples)
            {
                sum += sample;
            }

            return sum;
        }

        public static string RemoveIllegalCharacters(string path)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                path = path.Replace(c.ToString(), "");
            }

            return path;
        }

        public static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
