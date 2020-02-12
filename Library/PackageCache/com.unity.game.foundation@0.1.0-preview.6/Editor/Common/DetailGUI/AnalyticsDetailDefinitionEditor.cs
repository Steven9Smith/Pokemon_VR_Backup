using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(AnalyticsDetailDefinition))]
    internal class AnalyticsDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("This detail will automatically track Analytics data for any GameItem it is attached to.");
        }
    }
}