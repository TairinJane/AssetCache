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
                    else objectString += line + "\n";

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

                cache.Add(key, new SceneObject(GetHash(hashString), objDict));
            }

            // WriteIdAndHash(cache);
            Console.WriteLine("obj 17640");
            cache[17640].WriteContent();
            /*foreach (var component in cache[17640].GetComponents()) {
                Console.WriteLine("comp: " + component);
            }*/

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

        private static void RunThroughDict(Dictionary<ulong, object> dict) {
            foreach (var obj in dict) {
                Console.WriteLine("key: " + obj.Key);
                foreach (var pair in (Dictionary<string, object>) obj.Value) {
                    Console.WriteLine("  " + pair.Key + " = " + pair.Value);
                }
            }
        }

        private static ulong GetObjectFileId(string line) {
            Regex pattern = new Regex(@"--- !u!\d+ &(?<id>\d+)");
            Match match = pattern.Match(line);
            string key = match.Groups["id"].Value;
            // Console.WriteLine("id = " + key);
            return UInt64.Parse(key);
        }

        private static Dictionary<string, object> ReadObject(string obj) {
            var deserializer = new DeserializerBuilder()
                .Build();
            var dict = deserializer.Deserialize<Dictionary<string, object>>(obj);
            return dict;
        }

        private void Component(Dictionary<string, object> dict) {
            var list = (List<object>) dict["m_Component"];
            var newList = new List<string>();
            foreach (var str in list) {
                Console.WriteLine(str);
                foreach (var pair in (Dictionary<object, object>) str) {
                    Console.WriteLine("  " + pair.Key + " " + pair.Value);
                    var fileId = (Dictionary<object, object>) pair.Value;
                    foreach (var id in fileId) {
                        Console.WriteLine("    " + id.Key + " " + id.Value);
                        newList.Add((string) id.Value);
                    }
                }
            }

            dict["m_Component"] = newList;
            Console.WriteLine(dict["m_Component"].GetType());
            foreach (var str in (List<string>) dict["m_Component"]) {
                Console.WriteLine("newList: " + str);
            }
        }
    }
}