using System.IO;

namespace Unity.Build
{
    internal class Package
    {
        internal static T LoadResource<T>(string folder, string name) where T : UnityEngine.Object
        {
            var resourcePath = GetResourcePath(folder, name);
            if (!File.Exists(resourcePath))
            {
                UnityEngine.Debug.LogError($"{typeof(T).Name} resource {resourcePath.ToHyperLink()} not found.");
                return null;
            }

            var resource = UnityEditor.EditorGUIUtility.Load(resourcePath);
            if (resource == null || !resource)
            {
                UnityEngine.Debug.LogError($"Failed to load {typeof(T).Name} resource {resourcePath.ToHyperLink()}.");
                return null;
            }

            if (resource is T typed)
            {
                return typed;
            }

            UnityEngine.Debug.LogError($"Resource {resourcePath.ToHyperLink()} is not a {typeof(T).Name}.");
            return null;
        }

        static string GetResourcePath(string folder, string name)
        {
            return Path.Combine(Constants.PackagePath, folder, name).Replace('\\', '/');
        }
    }
}
