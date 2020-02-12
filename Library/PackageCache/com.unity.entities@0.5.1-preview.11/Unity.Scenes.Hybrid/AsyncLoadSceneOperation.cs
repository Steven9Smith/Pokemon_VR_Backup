﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;


namespace Unity.Scenes
{
    struct AsyncLoadSceneData
    {
        public EntityManager EntityManager;
        public int ExpectedObjectReferenceCount;
        public int SceneSize;
        public string ResourcesPathObjRefs;
        public string ScenePath;
        public bool UsingBundles;
        public bool BlockUntilFullyLoaded;
    }

    unsafe class AsyncLoadSceneOperation
    {
        public enum LoadingStatus
        {
            Completed,
            NotStarted,
            WaitingForAssetBundleLoad,
            WaitingForAssetLoad,
            WaitingForResourcesLoad,
            WaitingForEntitiesLoad,
            WaitingForSceneDeserialization
        }

        public override string ToString()
        {
            return $"AsyncLoadSceneJob({_ScenePath})";
        }

        unsafe struct FreeJob : IJob
        {
            [NativeDisableUnsafePtrRestriction]
            public void* ptr;
            public Allocator allocator;
            public void Execute()
            {
                UnsafeUtility.Free(ptr, allocator);
            }
        }

        public void Dispose()
        {
            if (_LoadingStatus == LoadingStatus.Completed)
            {
                new FreeJob { ptr = _FileContent, allocator = Allocator.Persistent }.Schedule();
            }
            else if (_LoadingStatus == LoadingStatus.WaitingForResourcesLoad || _LoadingStatus == LoadingStatus.WaitingForEntitiesLoad)
            {
                new FreeJob { ptr = _FileContent, allocator = Allocator.Persistent }.Schedule(_ReadHandle.JobHandle);
            }
            else if (_LoadingStatus == LoadingStatus.WaitingForSceneDeserialization)
            {
                _EntityManager.ExclusiveEntityTransactionDependency.Complete();
                new FreeJob { ptr = _FileContent, allocator = Allocator.Persistent }.Schedule();
            }
            if (_AssetBundle)
                _AssetBundle.Unload(true);
        }

        struct AsyncLoadSceneJob : IJob
        {
            public GCHandle                     LoadingOperationHandle;
            public GCHandle                     ObjectReferencesHandle;
            public ExclusiveEntityTransaction   Transaction;
            [NativeDisableUnsafePtrRestriction]
            public byte*                        FileContent;

            static readonly ProfilerMarker k_ProfileDeserializeWorld = new ProfilerMarker("AsyncLoadSceneJob.DeserializeWorld");

            public void Execute()
            {
                var loadingOperation = (AsyncLoadSceneOperation)LoadingOperationHandle.Target;
                LoadingOperationHandle.Free();

                var objectReferences = (UnityEngine.Object[]) ObjectReferencesHandle.Target;
                ObjectReferencesHandle.Free();

                try
                {
                    using (var reader = new MemoryBinaryReader(FileContent))
                    {
                        k_ProfileDeserializeWorld.Begin();
                        SerializeUtility.DeserializeWorld(Transaction, reader, objectReferences);
                        k_ProfileDeserializeWorld.End();
                    }
                }
                catch (Exception exc)
                {
                    loadingOperation._LoadingFailure = exc.ToString();
                }
            }
        }

        AsyncLoadSceneData      _Data;
        string                  _ScenePath => _Data.ScenePath;
        int                     _SceneSize => _Data.SceneSize;
        int                     _ExpectedObjectReferenceCount => _Data.ExpectedObjectReferenceCount;
        string                  _ResourcesPathObjRefs => _Data.ResourcesPathObjRefs;
        EntityManager           _EntityManager => _Data.EntityManager;
        bool                    _UsingBundles => _Data.UsingBundles;
        bool                    _BlockUntilFullyLoaded => _Data.BlockUntilFullyLoaded;

        ReferencedUnityObjects  _ResourceObjRefs;

        AssetBundleCreateRequest _AssetBundleRequest;
        AssetBundle             _AssetBundle;
        AssetBundleRequest      _AssetRequest;

        LoadingStatus           _LoadingStatus;
        string                  _LoadingFailure;

        byte*                    _FileContent;
        ReadHandle               _ReadHandle;

        private double _StartTime;

        public AsyncLoadSceneOperation(AsyncLoadSceneData asyncLoadSceneData, bool usingLiveLink)
        {
            _Data = asyncLoadSceneData;
            _LoadingStatus = LoadingStatus.NotStarted;

            if (usingLiveLink && _ExpectedObjectReferenceCount != 0)
            {
                // Resolving must happen on the main thread
                using(var data = new NativeArray<byte>(File.ReadAllBytes(_ResourcesPathObjRefs), Allocator.Temp))
                using (var reader = new MemoryBinaryReader((byte*)data.GetUnsafePtr()))
                {
                    var numObjRefGUIDs = reader.ReadInt();
                    NativeArray<RuntimeGlobalObjectId> objRefGUIDs = new NativeArray<RuntimeGlobalObjectId>(numObjRefGUIDs, Allocator.Temp);
                    reader.ReadArray(objRefGUIDs, numObjRefGUIDs);

                    var objs = new UnityEngine.Object[numObjRefGUIDs];
                    LiveLinkPlayerAssetRefreshSystem._GlobalAssetObjectResolver.ResolveObjects(objRefGUIDs, objs);
                    objRefGUIDs.Dispose();

                    _ResourceObjRefs = ScriptableObject.CreateInstance<ReferencedUnityObjects>();
                    _ResourceObjRefs.Array = objs;
                }
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _LoadingStatus == LoadingStatus.Completed;
            }
        }

        public string ErrorStatus
        {
            get
            {
                if (_LoadingStatus == LoadingStatus.Completed)
                    return _LoadingFailure;
                else
                    return null;
            }
        }

        public AssetBundle StealBundle()
        {
            var bundle = _AssetBundle;
            _AssetBundle = null;
            return bundle;
        }

        public void Update()
        {
            //@TODO: Try to overlap Resources load and entities scene load

            // Begin Async resource load
            if (_LoadingStatus == LoadingStatus.NotStarted)
            {
                if (_SceneSize == 0)
                    return;

                try
                {
                    _StartTime = Time.realtimeSinceStartup;

                    _FileContent = (byte*)UnsafeUtility.Malloc(_SceneSize, 16, Allocator.Persistent);

                    ReadCommand cmd;
                    cmd.Buffer = _FileContent;
                    cmd.Offset = 0;
                    cmd.Size = _SceneSize;
                    _ReadHandle = AsyncReadManager.Read(_ScenePath, &cmd, 1);

                    if (_ExpectedObjectReferenceCount != 0)
                    {
#if UNITY_EDITOR
                        if (!_UsingBundles)
                        {
                            var resourceRequests = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(_ResourcesPathObjRefs);
                            _ResourceObjRefs = (ReferencedUnityObjects)resourceRequests[0];

                            _LoadingStatus = LoadingStatus.WaitingForResourcesLoad;
                        }
                        else
#endif
                        if (_ResourceObjRefs != null)
                        {
                            _LoadingStatus = LoadingStatus.WaitingForResourcesLoad;
                        }
                        else
                        {
                            if (!_BlockUntilFullyLoaded)
                            {
                                _AssetBundleRequest = AssetBundle.LoadFromFileAsync(_ResourcesPathObjRefs);
                            }
                            else
                            {
                                _AssetBundle = AssetBundle.LoadFromFile(_ResourcesPathObjRefs);
                            }
                            _LoadingStatus = LoadingStatus.WaitingForAssetBundleLoad;
                        }
                    }
                    else
                    {
                        _LoadingStatus = LoadingStatus.WaitingForEntitiesLoad;
                    }
                }
                catch (Exception e)
                {
                    _LoadingFailure = e.Message;
                    _LoadingStatus = LoadingStatus.Completed;
                }
            }

            // Once async asset bundle load is done, we can read the asset
            if (_LoadingStatus == LoadingStatus.WaitingForAssetBundleLoad)
            {
                if (!_BlockUntilFullyLoaded)
                {
                    if (!_AssetBundleRequest.isDone)
                        return;

                    if (!_AssetBundleRequest.assetBundle)
                    {
                        _LoadingFailure = $"Failed to load Asset Bundle '{_ResourcesPathObjRefs}'";
                        _LoadingStatus = LoadingStatus.Completed;
                        return;
                    }

                    _AssetBundle = _AssetBundleRequest.assetBundle;

                    _AssetRequest = _AssetBundle.LoadAssetAsync(Path.GetFileName(_ResourcesPathObjRefs));
                    _LoadingStatus = LoadingStatus.WaitingForAssetLoad;
                }
                else
                {
                    _ResourceObjRefs = _AssetBundle.LoadAsset<ReferencedUnityObjects>(Path.GetFileName(_ResourcesPathObjRefs));
                    _LoadingStatus = LoadingStatus.WaitingForEntitiesLoad;
                }
            }

            // Once async asset bundle load is done, we can read the asset
            if (_LoadingStatus == LoadingStatus.WaitingForAssetLoad)
            {
                if (!_AssetRequest.isDone)
                    return;

                if (!_AssetRequest.asset)
                {
                    _LoadingFailure = $"Failed to load Asset '{Path.GetFileName(_ResourcesPathObjRefs)}'";
                    _LoadingStatus = LoadingStatus.Completed;
                    return;
                }

                _ResourceObjRefs = _AssetRequest.asset as ReferencedUnityObjects;

                if (_ResourceObjRefs == null)
                {
                    _LoadingFailure = $"Failed to load object references resource '{_ResourcesPathObjRefs}'";
                    _LoadingStatus = LoadingStatus.Completed;
                    return;
                }
                _LoadingStatus = LoadingStatus.WaitingForEntitiesLoad;
            }

            // Once async resource load is done, we can async read the entity scene data
            if (_LoadingStatus == LoadingStatus.WaitingForResourcesLoad)
            {
                if (_ResourceObjRefs == null)
                {
                    _LoadingFailure = $"Failed to load object references resource '{_ResourcesPathObjRefs}'";
                    _LoadingStatus = LoadingStatus.Completed;
                    return;
                }

                _LoadingStatus = LoadingStatus.WaitingForEntitiesLoad;
            }

            if (_LoadingStatus == LoadingStatus.WaitingForEntitiesLoad)
            {
                try
                {
                    _LoadingStatus = LoadingStatus.WaitingForSceneDeserialization;
                    ScheduleSceneRead(_ResourceObjRefs);

                    if (_BlockUntilFullyLoaded)
                    {
                        _EntityManager.ExclusiveEntityTransactionDependency.Complete();
                    }
                }
                catch (Exception e)
                {
                    _LoadingFailure = e.Message;
                    _LoadingStatus = LoadingStatus.Completed;
                }
            }

            // Complete Loading status
            if (_LoadingStatus == LoadingStatus.WaitingForSceneDeserialization)
            {
                if (_EntityManager.ExclusiveEntityTransactionDependency.IsCompleted)
                {
                    _EntityManager.ExclusiveEntityTransactionDependency.Complete();

                    _LoadingStatus = LoadingStatus.Completed;
                    var currentTime = Time.realtimeSinceStartup;
                    var totalTime = currentTime - _StartTime;
                    System.Console.WriteLine($"Streamed scene with {totalTime * 1000,3:f0}ms latency from {_ScenePath}");
                }
            }
        }

        void ScheduleSceneRead(ReferencedUnityObjects objRefs)
        {
            var transaction = _EntityManager.BeginExclusiveEntityTransaction();
            SerializeUtilityHybrid.DeserializeObjectReferences(_EntityManager, objRefs, _ScenePath, out var objectReferences);

            var loadJob = new AsyncLoadSceneJob
            {
                Transaction = transaction,
                LoadingOperationHandle = GCHandle.Alloc(this),
                ObjectReferencesHandle = GCHandle.Alloc(objectReferences),
                FileContent = _FileContent
            };

            _EntityManager.ExclusiveEntityTransactionDependency = loadJob.Schedule(JobHandle.CombineDependencies(_EntityManager.ExclusiveEntityTransactionDependency, _ReadHandle.JobHandle));
        }
    }

}
