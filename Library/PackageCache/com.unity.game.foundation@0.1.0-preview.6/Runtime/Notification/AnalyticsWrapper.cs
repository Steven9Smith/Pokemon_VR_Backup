using System.Collections.Generic;
using UnityEngine.Analytics;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Game Foundation wrapper class for Unity Analytics
    /// </summary>
    internal class AnalyticsWrapper
    {
        private static bool m_Initialized = false;

        /// <summary>
        /// This binds the firing methods to the notification system.
        /// </summary>
        internal static bool Initialize()
        {
            if (m_Initialized)
            {
                Debug.LogWarning("AnalyticsWrapper is already initialized and cannot be initialized again.");
                return false;
            }
            
            if (!VerifyAnalyticsEnabled())
            {
                return false;
            }
            
            NotificationSystem.RegisterNotification(NotificationType.Created, SendItemCreated);
            NotificationSystem.RegisterNotification(NotificationType.Destroyed, SendItemDestroyed);
            NotificationSystem.RegisterNotification(NotificationType.Modified, SendItemModified);
            
            m_Initialized = true;

            return true;
        }

        internal static void Uninitialize()
        {
            if (!m_Initialized)
            {
                return;
            }

            NotificationSystem.UnRegisterNotification(NotificationType.Created, SendItemCreated);
            NotificationSystem.UnRegisterNotification(NotificationType.Destroyed, SendItemDestroyed);
            NotificationSystem.UnRegisterNotification(NotificationType.Modified, SendItemModified);

            m_Initialized = false;
        }

        private static AnalyticsDetailDefinition GetAnalyticsDetail(GameItem gameItem)
        {
            if (gameItem == null || gameItem.definition == null)
            {
                return null;
            }
            
            return gameItem.definition.GetDetailDefinition<AnalyticsDetailDefinition>();
        }

        private static bool VerifyAnalyticsEnabled()
        {
#if UNITY_ANALYTICS
            if (!Application.isPlaying && GameFoundationSettings.enableEditorModeAnalytics)
            {
                return true;
            }

            if (GameFoundationSettings.enablePlayModeAnalytics)
            {
                return true;
            }
#endif
            return false;
        }

        private static void SendCustomGameItemEvent(string eventName, GameItem gameItem)
        {
#if UNITY_ANALYTICS
            if (!Analytics.Analytics.enabled || !m_Initialized || GetAnalyticsDetail(gameItem) == null)
            {
                return;
            }
            
            int quantity = 0;
            string inventoryOwner = "None";
            bool hasQuantity = false;
            if (gameItem.GetType() == typeof(InventoryItem))
            {
                var inventoryItem = (InventoryItem)(gameItem);
                quantity = inventoryItem.quantity;
                hasQuantity = true;

                if (inventoryItem.inventory != null)
                {
                    inventoryOwner = inventoryItem.inventory.displayName;
                }
            }

            string currencyType = "NonCurrency";
            var currencyDetail = gameItem.definition.GetDetailDefinition<CurrencyDetailDefinition>();
            if (currencyDetail != null)
            {
                currencyType = currencyDetail.currencyType.ToString();
            }
            
            AnalyticsEvent.Custom(eventName, new Dictionary<string, object>
            {
                { "id", gameItem.id },
                { "quantity", hasQuantity ? quantity.ToString() : "-" },
                { "currencyType", currencyType },
                { "owner", inventoryOwner }
            });
#endif
        }
        
        /// <summary>
        /// Triggered on Created notifications.
        /// </summary>
        /// <param name="gameItem">The game item that was created.</param>
        private static void SendItemCreated(GameItem gameItem)
        {
            SendCustomGameItemEvent("gameitem_created", gameItem);
        }
        
        /// <summary>
        /// Triggered on Destroyed notifications
        /// </summary>
        /// <param name="gameItem">The game item that was destroyed.</param>
        private static void SendItemDestroyed(GameItem gameItem)
        {
            SendCustomGameItemEvent("gameitem_destroyed", gameItem);
        }
        
        /// <summary>
        /// Triggered on Modified notifications
        /// </summary>
        /// <param name="gameItem">The game item that was modified.</param>
        private static void SendItemModified(GameItem gameItem)
        {
            SendCustomGameItemEvent("gameitem_modified", gameItem);
        }
    }
}
