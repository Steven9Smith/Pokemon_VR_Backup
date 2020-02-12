using System.IO;
using System;
using System.Text;

namespace UnityEngine.GameFoundation.DataPersistence
{
    public class LocalPersistence : BaseDataPersistence
    {
        /// <inheritdoc />
        public LocalPersistence(IDataSerializer serializer) : base(serializer)
        {
        }

        /// <inheritdoc />
        public override void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action onSaveFailed = null)
        {
            SaveFile(identifier, content, onSaveCompleted, onSaveFailed);
        }

        //We need to extract that code from the Save() because it will be used in the child but the child need to override the Save method sometimes
        //So to not rewrite the same code I have done a function with it
        private void SaveFile(string identifier, ISerializableData content, Action onSaveFileCompleted, Action onSaveFileFailed)
        {
            string pathMain = $"{Application.persistentDataPath}/{identifier}";
            string pathBackup = $"{Application.persistentDataPath}/{identifier + "_backup"}";

            try
            {
                WriteFile(pathBackup, content);
                File.Copy(pathBackup, pathMain, true);
            }
            catch
            {
                onSaveFileFailed?.Invoke();
                return;
            }

            onSaveFileCompleted?.Invoke();
        }

        /// <inheritdoc />
        public override void Load<T>(string identifier, Action<ISerializableData> onLoadCompleted = null, Action onLoadFailed = null)
        {
            string path;
            string pathMain = $"{Application.persistentDataPath}/{identifier}";
            string pathBackup = $"{Application.persistentDataPath}/{identifier + "_backup"}";

            //If the main file doesn't exist we check for backup
            if (System.IO.File.Exists(pathMain))
            {
                path = pathMain;
            }
            else if (System.IO.File.Exists(pathBackup))
            {
                path = pathBackup;
            }
            else
            {
                onLoadFailed?.Invoke();
                return;
            }

            var strData = "";
            try
            {
                strData = ReadFile(path);
            }
            catch
            {
                onLoadFailed?.Invoke();
                return;
            }

            var data = DeserializeString<T>(strData);
            onLoadCompleted?.Invoke(data);
        }

        private void WriteFile(string path, ISerializableData content)
        {
            using (var sw = new StreamWriter(path, false, Encoding.Default))
            {
                var data = SerializeString(content);
                sw.Write(data);
            }
        }

        private static string ReadFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            var str = "";
            
            using (StreamReader sr = new StreamReader(fileInfo.OpenRead(), Encoding.Default))
            {
                str = sr.ReadToEnd();
            }

            return str;
        }

        private static bool DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("DeleteFile: delete failed." + e);
            }

            return false;
        }

        private string SerializeString(object o)
        {
            return serializer.Serialize(o, true);
        }

        private T DeserializeString<T>(string value) where T : ISerializableData
        {
            return serializer.Deserialize<T>(value, true);
        }
    }
}