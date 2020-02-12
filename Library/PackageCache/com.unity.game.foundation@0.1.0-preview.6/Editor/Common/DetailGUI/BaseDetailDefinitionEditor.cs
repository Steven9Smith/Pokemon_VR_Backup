namespace UnityEditor.GameFoundation
{
    public class BaseDetailDefinitionEditor : Editor
    {
        protected virtual string[] m_ExcludedFields => new [] { "m_Script", "m_Owner" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                DrawPropertiesExcluding(serializedObject, m_ExcludedFields);

                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
