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
            StreamReader fs = File.OpenText("F:\\jopa\\stuff\\SampleScene\\SceneLittle.txt");
            Dictionary<ulong, SceneObject> cache = new Dictionary<ulong, SceneObject>();
            Regex stringFieldPattern = new Regex(@"(?<key>\w+): {(?<value>.+)}");
            Regex fileIDPattern = new Regex(@"- {fileID: (?<id>\d+)}");
            string componentKey = "m_Component";

            //two first lines
            Console.WriteLine(fs.ReadLine());
            Console.WriteLine(fs.ReadLine());

            string line = fs.ReadLine();

            while (line != null) {
                // Console.WriteLine(line);
                var key = GetObjectFileId(line);
                fs.ReadLine();
                string objectString = "";
                string hashString = "";
                // Console.WriteLine("obj Name: " + fs.ReadLine());
                var stringsDict = new Dictionary<string, string>();
                var componentList = new List<ulong>();
                var childrenList = new List<ulong>();

                while ((line = fs.ReadLine()) != null && !line.StartsWith("---")) {
                    Match match = stringFieldPattern.Match(line);
                    if (match.Success) {
                        string fieldKey = match.Groups["key"].Value;
                        if (fieldKey.Equals("component")) {
                            // Console.WriteLine("match: " + fieldKey + " = " +  match.Groups["value"].Value);
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
                /*Console.WriteLine(key);
                foreach (var pair in objDict) {
                    Console.WriteLine(pair.Key + ": " + pair.Value);
                }*/

                foreach (var pair in stringsDict) {
                    objDict.Add(pair.Key, pair.Value);
                }

                if (objDict.ContainsKey(componentKey)) {
                    objDict[componentKey] = componentList;
                }

                if (objDict.ContainsKey("m_Children")) {
                    objDict["m_Children"] = childrenList;
                }

                cache.Add(key, new SceneObject(GetHash(hashString), objDict));
            }
            
            Console.WriteLine("obj 242");
            cache[242].WriteContent();
            foreach (var component in cache[242].GetChildren()) {
                Console.WriteLine("comp: " + component);
            }

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
            Regex pattern = new Regex(@"--- !u!\d+ &(?<id>\d+)");
            Match match = pattern.Match(line);
            string key = match.Groups["id"].Value;
            // Console.WriteLine("id = " + key);
            return Convert.ToUInt64(key);
        }

        private static Dictionary<string, object> ReadObject(string obj) {
            var deserializer = new DeserializerBuilder()
                .Build();
            var dict = deserializer.Deserialize<Dictionary<string, object>>(obj);
            return dict;
        }
    }
}