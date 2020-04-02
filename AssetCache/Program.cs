using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace AssetCache {
    internal class Program {
        public static void Main(string[] args) {
            var fs = File.OpenText("F:\\jopa\\stuff\\SampleScene\\SceneLittle.txt");
            var cacheDict = new Dictionary<ulong, SceneObject>();
            var stringFieldPattern = new Regex(@"(?<key>\w+): {(?<value>.+)}");
            var fileIDPattern = new Regex(@"- {fileID: (?<id>\d+)}");
            var componentKey = "m_Component";
            var childrenKey = "m_Children";

            //two first lines
            Console.WriteLine(fs.ReadLine());
            Console.WriteLine(fs.ReadLine());

            var line = fs.ReadLine();

            while (line != null) {
                var key = GetObjectFileId(line);
                fs.ReadLine();
                var objectString = "";
                var hashString = "";
                var stringsDict = new Dictionary<string, string>();
                var componentList = new List<ulong>();
                var childrenList = new List<ulong>();

                while ((line = fs.ReadLine()) != null && !line.StartsWith("---")) {
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

            var cache = new Cache(cacheDict);
            Console.WriteLine("guid 8a53381b50169634491102a6508752e1: " +
                              cache.GetGuidUsages("8a53381b50169634491102a6508752e1"));
            Console.WriteLine("components for 17640:");
            foreach (var component in cache.GetComponentsFor(17640)) {
                Console.WriteLine("comp: " + component);
            }

            Console.WriteLine("local anchor 241: " + cache.GetLocalAnchorUsages(241));

            Console.WriteLine("END");
        }

        private static string GetHash(string input) {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        private static void WriteIdAndHash(Dictionary<ulong, SceneObject> dict) {
            foreach (var pair in dict) {
                Console.WriteLine(pair.Key + " " + pair.Value.Hash);
            }
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
    }
}