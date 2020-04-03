using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace AssetCache {
    public class AssetCache : IAssetCache {
        private Dictionary<ulong, SceneObject> cache;
        private string assetPath;

        private readonly Regex stringFieldPattern = new Regex(@"(?<key>\w+): {(?<value>.+)}");
        private readonly Regex fileIDPattern = new Regex(@"- {fileID: (?<id>\d+)}");
        private const string COMPONENT_KEY = "m_Component";
        private const string CHILDREN_KEY = "m_Children";

        public object Build(string path, Action interruptChecker) {
            var fileStream = File.OpenText(path);
            var cacheDict = new Dictionary<ulong, SceneObject>();

            fileStream.ReadLine();
            fileStream.ReadLine();

            var line = fileStream.ReadLine();

            while (line != null) {
                var key = GetObjectFileId(line);
                fileStream.ReadLine();
                var objectString = "";
                var hashString = "";
                var stringsDict = new Dictionary<string, string>();
                var componentSet = new HashSet<ulong>();
                var childrenSet = new HashSet<ulong>();

                while ((line = fileStream.ReadLine()) != null && !line.StartsWith("---")) {
                    var match = stringFieldPattern.Match(line);
                    if (match.Success) {
                        var fieldKey = match.Groups["key"].Value;
                        if (fieldKey.Equals("component")) {
                            componentSet.Add(Convert.ToUInt64(match.Groups["value"].Value.Split()[1]));
                        }
                        else stringsDict.Add(fieldKey, match.Groups["value"].Value);
                    }
                    else {
                        match = fileIDPattern.Match(line);
                        if (match.Success) {
                            childrenSet.Add(Convert.ToUInt64(match.Groups["id"].Value));
                        }
                        else objectString += line + "\n";
                    }

                    hashString += line;
                }

                var objDict = ReadObject(objectString);

                foreach (var pair in stringsDict) {
                    objDict.Add(pair.Key, pair.Value);
                }

                if (objDict.ContainsKey(COMPONENT_KEY)) {
                    objDict[COMPONENT_KEY] = componentSet;
                }

                if (objDict.ContainsKey(CHILDREN_KEY)) {
                    objDict[CHILDREN_KEY] = childrenSet;
                }

                cacheDict.Add(key, new SceneObject(GetHash(hashString), objDict));
            }

            return cacheDict;
        }

        public void Merge(string path, object result) {
            var resultDict = (Dictionary<ulong, SceneObject>) result;
            if (path != assetPath) {
                assetPath = path;
                cache = resultDict;
                return;
            }

            var updateKeys = cache.Keys.Intersect(resultDict.Keys);
            var removeKeys = cache.Keys.Except(resultDict.Keys);
            var addKeys = resultDict.Keys.Except(cache.Keys);

            foreach (var key in removeKeys) {
                cache.Remove(key);
            }

            foreach (var key in addKeys) {
                cache.Add(key, resultDict[key]);
            }

            foreach (var key in updateKeys) {
                if (cache[key].Hash != resultDict[key].Hash) {
                    cache[key] = resultDict[key];
                }
            }
        }

        public int GetLocalAnchorUsages(ulong anchor) {
            var count = 0;
            foreach (var obj in cache.Values) {
                count += obj.GetLocalAnchorUsages(anchor);
            }

            return count;
        }

        public int GetGuidUsages(string guid) {
            var count = 0;
            foreach (var obj in cache.Values) {
                count += obj.GetGuidUsages(guid);
            }

            return count;
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor) {
            return cache[gameObjectAnchor].GetComponents();
        }

        private static string GetHash(string input) {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        private static ulong GetObjectFileId(string line) {
            var pattern = new Regex(@"--- !u!\d+ &(?<id>\d+)");
            var match = pattern.Match(line);
            if (!match.Success) throw new ArgumentException("No id in " + line);
            var key = match.Groups["id"].Value;
            return Convert.ToUInt64(key);
        }

        private static Dictionary<string, object> ReadObject(string obj) {
            var deserializer = new DeserializerBuilder().Build();
            var dict = deserializer.Deserialize<Dictionary<string, object>>(obj);
            return dict;
        }
    }
}