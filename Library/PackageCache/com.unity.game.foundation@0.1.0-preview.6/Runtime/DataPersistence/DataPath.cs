namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Enum to determine the save data path for player account data.
    /// </summary>
    public enum SaveDataPath
    {
        /// <summary>
        /// Application.dataPath
        /// </summary>
        DataPath,
        
        /// <summary>
        /// Application.persistentDataPath
        /// </summary>
        PersistentDataPath
    }
}