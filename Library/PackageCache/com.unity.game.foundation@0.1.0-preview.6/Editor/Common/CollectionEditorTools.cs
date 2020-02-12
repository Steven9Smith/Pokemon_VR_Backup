using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Collection Editor Tools class provides cross-class editor tools for the
    /// creation/display/rule enforcing of different aspects of game foundation systems.
    /// </summary>
    internal static class CollectionEditorTools
    {
        /// <summary>
        /// Converts a given display name to a definition Id which follows variable naming rules.
        /// Conversions it makes:
        /// 1. Removes special characters with the exception of underscore
        /// 2. Removes any non English-alphabet characters at the beginning of the string, until it gets to an alphabet character
        /// 3. Converts to camelCase
        /// 4. Removes white space
        /// </summary>
        /// <param name="displayName">The string value entered in by the user as a display name. All types
        /// of characters are possible for this value.</param>
        /// <returns>A "cleaned" version of the display name, without special characters (underscores being
        /// the exception), with the first character being a letter, no spaces, and camelCase format.</returns>
        public static string ConvertNameToId(string displayName)
        {
            string convertedId = displayName;

            if (convertedId != null)
            {
            // Remove special characters
            // Regex matches anything not A-Z, a-z 0-9, underscore or space
            // and replaces with an empty string
            Regex noSpecialCharacters = new Regex(@"[^\w\s_]", RegexOptions.ECMAScript);
            convertedId = noSpecialCharacters.Replace(convertedId, "");

            // Remove any numbers, spaces or underscores as the first character of the string
            // Regex captures beginning of string (any character that is not a US letter)(until it reaches a US letter)
            // the replace removes everything in the first capturing group, leaving only what's in the second capturing group
            convertedId = Regex.Replace(convertedId, @"^([^A-z]*)([A-z]*)", "$2");

            // Convert first letter of string to lowercase
            // Regex matches the start of the string followed by a capital letter
            // and replaces it with lower case version of the same letter
            convertedId = Regex.Replace(convertedId, @"^[A-Z]", m => m.ToString().ToLower());


            // Convert to camel case
            // Regex finds any not newline followed by a word boundary followed by a lower case letter,
            // then replaces it with the capital version of the letter
            convertedId = Regex.Replace(convertedId, @"[^\n](\b[a-z])", m => m.ToString().ToUpper());

            // Remove white space
            // Removes any space character grouped in 1 or more occurences and replaces with an empty string
            convertedId = Regex.Replace(convertedId, @"\s+", "");
            }

            return convertedId;
        }

        /// <summary>
        /// This method checks the given definitionId against a list of existing Ids, making sure that the given
        /// one doesn't conflict with any existing ones.
        /// If it does conflict, it will add a number to the end corresponding to when there is no longer a conflict.
        /// </summary>
        /// <param name="definitionId">The definition Id that needs to be checked to see if it exists yet.</param>
        /// <param name="existingIds">The list of existing objects to check their Ids against the new definitonId.</param>
        /// <returns>The definition Id that is not a duplicate, either the original one passed in, or one with a valid number at the end.</returns>
        public static string DeDuplicateNewId(string definitionId, HashSet<string> existingIds)
        {
            string testId = definitionId;

            if (existingIds != null && existingIds.Count > 0)
            {
                int i = 0;
                bool conflict = existingIds.Contains(testId);
                while (conflict)
                {
                    i++;
                    testId = definitionId + i;
                    conflict = existingIds.Contains(testId);
                }
            }

            return testId;
        }

        /// <summary>
        /// Helper method which calls <see cref="CollectionEditorTools.ConvertNameToId(string)"/> followed by <see cref="CollectionEditorTools.DeDuplicateNewId(string, HashSet<string>)"/>
        /// </summary>
        /// <param name="displayName">The string value entered in by the user as a display name. All types
        /// of characters are possible for this value.</param>
        /// <param name="existingIds">The list of existing objects to check their Ids against the new definitonId.</param>
        /// <returns>The definition Id that is not a duplicate, either the original one passed in, or one with a valid number at the end.</returns>
        public static string CraftUniqueId(string displayName, HashSet<string> existingIds)
        {
            string newId = ConvertNameToId(displayName);
            return DeDuplicateNewId(newId, existingIds);
        }

        /// <summary>
        /// Helper method to create an path with a uniquely incremented index at the end"/>
        /// </summary>
        /// <param name="filePath">The path being checked for uniquness. Expects the format "Assets/Resources/{NameOfFile}.asset"</param>
        /// <returns>A new string with the full, unique path.</returns>
        public static string CreateUniqueCatalogPath(string filePath)
        {
            int index = 0;
            string testPath = filePath;
            int insertIndex = filePath.LastIndexOf(".asset");
            
            if (insertIndex < 0)
            {
                throw new System.ArgumentException(string.Format("Cannot create a unique catalog path with given filepath argument, {0}. Filepath must end in .asset", filePath));
            }

            while (File.Exists(Path.Combine(Application.dataPath, testPath.Substring(7))))
            {
                index++;
                testPath = filePath.Insert(insertIndex, index.ToString());
            }

            return testPath;
        }

        /// <summary>
        /// Helper method for adding an object to a ScriptableObject asset in the Asset Database.
        /// </summary>
        /// <param name="objectToAdd">Object to be added to the asset.</param>
        /// <param name="asset">Asset to which the object should be added.</param>
        public static void AssetDatabaseAddObject(GameItemDefinition objectToAdd, ScriptableObject asset)
        {
            AssetDatabase.AddObjectToAsset(objectToAdd, asset);
            AssetDatabaseUpdate();
        }

        /// <summary>
        /// Helper method for removing an object from an asset in the Asset Database.
        /// </summary>
        /// <param name="objectToRemove">Object to be removed from its asset.</param>
        public static void AssetDatabaseRemoveObject(GameItemDefinition objectToRemove)
        {
            AssetDatabase.RemoveObjectFromAsset(objectToRemove);
            AssetDatabaseUpdate();
        }

        /// <summary>
        /// Helper method for updating the Asset Database. Calls AssetDatabase.SaveAssets() and AssetDatabase.Refresh().
        /// </summary>
        public static void AssetDatabaseUpdate()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Show the user a dialog that confirms whether they want to discard the current progress of creating a new item.
        /// </summary>
        /// <returns>True if we should cancel the current item creation, otherwise false.</returns>
        public static bool ConfirmDiscardingNewItem()
        {
            return EditorUtility.DisplayDialog(
                "New Item In Progress",
                "You are creating a new item. Are you sure you want to discard the new item?",
                "Discard",
                "Stay");
        }

        /// <summary>
        /// Sets GUI.enabled to whatever value is being passed in only if Application.isPlaying is false.
        /// </summary>
        /// <param name="enabled">Boolean for whether or not to enable the GUI.</param>
        public static void SetGUIEnabledAtEditorTime(bool enabled)
        {
            if (Application.isPlaying)
            {
                return;
            }

            GUI.enabled = enabled;
        }

        /// <summary>
        /// Sets GUI.enabled to whatever value is being passed in only if Application.isPlaying is true.
        /// </summary>
        /// <param name="enabled">Boolean for whether or not to enable the GUI.</param>
        public static void SetGUIEnabledAtRunTime(bool enabled)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            GUI.enabled = enabled;
        }
        
        /// <summary>
        /// Checks to see if the argument is a valid Id. Valid Ids are alphanumeric with optional dashes or underscores.
        /// No whitespace or empty strings are permitted
        /// </summary>
        /// <param name="id">id to check</param>
        /// <returns>whether Id is valid or not</returns>
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[\w-_]+$");
        }
    }
}
