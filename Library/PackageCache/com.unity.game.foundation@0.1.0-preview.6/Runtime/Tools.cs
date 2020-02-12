using System.Text.RegularExpressions;
using System.IO; 

namespace UnityEngine.GameFoundation
{
    internal static class Tools
    {
        /// <summary>
        /// Handy function for converting a string value to a unique Hash.
        /// Right now we are just hijacking the Animator's StringToHash but down the road we'll make our own implementation.
        /// </summary>
        /// <param name="value">The string value to Hash.</param>
        /// <returns>The unique int Hash of value.</returns>
        public static int StringToHash(string value)
        {
            return Animator.StringToHash(value);
        }

        /// <summary>
        /// Checks to see if the argument is a valid Id. Valid Ids are alphanumeric with optional dashes or underscores.
        /// No whitespace is permitted
        /// </summary>
        /// <param name="id">id to check</param>
        /// <returns>whether Id is valid or not</returns>
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[\w-_]+$");
        }
        
        /// <summary>
        /// Helper method for making sure the application is not in play mode.
        /// This will mainly be used to make sure users aren't modifying definitions in play mode.
        /// </summary>
        /// <param name="errorMessage">The error message to display if we are in play mode.</param>
        /// <exception cref="System.Exception">Thrown when in play mode with the given error message.</exception>
        public static void ThrowIfPlayMode(string errorMessage)
        {
            if (Application.isPlaying)
            {
                throw new System.Exception(errorMessage);
            }
        }

        public static void DeleteRuntimeData()
        {
            string gameFoundationPersistencePath = $"{Application.persistentDataPath}/" + GameFoundation.k_GameFoundationPersistenceId;
            string gameFoundationPersistenceBackupPath = $"{Application.persistentDataPath}/" + GameFoundation.k_GameFoundationPersistenceId + "_backup";

            bool isFileDeleted = false;
            
            if (File.Exists(gameFoundationPersistencePath))
            {
                File.Delete(gameFoundationPersistencePath);
                isFileDeleted = true;
            }
            
            if (File.Exists(gameFoundationPersistenceBackupPath))
            {
                File.Delete(gameFoundationPersistenceBackupPath);
                isFileDeleted = true;
            }

            if (isFileDeleted)
            {
                Debug.Log("Local persistence data is deleted.");
            }
            else
            {
                Debug.Log("There is no data to delete.");
            }
        }
    }
}
