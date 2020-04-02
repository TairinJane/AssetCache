﻿using System;
using System.Collections.Generic;
 using System.IO;
 using System.Security.Cryptography;
 using System.Text;
 using System.Text.RegularExpressions;
 using YamlDotNet.Serialization;

 namespace AssetCache {
    public class AssetCache : IAssetCache{
        private Cache cache;
        private string assetPath;
        
        private readonly Regex stringFieldPattern = new Regex(@"(?<key>\w+): {(?<value>.+)}");
        private readonly Regex fileIDPattern = new Regex(@"- {fileID: (?<id>\d+)}");
        private const string componentKey = "m_Component";
        private const string childrenKey = "m_Children";
        public object Build(string path, Action interruptChecker) {
            var fileStream = File.OpenText(path);
            var cacheDict = new Dictionary<ulong, SceneObject>();

            //two first lines
            fileStream.ReadLine();
            fileStream.ReadLine();

            var line = fileStream.ReadLine();

            while (line != null) {
                var key = GetObjectFileId(line);
                fileStream.ReadLine();
                var objectString = "";
                var hashString = "";
                var stringsDict = new Dictionary<string, string>();
                var componentList = new List<ulong>();
                var childrenList = new List<ulong>();

                while ((line = fileStream.ReadLine()) != null && !line.StartsWith("---")) {
                    var match = stringFieldPattern.Match(line);
                    if (match.Success) {
                        var fieldKey = match.Groups["key"].Value;
                        if (fieldKey.Equals("component")) {
                            componentList.Add(Convert.ToUInt64(match.Groups["value"].Value.Split()[1]));
                        }
                        else stringsDict.Add(fieldKey, match.Groups["value"].Value);
                    }
                    else {
                        match = fileIDPattern.Match(line);
                        if (match.Success) {
                            childrenList.Add(Convert.ToUInt64(match.Groups["id"].Value));
                        }
                        else objectString += line + "\n";
                    }

                    hashString += line;
                }

                var objDict = ReadObject(objectString);

                foreach (var pair in stringsDict) {
                    objDict.Add(pair.Key, pair.Value);
                }

                if (objDict.ContainsKey(componentKey)) {
                    objDict[componentKey] = componentList;
                }

                if (objDict.ContainsKey(childrenKey)) {
                    objDict[childrenKey] = childrenList;
                }

                cacheDict.Add(key, new SceneObject(GetHash(hashString), objDict));
            }

            return new Cache(cacheDict);
        }
        
        private static string GetHash(string input) {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }
        
        private static ulong GetObjectFileId(string line) {
            var pattern = new Regex(@"--- !u!\d+ &(?<id>\d+)");
            var match = pattern.Match(line);
            if (match.Success) {
                var key = match.Groups["id"].Value;
                return Convert.ToUInt64(key);
            }

            throw new ArgumentException("No id in string: " + line);
        }

        private static Dictionary<string, object> ReadObject(string obj) {
            var deserializer = new DeserializerBuilder()
                .Build();
            var dict = deserializer.Deserialize<Dictionary<string, object>>(obj);
            return dict;
        }

        public void Merge(string path, object result) {
            cache = (Cache) result;
        }

        public int GetLocalAnchorUsages(ulong anchor) {
            return cache.GetLocalAnchorUsages(anchor);
        }

        public int GetGuidUsages(string guid) {
            return cache.GetGuidUsages(guid);
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor) {
            return cache.GetComponentsFor(gameObjectAnchor);
        }
    }
}