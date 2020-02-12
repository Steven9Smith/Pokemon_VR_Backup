using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Will be the main hub for all data input throughout the system
    /// </summary>
    internal static class NotificationSystem
    {
        /// <summary>
        /// This is a delegate that takes in a single Item as the parameter.
        /// </summary>
        public delegate void NotificationEvent(GameItem gameItem);

        private static Dictionary<NotificationType, NotificationEvent> m_NotificationEvents = new Dictionary<NotificationType, NotificationEvent>();

        private static bool m_TemporaryDisable = false;

        internal static bool temporaryDisable
        {
            get { return m_TemporaryDisable; }
            set { m_TemporaryDisable = value; }
        }
        /// <summary>
        /// Registers the given method to be invoked whenever the given notification type is fired.
        /// </summary>
        /// <param name="type">The type of notification to register the action under.</param>
        /// <param name="action">The method to invoke when notified.</param>
        internal static void RegisterNotification(NotificationType type, NotificationEvent action)
        {
            if (m_NotificationEvents.ContainsKey(type))
            {
                m_NotificationEvents[type] += action;
            }
            else
            {
                m_NotificationEvents.Add(type, action);
            }
        }
        
        /// <summary>
        /// Removes the given method from the notification system.
        /// </summary>
        /// <param name="type">The notification type the method was registered under.</param>
        /// <param name="action">The method to be removed.</param>
        internal static void UnRegisterNotification(NotificationType type, NotificationEvent action)
        {
            if (m_NotificationEvents.ContainsKey(type))
            {
                m_NotificationEvents[type] -= action;
            }
        }

        /// <summary>
        /// Invokes the given notification type with the given game item.
        /// </summary>
        /// <param name="type">The notification type to fire.</param>
        /// <param name="gameItem">The game item to pass along to the notification</param>
        internal static void FireNotification(NotificationType type, GameItem gameItem)
        {
            if (m_TemporaryDisable)
            {
                return;
            }

            m_NotificationEvents.TryGetValue(type, out var notificationEvent);
            if (notificationEvent != null)
            {
                m_NotificationEvents[type].Invoke(gameItem);
            }
        }
    }
}
