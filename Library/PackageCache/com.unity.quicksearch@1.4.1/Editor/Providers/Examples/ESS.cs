// #define QUICKSEARCH_EXAMPLES
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class ESS
        {
            internal static string type = "ess";
            internal static string displayName = "Source Search";
            internal static string ess_exe = @"C:\Program Files (x86)\Entrian Source Search\ess.exe";

            internal static bool s_BuildingIndex = true;

            internal static string indexPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/ess.index"));
            internal static Regex essRx = new Regex(@"([^(]+)\((\d+)\):\s*(.*)");

            struct ESSMatchInfo
            {
                public string path;
                public int lineNumber;
                public string content;
            }

            struct RunResult
            {
                public int code;
                public string output;
                public Exception exception;
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchItemProvider]
            #endif
            internal static SearchProvider CreateProvider()
            {
                if (!File.Exists(ess_exe))
                    return null;

                return new SearchProvider(type, displayName)
                {
                    active = false, // Still experimental
                    priority = 7000,
                    filterId = "ess:",
                    fetchItems = (context, items, provider) => SearchEntries(context, provider),

                    fetchThumbnail = (item, context) =>
                    {
                        if (item.data == null)
                            return null;

                        var essmi = (ESSMatchInfo)item.data;
                        return (item.thumbnail = UnityEditorInternal.InternalEditorUtility.FindIconForFile(essmi.path));
                    }
                };
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchActionsProvider]
            #endif
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new[]
                {
                    new SearchAction(type, "reveal", null, "Locate statement...")
                    {
                        handler = (item, context) =>
                        {
                            var essmi = (ESSMatchInfo)item.data;
                            #if UNITY_2019_3_OR_NEWER
                            CodeEditor.CodeEditor.CurrentEditor.OpenProject(essmi.path, essmi.lineNumber);
                            #elif UNITY_2019_2_OR_NEWER
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(essmi.path, essmi.lineNumber, -1);
                            #else
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(essmi.path, essmi.lineNumber);
                            #endif
                        }
                    }
                };
            }

            private static RunResult RunESS(params string[] args)
            {
                var result = new RunResult { code = -1 };

                try
                {
                    var essProcess = CreateESSProcess(args);

                    essProcess.OutputDataReceived += (sender, log) => result.output += log.Data + "\n";
                    essProcess.Start();
                    essProcess.BeginOutputReadLine();

                    essProcess.WaitForExit();

                    result.output = result.output.Trim();
                    result.code = essProcess.ExitCode;
                }
                catch (Exception e)
                {
                    result.exception = e;
                }

                return result;
            }

            private static Process CreateESSProcess(params string[] args)
            {
                var essProcess = new Process
                {
                    StartInfo =
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = ess_exe,
                        Arguments = String.Join(" ", args)
                    },

                    EnableRaisingEvents = true
                };

                return essProcess;
            }

            private static string ParamValueString(string param, string value)
            {
                return $"-{param}=\"{value}\"";
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, UnityEditor.InitializeOnLoadMethod]
            #endif
            private static void BuildIndex()
            {
                if (!File.Exists(ess_exe))
                    return;

                var localIndexPath = indexPath;
                var localDataPath = Application.dataPath;
                var thread = new Thread(() =>
                {
                    var result = new RunResult { code = 0 };
                    // Create index if not exists
                    if (!Directory.Exists(localIndexPath))
                        result = RunESS("create", ParamValueString("index", localIndexPath), ParamValueString("root", localDataPath),
                                        ParamValueString("include", "*.cs,*.txt,*.uss,*.asmdef,*.shader,*.json"),
                                        ParamValueString("exclude", "*.meta"));

                    if (result.code != 0)
                    {
                        UnityEngine.Debug.LogError($"[{result.code}] Failed to create ESS index at {localIndexPath}\n\n" + result.output);
                        if (result.exception != null)
                            UnityEngine.Debug.LogException(result.exception);
                        return;
                    }

                    // Update index
                    if (RunESS("update", ParamValueString("index", localIndexPath)).code != 0)
                        result = RunESS("check", ParamValueString("index", localIndexPath), "-fix");

                    if (result.code != 0)
                    {
                        UnityEngine.Debug.LogError($"[{result.code}] Failed fix the ESS index at {localIndexPath}\n\n" + result.output);
                        if (result.exception != null)
                            UnityEngine.Debug.LogException(result.exception);
                        return;
                    }

                    //UnityEngine.Debug.Log("ESS index ready");
                    s_BuildingIndex = false;
                });
                thread.Start();
            }

            private static SearchItem ProcessLine(string line, string searchQuery, SearchProvider provider)
            {
                line = line.Trim();

                var m = essRx.Match(line);
                var filePath = m.Groups[1].Value.Replace("\\", "/");
                var essmi = new ESSMatchInfo
                {
                    path = filePath.Replace(Application.dataPath, "Assets").Replace("\\", "/"),
                    lineNumber = int.Parse(m.Groups[2].Value),
                    content = m.Groups[3].Value
                };
                var fsq = searchQuery.Replace("*", "");
                var content = Regex.Replace(essmi.content, fsq, "<color=#FFFF00>" + fsq + "</color>", RegexOptions.IgnoreCase);
                var description = $"{essmi.path} (<b>{essmi.lineNumber}</b>)";
                return provider.CreateItem(essmi.content.GetHashCode().ToString(), content, description, null, essmi);
            }

            private static IEnumerable<SearchItem> SearchEntries(SearchContext context, SearchProvider provider)
            {
                var localSearchQuery = context.searchQuery;

                while (s_BuildingIndex)
                {
                    yield return null;
                }

                Process essProcess;
                var lines = new List<string>();
                //using (new DebugTimer("ProcessStart"))
                {
                    try
                    {

                        essProcess = CreateESSProcess("search", ParamValueString("index", indexPath), localSearchQuery);
                        essProcess.OutputDataReceived += (sender, log) =>
                        {
                            lock (lines)
                            {
                                lines.Add(log.Data);
                            }
                        };
                        essProcess.Start();
                        essProcess.BeginOutputReadLine();

                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                        yield break;
                    }
                }

                while (!essProcess.WaitForExit(1))
                {
                    // Copy the collection so it does not get modified during enumeration
                    string[] linesCopy;
                    lock (lines)
                    {
                        linesCopy = lines.ToArray();
                        lines.Clear();
                    }

                    foreach (var searchItem in SearchLines(provider, linesCopy, localSearchQuery))
                        yield return searchItem;
                    yield return null;
                }

                foreach (var searchItem in SearchLines(provider, lines, localSearchQuery))
                    yield return searchItem;
            }

            private static IEnumerable<SearchItem> SearchLines(SearchProvider provider, IEnumerable<string> lines, string searchQuery)
            {
                foreach (var l in lines)
                {
                    if (l == null)
                    {
                        yield return null;
                        continue;
                    }

                    yield return ProcessLine(l, searchQuery, provider);
                }
            }
        }
    }
}