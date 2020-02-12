using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// This class contains methods that help with working with details and detail definitions in general.
    /// </summary>
    internal static class DetailHelper
    {
        private static Dictionary<string, System.Type> m_DetailDefinitionInfo;

        /// <summary>
        /// A list of all classes that inherit from BaseDetailDefinition. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, System.Type> detailDefinitionInfo
        {
            get { return m_DetailDefinitionInfo; }
        }

        /// <summary>
        /// Refreshes (or creates) a static list of all classes that inherit from BaseDetailDefinition.
        /// </summary>
        public static void RefreshTypeDict()
        {
            m_DetailDefinitionInfo = new Dictionary<string, System.Type>();

            var baseType = typeof(BaseDetailDefinition);
            var assembly = baseType.Assembly;

            var detailDefinitionList = assembly
                .GetTypes()
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && baseType.IsAssignableFrom(t)
                    );

            foreach (System.Type t in detailDefinitionList)
            {
                BaseDetailDefinition inst = (BaseDetailDefinition)ScriptableObject.CreateInstance(t.ToString());
                m_DetailDefinitionInfo.Add(inst.DisplayName(), t);
            }
        }
    }
}
