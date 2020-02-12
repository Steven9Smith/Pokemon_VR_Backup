namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Enum for each supported notification type.
    /// </summary>
    internal enum NotificationType
    {
        /// <summary>
        /// Whenever a game item is first instantiated.
        /// </summary>
        Created,
        
        /// <summary>
        /// Whenever a game item is removed from the program.
        /// </summary>
        Destroyed,
        
        /// <summary>
        /// Whenever a game item is modified in anyway.
        /// </summary>
        Modified
    }
}
