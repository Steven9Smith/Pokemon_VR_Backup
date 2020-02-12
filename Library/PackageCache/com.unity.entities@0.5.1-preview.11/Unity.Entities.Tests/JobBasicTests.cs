using System;
using System.Threading;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Entities.Tests
{
    // These are very basic tests. As we bring up the Tiny system,
    // it's useful to have super simple tests to make sure basics
    // are working.
    public class JobBasicTests : ECSTestsFixture
    {
        private const int NMULT = 1;

        // TODO calling nUnit Assert on a job thread may be causing errors.
        // Until sorted out, pull a simple exception out for use by the worker threads.
        static void AssertOnThread(bool test)
        {
            if (!test) throw new Exception("AssertOnThread Failed.");
        }

        public struct SimpleJob : IJob
        {
            public const int N = 1000 * NMULT;

            public int a;
            public int b;

            [WriteOnly]
            public NativeArray<int> result;

            public void Execute()
            {
                for (int i = 0; i < N; ++i)
                    result[i] = a + b;

#if UNITY_DOTSPLAYER    // TODO: Don't have the library in the editor that grants access.
                AssertOnThread(result.m_Safety.IsAllowedToWrite());
                AssertOnThread(!result.m_Safety.IsAllowedToRead());
#endif
            }
        }

        [Test]
        public void RunSimpleJob()
        {
            SimpleJob job = new SimpleJob()
            {
                a = 5,
                b = 10
            };
            NativeArray<int> jobResult = new NativeArray<int>(SimpleJob.N, Allocator.TempJob);
            job.result = jobResult;

            job.Run();

            for (int i = 0; i < SimpleJob.N; ++i)
            {
                Assert.AreEqual(15, jobResult[i]);
            }

            jobResult.Dispose();
        }

        [Test]
        public void ScheduleSimpleJob()
        {
            SimpleJob job = new SimpleJob()
            {
                a = 5,
                b = 10
            };

            NativeArray<int> jobResult = new NativeArray<int>(SimpleJob.N, Allocator.TempJob);
            job.result = jobResult;

            JobHandle handle = job.Schedule();
            handle.Complete();

            for (int i = 0; i < SimpleJob.N; ++i)
            {
                Assert.AreEqual(15, jobResult[i]);
            }

            jobResult.Dispose();
        }

        public struct SimpleAddSerial : IJob
        {
            public const int N = 1000 * NMULT;

            public int a;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<int> input;

            [WriteOnly]
            public NativeArray<int> result;

            public void Execute()
            {
#if UNITY_DOTSPLAYER    // Don't have the C# version in the editor.
                AssertOnThread(!input.m_Safety.IsAllowedToWrite());
                AssertOnThread(input.m_Safety.IsAllowedToRead());
                AssertOnThread(result.m_Safety.IsAllowedToWrite());
                AssertOnThread(!result.m_Safety.IsAllowedToRead());

#if UNITY_SINGLETHREADED_JOBS
                AssertOnThread(JobsUtility.InJob);
#endif
#endif
                for (int i = 0; i < N; ++i)
                    result[i] = a + input[i];
            }
        }

        [Test]
        public void Run3SimpleJobsInSerial()
        {
            NativeArray<int> input = new NativeArray<int>(SimpleAddSerial.N, Allocator.TempJob);
            NativeArray<int> jobResult1 = new NativeArray<int>(SimpleAddSerial.N, Allocator.TempJob);
            NativeArray<int> jobResult2 = new NativeArray<int>(SimpleAddSerial.N, Allocator.TempJob);
            NativeArray<int> jobResult3 = new NativeArray<int>(SimpleAddSerial.N, Allocator.TempJob);

            for (int i = 0; i < SimpleAddSerial.N; ++i)
            {
                input[i] = i;
            }

            SimpleAddSerial job1 = new SimpleAddSerial() {a = 1, input = input, result = jobResult1};
            SimpleAddSerial job2 = new SimpleAddSerial() {a = 2, input = jobResult1, result = jobResult2};
            SimpleAddSerial job3 = new SimpleAddSerial() {a = 3, input = jobResult2, result = jobResult3};

#if UNITY_SINGLETHREADED_JOBS && UNITY_DOTSPLAYER
            Assert.IsFalse(JobsUtility.InJob);
#endif

            JobHandle handle1 = job1.Schedule();
            JobHandle handle2 = job2.Schedule(handle1);
            JobHandle handle3 = job3.Schedule(handle2);

            handle3.Complete();

#if UNITY_SINGLETHREADED_JOBS && UNITY_DOTSPLAYER
            Assert.IsFalse(JobsUtility.InJob);
#endif

            for (int i = 0; i < SimpleAddSerial.N; ++i)
            {
                Assert.AreEqual(i + 1 + 2 + 3, jobResult3[i]);
            }

            jobResult3.Dispose();
        }

        public struct SimpleAddParallel : IJob
        {
            public const int N = 1000 * NMULT;

            public int a;

            [ReadOnly]
            public NativeArray<int> input;

            [WriteOnly]
            public NativeArray<int> result;

            public void Execute()
            {
                for (int i = 0; i < N; ++i)
                    result[i] = a + input[i];
            }
        }

        [Test]
        public void Run3SimpleJobsInParallel()
        {
            NativeArray<int> input = new NativeArray<int>(SimpleAddParallel.N, Allocator.TempJob);
            NativeArray<int> jobResult1 = new NativeArray<int>(SimpleAddParallel.N, Allocator.TempJob);
            NativeArray<int> jobResult2 = new NativeArray<int>(SimpleAddParallel.N, Allocator.TempJob);
            NativeArray<int> jobResult3 = new NativeArray<int>(SimpleAddParallel.N, Allocator.TempJob);

            for (int i = 0; i < SimpleAddParallel.N; ++i)
            {
                input[i] = i;
            }

            SimpleAddParallel job1 = new SimpleAddParallel() {a = 1, input = input, result = jobResult1};
            SimpleAddParallel job2 = new SimpleAddParallel() {a = 2, input = input, result = jobResult2};
            SimpleAddParallel job3 = new SimpleAddParallel() {a = 3, input = input, result = jobResult3};

            JobHandle handle1 = job1.Schedule();
            JobHandle handle2 = job2.Schedule();
            JobHandle handle3 = job3.Schedule();

            JobHandle[] arr = {handle1, handle2, handle3};
            NativeArray<JobHandle> group = new NativeArray<JobHandle>(arr, Allocator.TempJob);
            JobHandle handle = JobHandle.CombineDependencies(group);

            handle.Complete();

            for (int i = 0; i < SimpleAddParallel.N; ++i)
            {
                Assert.AreEqual(i + 1, jobResult1[i]);
                Assert.AreEqual(i + 2, jobResult2[i]);
                Assert.AreEqual(i + 3, jobResult3[i]);
            }

            input.Dispose();
            jobResult1.Dispose();
            jobResult2.Dispose();
            jobResult3.Dispose();
            group.Dispose();
        }

        public struct SimpleListAdd : IJob
        {
            public const int N = 10000 * NMULT;

            public int a;

            [ReadOnly] public NativeList<int> input;
            [WriteOnly] public NativeList<int> result;

            public void Execute()
            {
#if UNITY_DOTSPLAYER    // Don't have the C# version in the editor.
                AssertOnThread(!input.m_Safety.IsAllowedToWrite());
                AssertOnThread(input.m_Safety.IsAllowedToRead());
                AssertOnThread(result.m_Safety.IsAllowedToWrite());
                AssertOnThread(!result.m_Safety.IsAllowedToRead());
#endif
                for (int i = 0; i < N; ++i)
                    result.Add(a + input[i]);
            }
        }

        [Test]
        public void Run3SimpleListJobsInParallel()
        {
            NativeList<int> input = new NativeList<int>(Allocator.TempJob);
            NativeList<int> jobResult1 = new NativeList<int>(Allocator.TempJob);
            NativeList<int> jobResult2 = new NativeList<int>(Allocator.TempJob);
            NativeList<int> jobResult3 = new NativeList<int>(Allocator.TempJob);

            for (int i = 0; i < SimpleListAdd.N; ++i)
            {
                input.Add(i);
            }

            SimpleListAdd job1 = new SimpleListAdd() {a = 11, input = input, result = jobResult1};
            SimpleListAdd job2 = new SimpleListAdd() {a = 22, input = input, result = jobResult2};
            SimpleListAdd job3 = new SimpleListAdd() {a = 33, input = input, result = jobResult3};

            JobHandle handle1 = job1.Schedule();
            JobHandle handle2 = job2.Schedule();
            JobHandle handle3 = job3.Schedule();

            JobHandle[] arr = {handle1, handle2, handle3};
            NativeArray<JobHandle> group = new NativeArray<JobHandle>(arr, Allocator.TempJob);

            JobHandle handle = JobHandle.CombineDependencies(group);
            input.Dispose(handle);

            handle.Complete();

            for (int i = 0; i < SimpleListAdd.N; ++i)
            {
                Assert.AreEqual(i + 11, jobResult1[i]);
                Assert.AreEqual(i + 22, jobResult2[i]);
                Assert.AreEqual(i + 33, jobResult3[i]);
            }

            jobResult1.Dispose();
            jobResult2.Dispose();
            jobResult3.Dispose();
            group.Dispose();
        }



        public struct SimpleParallelFor : IJobParallelFor
        {
            public const int N = 10000 * NMULT;
            
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<int> a;

            [ReadOnly]
            public NativeArray<int> b;

            [WriteOnly]
            public NativeArray<int> result;

            public void Execute(int i)
            {
#if UNITY_DOTSPLAYER    // Don't have the C# version in the editor.
                AssertOnThread(!a.m_Safety.IsAllowedToWrite());
                AssertOnThread(a.m_Safety.IsAllowedToRead());
                AssertOnThread(!b.m_Safety.IsAllowedToWrite());
                AssertOnThread(b.m_Safety.IsAllowedToRead());
                AssertOnThread(result.m_Safety.IsAllowedToWrite());
                AssertOnThread(!result.m_Safety.IsAllowedToRead());
#endif
                result[i] = a[i] + b[i];
            }
        }

        [Test]
        public void ScheduleSimpleParallelFor()
        {
            NativeArray<int> a = new NativeArray<int>(SimpleParallelFor.N, Allocator.TempJob);
            NativeArray<int> b = new NativeArray<int>(SimpleParallelFor.N, Allocator.TempJob);
            NativeArray<int> result = new NativeArray<int>(SimpleParallelFor.N, Allocator.TempJob);

            for (int i = 0; i < SimpleParallelFor.N; ++i)
            {
                a[i] = 100 + i;
                b[i] = 200 + i;
            }

            SimpleParallelFor job = new SimpleParallelFor() {a = a, b = b, result = result};
            job.a = a;
            job.b = b;
            job.result = result;

            JobHandle handle = job.Schedule(result.Length, 300);
            handle.Complete();

            for (int i = 0; i < SimpleParallelFor.N; ++i)
            {
                Assert.AreEqual(300 + i * 2, result[i]);
            }

            b.Dispose();
            result.Dispose();
        }

        public struct HashWriterParallelFor : IJobParallelFor
        {
            [WriteOnly]
            public NativeHashMap<int, int>.ParallelWriter result;

            [WriteOnly]
            public NativeHashMap<int, bool>.ParallelWriter threadMap;

            public static int nCalls;

            public unsafe void Execute(int i)
            {
                result.TryAdd(i, 17);
                threadMap.TryAdd(threadMap.m_ThreadIndex, true);
                nCalls++;

#if UNITY_DOTSPLAYER    // Don't have the library in the editor that grants access.
                AssertOnThread(result.m_Safety.IsAllowedToWrite());
                AssertOnThread(!result.m_Safety.IsAllowedToRead());
                AssertOnThread(threadMap.m_Safety.IsAllowedToWrite());
                AssertOnThread(!threadMap.m_Safety.IsAllowedToRead());
#endif
            }
        }

        [Test]
        public void RunHashWriterParallelFor()
        {
            const int MAPSIZE = 100;
            // Make sure that each iteration was called and the parallel write worked.
            NativeHashMap<int, int> map = new NativeHashMap<int, int>(MAPSIZE, Allocator.TempJob);
            // Tracks the threadIndex used for each job.
            NativeHashMap<int, bool> threadMap = new NativeHashMap<int, bool>(JobsUtility.MaxJobThreadCount, Allocator.TempJob);

            HashWriterParallelFor job = new HashWriterParallelFor()
            {
                result = map.AsParallelWriter(),
                threadMap = threadMap.AsParallelWriter()
            };

            JobHandle handle = job.Schedule(MAPSIZE, 10);
            handle.Complete();

            for (int i = 0; i < MAPSIZE; ++i)
            {
                Assert.AreEqual(17, map[i]);
            }

            var workerCount = JobsUtility.JobWorkerCount;

#if !UNITY_SINGLETHREADED_JOBS
            if (workerCount > 1)
            {
                Assert.IsTrue(threadMap.Length > 1);              // should have run in parallel, and used different thread indices
            }
            else
            {
                Assert.That(threadMap.Length == 1);
            }
#else
            Assert.IsTrue(threadMap.Length == 1);    // only have one thread.
            Assert.IsTrue(threadMap[0] == true);     // and it should be job index 0
#endif

            map.Dispose();
            threadMap.Dispose();
        }

        public struct SimpleParallelForDefer : IJobParallelForDefer
        {
            public const int N = 10000 * NMULT;

            [ReadOnly] public NativeList<int> a;
            [ReadOnly] public NativeArray<int> b;

            [WriteOnly] public NativeArray<int> result;

            public void Execute(int i)
            {
                result[i] = a[i] + b[i];
            }
        }

        [Test]
        public void ScheduleSimpleParallelForDefer_1()
        {
            NativeList<int> a = new NativeList<int>(SimpleParallelForDefer.N, Allocator.TempJob);
            NativeArray<int> b = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);
            NativeArray<int> result = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);

            for (int i = 0; i < SimpleParallelForDefer.N; ++i)
            {
                a.Add(100 + i);
                b[i] = 200 + i;
            }

            SimpleParallelForDefer job = new SimpleParallelForDefer() {a = a, b = b, result = result};
            job.a = a;
            job.b = b;
            job.result = result;

            JobHandle handle = job.Schedule(a, 300);
            handle.Complete();

            for (int i = 0; i < SimpleParallelFor.N; ++i)
            {
                Assert.AreEqual(300 + i * 2, result[i]);
            }

            a.Dispose();
            b.Dispose();
            result.Dispose();
        }

        [Test]
        public unsafe void ScheduleSimpleParallelForDefer_2()
        {
            NativeList<int> a = new NativeList<int>(SimpleParallelForDefer.N, Allocator.TempJob);
            NativeArray<int> b = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);
            NativeArray<int> result = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);

            for (int i = 0; i < SimpleParallelForDefer.N; ++i)
            {
                a.Add(100 + i);
                b[i] = 200 + i;
            }

            SimpleParallelForDefer job = new SimpleParallelForDefer() {a = a, b = b, result = result};
            job.a = a;
            job.b = b;
            job.result = result;

            var lengthValue = new NativeArray<int>(1, Allocator.TempJob);
            lengthValue[0] = SimpleParallelForDefer.N;

            JobHandle handle = job.Schedule((int*) lengthValue.GetUnsafePtr(), 300);
            handle.Complete();

            for (int i = 0; i < SimpleParallelFor.N; ++i)
            {
                Assert.AreEqual(300 + i * 2, result[i]);
            }

            a.Dispose();
            b.Dispose();
            result.Dispose();
            lengthValue.Dispose();
        }

        public struct SimpleParallelForBatch : IJobParallelForBatch
        {
            public const int N = 10000 * NMULT;

            [ReadOnly] public NativeArray<int> a;
            [ReadOnly] public NativeArray<int> b;

            [WriteOnly] public NativeArray<int> result;

            public void Execute(int index, int count)
            {
                for (int i = 0; i < count; ++i, ++index)
                {
                    result[index] = a[index] + b[index];
                }
            }
        }

        [Test]
        public void ScheduleSimpleParallelForBatch()
        {
            NativeArray<int> a = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);
            NativeArray<int> b = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);
            NativeArray<int> result = new NativeArray<int>(SimpleParallelForDefer.N, Allocator.TempJob);

            for (int i = 0; i < SimpleParallelForBatch.N; ++i)
            {
                a[i] = 100 + i;
                b[i] = 200 + i;
            }

            SimpleParallelForBatch job = new SimpleParallelForBatch() {a = a, b = b, result = result};
            job.a = a;
            job.b = b;
            job.result = result;

            JobHandle handle = job.ScheduleBatch(SimpleParallelForBatch.N, 20);
            handle.Complete();

            for (int i = 0; i < SimpleParallelFor.N; ++i)
            {
                Assert.AreEqual(300 + i * 2, result[i]);
            }

            a.Dispose();
            b.Dispose();
            result.Dispose();
        }

        public struct HashWriterJob : IJob
        {
            public const int N = 1000 * NMULT;
            // Don't declare [WriteOnly]. Write only is "automatic" for the ParallelWriter
            public NativeHashMap<int, int>.ParallelWriter result;

            public void Execute()
            {
#if UNITY_DOTSPLAYER    // Don't have the C# version in the editor.
                Assert.IsTrue(result.m_Safety.IsAllowedToWrite());
                Assert.IsTrue(!result.m_Safety.IsAllowedToRead());
#endif
                for (int i = 0; i < N; ++i)
                {
                    result.TryAdd(i, 47);
                }
            }
        }

        [Test]
        public void RunHashWriterJob()
        {
            NativeHashMap<int, int> map = new NativeHashMap<int, int>(HashWriterJob.N, Allocator.TempJob);

            HashWriterJob job = new HashWriterJob();
            job.result = map.AsParallelWriter();
            JobHandle handle = job.Schedule();
            handle.Complete();

            for (int i = 0; i < HashWriterJob.N; ++i)
            {
                Assert.AreEqual(map[i], 47);
            }
            map.Dispose();
        }

        public struct SimpleChunk<T> : IJobChunk
            where T : struct, IEquatable<T>
        {
            public ArchetypeChunkComponentType<EcsTestData> testType;

            [ReadOnly]
            public NativeList<T> listOfT;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<EcsTestData> chunkData = chunk.GetNativeArray(testType);

                for (int i = 0; i < chunk.Count; ++i)
                {
                    chunkData[i] = new EcsTestData() {value = 100 + chunkData[i].value};
                }
            }
        }

        [Test]
        public void ScheduleSimpleIJobChunk()
        {
            const int N = 10000 * NMULT;
            NativeArray<Entity> eArr = new NativeArray<Entity>(N, Allocator.TempJob);
            var arch = m_Manager.CreateArchetype(typeof(EcsTestData));

            m_Manager.CreateEntity(arch, eArr);

            for (int i = 0; i < N; ++i)
            {
                m_Manager.SetComponentData(eArr[i], new EcsTestData() {value = 10 + i});
            }

            NativeList<int> listOfInt = new NativeList<int>(1, Allocator.TempJob);

            EntityQuery query = EmptySystem.GetEntityQuery(typeof(EcsTestData));
            var job = new SimpleChunk<int>
            {
                testType = m_Manager.GetArchetypeChunkComponentType<EcsTestData>(false),
                listOfT = listOfInt
            };
            job.Schedule(query).Complete();

            for (int i = 0; i < N; ++i)
            {
                EcsTestData data = m_Manager.GetComponentData<EcsTestData>(eArr[i]);
                Assert.AreEqual(10 + i + 100, data.value);
            }

            listOfInt.Dispose();
            eArr.Dispose();
        }

        [Test]
        public void RunSimpleIJobChunk()
        {
            const int N = 10000 * NMULT;
            NativeArray<Entity> eArr = new NativeArray<Entity>(N, Allocator.TempJob);
            var arch = m_Manager.CreateArchetype(typeof(EcsTestData));

            m_Manager.CreateEntity(arch, eArr);

            for (int i = 0; i < N; ++i)
            {
                m_Manager.SetComponentData(eArr[i], new EcsTestData() {value = 10 + i});
            }

            NativeList<int> listOfInt = new NativeList<int>(1, Allocator.TempJob);

            EntityQuery query = EmptySystem.GetEntityQuery(typeof(EcsTestData));
            var job = new SimpleChunk<int>
            {
                testType = m_Manager.GetArchetypeChunkComponentType<EcsTestData>(false),
                listOfT = listOfInt
            };
            job.Run(query);

            for (int i = 0; i < N; ++i)
            {
                EcsTestData data = m_Manager.GetComponentData<EcsTestData>(eArr[i]);
                Assert.AreEqual(10 + i + 100, data.value);
            }

            listOfInt.Dispose();
            eArr.Dispose();
        }

    }
}
