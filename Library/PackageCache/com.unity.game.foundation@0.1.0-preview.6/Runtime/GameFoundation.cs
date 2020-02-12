using System;
using UnityEngine.GameFoundation.DataPersistence;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manage the initialization and the persistence of the Game Foundation systems.
    /// </summary>
    public static class GameFoundation
    {
        private enum InitializationStatus
        {
            NotInitialized,
            Initializing,
            Initialized,
            Failed
        }
        
        internal const string k_GameFoundationPersistenceId = "gamefoundation_persistence";

        private static InitializationStatus m_InitializationStatus = InitializationStatus.NotInitialized;

        /// <summary>
        /// Check if the Game Foundation is initialized.
        /// </summary>
        /// <returns>Whether the Game Foundation is initialized or not</returns>
        public static bool IsInitialized
        {
            get { return m_InitializationStatus == InitializationStatus.Initialized; }
        }
        
        /// <summary>
        /// Initialize the GameFoundation . It need a persistence object to be passed as argument to set the default persistence layer
        /// If the initialization fails, onInitializeFailed will be called with an exception.
        /// </summary>
        /// <param name="dataPersistence">The persistence layer of the Game Foundation. Required and cached for future execution</param>
        /// <param name="onInitializeCompleted">Called when the initialization process is completed with success</param>
        /// <param name="onInitializeFailed">Called when the initialization process failed</param>
        public static void Initialize(
            IDataPersistence dataPersistence = null, 
            Action onInitializeCompleted = null, 
            Action onInitializeFailed = null)
        {
            if (m_InitializationStatus == InitializationStatus.Initializing ||
                m_InitializationStatus == InitializationStatus.Initialized)
            {
                Debug.LogWarning("GameFoundation is already initialized and cannot be initialized again.");
                onInitializeFailed?.Invoke();
                
                return;
            }

            m_InitializationStatus = InitializationStatus.Initializing;
            
            if (dataPersistence != null)
            {
                LoadData(dataPersistence,
                    (data) =>
                    {
                        InitializeSystems(data, onInitializeCompleted, onInitializeFailed);
                    },
                    () =>
                    {
                        InitializeSystems(null, onInitializeCompleted, onInitializeFailed);
                    });
            }
            else
            {
                InitializeSystems(null, onInitializeCompleted, onInitializeFailed);
            }
        }

        static void InitializeSystems(ISerializableData data, Action onInitializeCompleted = null, Action onInitializeFailed = null)
        {
            bool isInitialized = true;
            
            try
            {
                NotificationSystem.temporaryDisable = true;
                isInitialized = GameItemLookup.Initialize(data) &&
                                StatManager.Initialize(data) &&
                                InventoryManager.Initialize(data);

                if (isInitialized)
                {
                    AnalyticsWrapper.Initialize();
                }

                NotificationSystem.temporaryDisable = false;
            }
            catch
            {
                isInitialized = false;
            }

            if (isInitialized)
            {
                m_InitializationStatus = InitializationStatus.Initialized;
                onInitializeCompleted?.Invoke();
            }
            else
            {
                Uninitialize();
                
                Debug.LogWarning("GameFoundation can't be initialized.");
                
                m_InitializationStatus = InitializationStatus.Failed;
                onInitializeFailed?.Invoke();
            }
        }
        
        /// <summary>
        /// Asynchronously loads data from the persistence layer and populates managed systems with it
        /// </summary>
        /// <param name="dataPersistence">The persistence layer used to execute the process</param>
        /// <param name="onLoadCompleted">Called when the loading process is completed with success</param>
        /// <param name="onLoadFailed">Called when the loading process failed</param>
        public static void Load(
            IDataPersistence dataPersistence, 
            Action onLoadCompleted = null, 
            Action onLoadFailed = null)
        {
            if (dataPersistence == null)
            {
                onLoadFailed?.Invoke();
                Debug.LogWarning("DataPersistence cannot be null on persistence process.");
                return;
            }

            LoadData(dataPersistence,
                (data) =>
                {
                    try
                    {
                        FillAllSystems(data);
                        onLoadCompleted?.Invoke();
                    }
                    catch
                    {
                        onLoadFailed?.Invoke();
                    }
                    
                }, onLoadFailed);
        }
        
        /// <summary>
        /// Asynchronously saves data through the persistence layer.
        /// </summary>
        /// <param name="dataPersistence">The persistence layer used to execute the process</param>
        /// <param name="onSaveCompleted">Called when the saving process is completed with success</param>
        /// <param name="onSaveFailed">Called when the saving process failed</param>
        public static void Save(
            IDataPersistence dataPersistence, 
            Action onSaveCompleted = null, 
            Action onSaveFailed = null)
        {
            if (dataPersistence == null)
            {
                onSaveFailed?.Invoke();
                Debug.LogWarning("DataPersistence cannot be null on persistence process.");
                return;
            }
            
            if (!InventoryManager.IsInitialized)
            {
                onSaveFailed?.Invoke();
                Debug.LogWarning("Cannot save GameFoundation. InventoryManager is not initialized.");
                return;
            }
            
            if (!StatManager.IsInitialized)
            {
                onSaveFailed?.Invoke();
                Debug.LogWarning("Cannot save GameFoundation. StatManager is not initialized.");
                return;
            }

            StatManagerSerializableData statManagerData = StatManager.GetSerializableData();
            InventoryManagerSerializableData inventoryManagerData = InventoryManager.GetSerializableData();
            GameItemLookupSerializableData lookupData = GameItemLookup.GetSerializableData();
            
            GameFoundationSerializableData gameFoundationData = new GameFoundationSerializableData(
                BaseDataPersistence.k_SaveVersion, 
                statManagerData,
                inventoryManagerData,
                lookupData);
            
            dataPersistence.Save(k_GameFoundationPersistenceId, gameFoundationData, onSaveCompleted, onSaveFailed);
        }

        internal static void Uninitialize()
        {
            m_InitializationStatus = InitializationStatus.NotInitialized;
            
            GameItemLookup.Unintialize();
            InventoryManager.Uninitialize();
            StatManager.Uninitialize();
            AnalyticsWrapper.Uninitialize();
        }

        private static void LoadData
        (
            IDataPersistence dataPersistence,
            Action<ISerializableData> onLoadCompleted = null,
            Action onLoadFailed = null)
        {
            if (dataPersistence == null)
            {
                onLoadFailed?.Invoke();
                Debug.LogWarning("DataPersistence cannot be null on persistence process.");
            }
            
            dataPersistence.Load<GameFoundationSerializableData>(k_GameFoundationPersistenceId,
                (data) =>
                {
                    try
                    {
                        onLoadCompleted?.Invoke(data);
                    }
                    catch
                    {
                        onLoadFailed?.Invoke();
                    }
                },
                () => { onLoadFailed?.Invoke(); });
        }

        private static void FillAllSystems(ISerializableData data)
        {
            NotificationSystem.temporaryDisable = true;
            GameItemLookup.FillFromLookupData(data);
            StatManager.FillFromStatsData(data);
            InventoryManager.FillFromInventoriesData(data);
            NotificationSystem.temporaryDisable = false;
        }
    }
}