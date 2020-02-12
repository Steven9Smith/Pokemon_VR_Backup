using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Module for reference definition selection UI.
    /// </summary>
    internal static class ReferenceDefinitionPickerEditor
    {
        internal static void DrawReferenceDefinitionPicker(GameItemDefinition gameItemDefinition, List<GameItemDefinition> siblingGameItemDefinitions)
        {
            if (gameItemDefinition == null)
            {
                throw new System.NullReferenceException("Cannot pass a null value for GameItemDefinition into DrawReferenceDefinitionPicker!");
            }

            if (siblingGameItemDefinitions == null)
            {
                throw new System.NullReferenceException("Cannot pass a null value for siblingGameItemDefinitions into DrawReferenceDefinitionPicker!");
            }

            // NOTE: EditorGUILayout.ObjectField is too broad, it doesn't show what asset an object is nested in
            // NOTE: EditorGUIUtility.ShowObjectPicker with a search filter is also not going to work since you can't filter by specific asset file

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Reference Definition");

                if (gameItemDefinition.referenceDefinition == null)
                {
                    EditorGUILayout.LabelField(
                        $"<color=\"{GameFoundationEditorStyles.noValueLabelColor}\">none selected</color>",
                        GameFoundationEditorStyles.richTextLabelStyle);
                }
                else
                {
                    string referenceDefinitionDisplayName = gameItemDefinition.referenceDefinition.id;

                    if (!string.IsNullOrEmpty(gameItemDefinition.referenceDefinition.displayName))
                    {
                        referenceDefinitionDisplayName = gameItemDefinition.referenceDefinition.displayName;
                    }

                    EditorGUILayout.SelectableLabel(referenceDefinitionDisplayName, GUILayout.Height(15f));
                }

                if (GUILayout.Button("Choose", EditorStyles.miniButton, GUILayout.Width(50f)))
                {
                    if (GameFoundationSettings.database.gameItemCatalog == null)
                    {
                        throw new System.NullReferenceException("There is no GameItemDefinition catalog!");
                    }

                    var gameItemDefinitions = GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinitions();

                    List<GameItemDefinition> defsAlreadyReferenced = new List<GameItemDefinition>();

                    foreach (GameItemDefinition rootGameItemDefinition in siblingGameItemDefinitions)
                    {
                        if (rootGameItemDefinition.referenceDefinition != null)
                        {
                            defsAlreadyReferenced.Add(rootGameItemDefinition.referenceDefinition);
                        }
                    }

                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(
                        new GUIContent("None"),
                        gameItemDefinition.referenceDefinition == null,
                        () => { gameItemDefinition.referenceDefinition = null; });

                    foreach (GameItemDefinition rootGameItemDefinition in gameItemDefinitions)
                    {
                        if (rootGameItemDefinition == null)
                        {
                            Debug.LogWarning("There is a null entry in the GameItemDefinition collection!");
                            continue;
                        }

                        if (gameItemDefinition.referenceDefinition != null
                            && gameItemDefinition.referenceDefinition != rootGameItemDefinition
                            && defsAlreadyReferenced.Contains(rootGameItemDefinition))
                        {
                            menu.AddDisabledItem(new GUIContent(rootGameItemDefinition.displayName));
                        }
                        else
                        {
                            bool selected = gameItemDefinition.referenceDefinition != null
                                            && gameItemDefinition.referenceDefinition.id == rootGameItemDefinition.id;

                            menu.AddItem(
                                new GUIContent(rootGameItemDefinition.displayName),
                                selected,
                                () =>
                                {
                                    gameItemDefinition.referenceDefinition = rootGameItemDefinition; EditorUtility.SetDirty(gameItemDefinition);
                                });
                        }
                    }

                    menu.ShowAsContext();
                }
            }
        }
    }
}
