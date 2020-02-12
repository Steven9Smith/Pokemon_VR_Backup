using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Allow looking up GameItems at runtime.
    /// </summary>
    internal static class GameItemLookup
    {
        private static Dictionary<int, GameItem> m_Instances = new Dictionary<int, GameItem>();

        private static int m_LastGuidUsed = 0;
        private static bool m_IsInitialized = false;

        /// <summary>
        /// Returns the current initialization state of GameItemLookup.
        /// </summary>
        public static bool IsInitialized
        {
            get { return m_IsInitialized; }
        }
        
        internal static bool Initialize(ISerializableData data = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("GameItemLookup is already initialized and cannot be initialized again.");
                return false;
            }
            
            m_IsInitialized = true;
            
            if (data != null)
            {
                m_IsInitialized = FillFromLookupData(data);
            }
            
            return m_IsInitialized;
        }

        internal static void Unintialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            Reset();

            m_IsInitialized = false;
        }
        
        internal static void Reset()
        {
            m_Instances = new Dictionary<int, GameItem>();
            m_LastGuidUsed = 0;
        }

        internal static bool FillFromLookupData(ISerializableData data)
        {
            Reset();

            if (data == null)
                return false;
            
            var lookupData = (GameFoundationSerializableData) data;
            if (lookupData.gameItemLookupData == null)
            {
                Debug.LogWarning("Persistence Data data doesn't contain Game Item Lookup.");
                return false;
            }
                
            m_LastGuidUsed = lookupData.gameItemLookupData.lastGuidUsed;

            return true;
        }

        internal static GameItemLookupSerializableData GetSerializableData()
        {
            GameItemLookupSerializableData data = new GameItemLookupSerializableData(m_LastGuidUsed);
            return data;
        }
        
        /// <summary>
        /// Registers a specific Hash for specified GameItem so it can be looked up later.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's  Hash  to unregister with GameItemLookup.
        /// <param name="gameItem">The GameItem to register with GameItemLookup.
        /// <returns>True if GameItem was properly registered ( Hash must not already be registered).</returns>
        /// <exception cref="ArgumentException">Thrown if the given parameters are duplicates.</exception>
        public static bool RegisterInstance(int gameItemIdHash, GameItem gameItem)
        {
            if (gameItem == null)
            {
                return false;
            }

            if (m_Instances.ContainsKey(gameItemIdHash))
            {
                throw new ArgumentException("Cannot register an instance with a duplicate item hash.");
            }
            
            m_Instances[gameItemIdHash] = gameItem;
            return true;
        }

        /// <summary>
        /// Unregisters a specific Hash from GameItemLookup.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's  Hash  to unregister.
        /// <returns>True if GameItem was properly unregistered ( Hash must be registered).</returns>
        public static bool UnregisterInstance(int gameItemIdHash)
        {
            if (!m_Instances.ContainsKey(gameItemIdHash))
            {
                return false;
            }
            
            return m_Instances.Remove(gameItemIdHash);
        }

        /// <summary>
        /// Looks up GameItem for specified Hash.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's Hash to look up.
        /// <returns>GameItem previously registered with specified Hash.</returns>
        public static GameItem GetInstance(int gameItemIdHash)
        {
            if (!m_Instances.ContainsKey(gameItemIdHash))
            {
                return null;
            }
            
            return m_Instances[gameItemIdHash];
        }

        /// <summary>
        /// Returns next Hash to assign to a GameItem and updates internal counter so all Hash es assigned are unique.
        /// </summary>
        /// <returns>Hash to assign to newly created GameItem.</returns>
        public static int GetNextIdForInstance()
        {
            ++m_LastGuidUsed;
            return m_LastGuidUsed;
        }
    }
}
