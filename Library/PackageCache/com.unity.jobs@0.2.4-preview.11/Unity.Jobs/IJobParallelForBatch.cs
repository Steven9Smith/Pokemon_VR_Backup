﻿using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Jobs
{
    [JobProducerType(typeof(IJobParallelForBatchExtensions.ParallelForBatchJobStruct<>))]
    public interface IJobParallelForBatch
    {
        void Execute(int startIndex, int count);
    }

    public static class IJobParallelForBatchExtensions
    {
        internal struct ParallelForBatchJobStruct<T> where T : struct, IJobParallelForBatch
        {
            static IntPtr JobReflectionData;

            public static unsafe IntPtr Initialize()
            {
                if (JobReflectionData == IntPtr.Zero)
                {
                    JobReflectionData = JobsUtility.CreateJobReflectionData(typeof(T),
#if UNITY_DOTSPLAYER
                        // TODO remove this #if
                        // https://unity3d.atlassian.net/browse/DOTSR-379
                        typeof(T),
#endif
                        JobType.ParallelFor, (ExecuteJobFunction)Execute);
                }
                return JobReflectionData;
            }

            public delegate void ExecuteJobFunction(ref T jobData, System.IntPtr additionalPtr, System.IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);
            public unsafe static void Execute(ref T jobData, System.IntPtr additionalPtr, System.IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                while (true)
                {
                    if (!JobsUtility.GetWorkStealingRange(
                        ref ranges,
                        jobIndex, out int begin, out int end))
                        return;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobData), begin, end - begin);
#endif

                    jobData.Execute(begin, end - begin);
                }
            }
        }

        public static unsafe JobHandle ScheduleBatch<T>(this T jobData, int arrayLength, int minIndicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatch
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobData),
                ParallelForBatchJobStruct<T>.Initialize(),
                dependsOn,
                ScheduleMode.Batched);

            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, minIndicesPerJobCount);
        }

        public static unsafe void RunBatch<T>(this T jobData, int arrayLength) where T : struct, IJobParallelForBatch
        {
            var scheduleParams =
                new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData),
                    ParallelForBatchJobStruct<T>.Initialize(), new JobHandle(), ScheduleMode.Run);
            JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, arrayLength);
        }
    }
}
